using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class AnchorApi
    {
        private readonly static object stateLock = new object();
        private readonly static Dictionary<TrackableId, AnchorInfo> all = new Dictionary<TrackableId, AnchorInfo>();
        private readonly static Dictionary<TrackableId, AnchorInfo> added = new Dictionary<TrackableId, AnchorInfo>();
        private readonly static Dictionary<TrackableId, AnchorInfo> updated = new Dictionary<TrackableId, AnchorInfo>();
        private readonly static Dictionary<TrackableId, AnchorInfo> removed = new Dictionary<TrackableId, AnchorInfo>();

        public static XRAnchor Attach(Pose pose, TrackingState trackingState, Guid sessionId)
        {
            lock (stateLock)
            {
                var trackableId = NativeApi.NewTrackableId();
                AttachInternal(trackableId, pose, trackingState, sessionId);
                return all[trackableId].ToXRAnchor(XRAnchor.defaultValue);
            }
        }

        public static void Attach(TrackableId trackableId, Pose pose, TrackingState trackingState, Guid sessionId)
        {
            lock (stateLock)
            {
                AttachInternal(trackableId, pose, trackingState, sessionId);
            }
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            lock (stateLock)
            {
                var anchorInfo = all[trackableId];
                anchorInfo.pose = pose;
                anchorInfo.trackingState = trackingState;
                updated[trackableId] = anchorInfo;
            }
        }

        public static bool Remove(TrackableId trackableId)
        {
            lock (stateLock)
            {
                if (all.ContainsKey(trackableId))
                {
                    if (!added.Remove(trackableId))
                    {
                        removed[trackableId] = all[trackableId];
                    }

                    all.Remove(trackableId);
                    updated.Remove(trackableId);
                    return true;
                }

                return false;
            }
        }

        public static void RemoveAll()
        {
            lock (stateLock)
            {
                foreach (var a in all)
                {
                    removed[a.Key] = a.Value;
                }

                all.Clear();
                added.Clear();
                updated.Clear();
            }
        }

        public static TrackableChanges<XRAnchor> ConsumeChanges(XRAnchor defaultAnchor, Allocator allocator)
        {
            lock (stateLock)
            {
                try
                {
                    if (allocator != Allocator.None)
                    {
                        T[] EfficientArray<T>(IEnumerable<AnchorApi.AnchorInfo> collection, Func<AnchorApi.AnchorInfo, T> converter)
                            => collection.Any(m => true) ? collection.Select(converter).ToArray() : Array.Empty<T>();

                        return TrackableChanges<XRAnchor>.CopyFrom(
                            new NativeArray<XRAnchor>(
                                EfficientArray(AnchorApi.added.Values, m => m.ToXRAnchor(defaultAnchor)), allocator),
                            new NativeArray<XRAnchor>(
                                EfficientArray(AnchorApi.updated.Values, m => m.ToXRAnchor(defaultAnchor)), allocator),
                            new NativeArray<TrackableId>(
                                EfficientArray(AnchorApi.removed.Values, m => m.id), allocator),
                            allocator);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    added.Clear();
                    updated.Clear();
                    removed.Clear();
                }
            }
        }

        public static void Reset()
        {
            lock (stateLock)
            {
                ConsumeChanges(default, Allocator.None);
                all.Clear();
            }
        }

        private static TrackableId AttachInternal(TrackableId trackableId, Pose pose, TrackingState trackingState, Guid sessionId)
        {
            if (trackableId == TrackableId.invalidId)
            {
                trackableId = NativeApi.NewTrackableId();
            }

            if (!all.ContainsKey(trackableId) || added.ContainsKey(trackableId))
            {
                if (!all.ContainsKey(trackableId))
                {
                    all[trackableId] = new AnchorInfo()
                    {
                        id = trackableId
                    };
                }

                added[trackableId] = all[trackableId];
            }
            else
            {
                updated[trackableId] = all[trackableId];
            }

            var anchorInfo = all[trackableId];
            anchorInfo.pose = pose;
            anchorInfo.trackingState = trackingState;
            anchorInfo.sessionId = sessionId;
            return anchorInfo.id;
        }

        public class AnchorInfo
        {
            public TrackableId id;
            public Pose pose;
            public TrackingState trackingState;
            public Guid sessionId;

            public XRAnchor ToXRAnchor(XRAnchor defaultAnchor)
            {
                return new XRAnchor(this.id, this.pose, this.trackingState, defaultAnchor.nativePtr);
            }
        }
    }
}
