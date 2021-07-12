using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class DepthApi
    {
        public static readonly object stateLock = new object();
        private static readonly Dictionary<TrackableId, DepthInfo> all = new Dictionary<TrackableId, DepthInfo>();
        private static readonly List<DepthInfo> added = new List<DepthInfo>();
        private static readonly List<DepthInfo> updated = new List<DepthInfo>();
        private static readonly List<DepthInfo> removed = new List<DepthInfo>();
        private static readonly Dictionary<DepthInfo, DepthData> datas = new Dictionary<DepthInfo, DepthData>();

        public static TrackableId Add(Pose pose, TrackingState trackingState)
        {
            lock (stateLock)
            {
                var result = NativeApi.NewTrackableId();
                Add(result, pose, trackingState);
                return result;
            }
        }

        public static void Add(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            lock (stateLock)
            {
                var tmp = new DepthInfo()
                {
                    trackableId = trackableId,
                    pose = pose,
                    trackingState = trackingState
                };

                all[trackableId] = tmp;
                added.Add(tmp);
            }
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            lock (stateLock)
            {
                if (all.TryGetValue(trackableId, out var tmp))
                {
                    tmp.pose = pose;
                    tmp.trackingState = trackingState;

                    if (!added.Contains(tmp) && !updated.Contains(tmp))
                    {
                        updated.Add(tmp);
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown trackableID");
                }
            }
        }

        public static void Remove(TrackableId trackableId)
        {
            lock (stateLock)
            {
                if (all.TryGetValue(trackableId, out var tmp))
                {
                    all.Remove(trackableId);
                    added.Remove(tmp);
                    updated.Remove(tmp);
                    removed.Add(tmp);
                    datas.Remove(tmp);
                }
                else
                {
                    throw new ArgumentException("Unknown trackableID");
                }
            }
        }

        public static void RemoveAll()
        {
            lock (stateLock)
            {
                added.Clear();
                updated.Clear();
                datas.Clear();

                foreach (var p in all)
                {
                    if (!removed.Contains(p.Value))
                        removed.Add(p.Value);
                }

                all.Clear();
            }
        }

        public static void SetDepthData(TrackableId trackableId, Vector3[] positions, float[] confidenceValues = null, ulong[] identifiers = null)
        {
            lock (stateLock)
            {
                if (all.TryGetValue(trackableId, out var tmp))
                {
                    if (positions == null)
                        throw new ArgumentNullException("positions");

                    if (confidenceValues != null && positions.Length != confidenceValues.Length)
                        throw new ArgumentException("confidenceValues must be the same length as positions");

                    if (identifiers != null && positions.Length != identifiers.Length)
                        throw new ArgumentException("positions must be the same length as positions");

                    if (!datas.TryGetValue(tmp, out var data))
                    {
                        data = new DepthData();
                        datas[tmp] = data;
                    }

                    data.positions = positions;
                    data.confidenceValues = confidenceValues;
                    data.identifiers = identifiers;

                    if (!added.Contains(tmp) && !updated.Contains(tmp))
                    {
                        updated.Add(tmp);
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown trackableID");
                }
            }
        }

        public static XRPointCloudData GetDepthData(TrackableId trackableId, Allocator allocator)
        {
            lock (stateLock)
            {
                var tmp = DepthApi.datas.FirstOrDefault(m => m.Key.trackableId == trackableId);
                return (tmp.Key == null
                    ? new XRPointCloudData()
                    : tmp.Value.ToPointCloudData(allocator));
            }
        }

        public static TrackableChanges<XRPointCloud> ConsumeChanges(XRPointCloud defaultPointCloud, Allocator allocator)
        {
            lock (stateLock)
            {
                try
                {
                    if (allocator != Allocator.None)
                    {
                        T[] EfficientArray<T>(IEnumerable<DepthApi.DepthInfo> collection, Func<DepthApi.DepthInfo, T> converter)
                            => collection.Any(m => true) ? collection.Select(converter).ToArray() : Array.Empty<T>();

                        return TrackableChanges<XRPointCloud>.CopyFrom(
                            new NativeArray<XRPointCloud>(
                                EfficientArray(DepthApi.added, m => m.ToPointCloud(defaultPointCloud)), allocator),
                            new NativeArray<XRPointCloud>(
                                EfficientArray(DepthApi.updated, m => m.ToPointCloud(defaultPointCloud)), allocator),
                            new NativeArray<TrackableId>(
                                EfficientArray(DepthApi.removed, m => m.trackableId), allocator),
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
                datas.Clear();
            }
        }

        public class DepthInfo
        {
            public TrackableId trackableId;
            public Pose pose;
            public TrackingState trackingState;

            public XRPointCloud ToPointCloud(XRPointCloud defaultPointCloud) => new XRPointCloud(this.trackableId, this.pose, this.trackingState, IntPtr.Zero);
        }

        public class DepthData
        {
            public Vector3[] positions;
            public float[] confidenceValues;
            public ulong[] identifiers;

            public XRPointCloudData ToPointCloudData(Allocator allocator)
            {
                var result = new XRPointCloudData();

                if (positions != null)
                {
                    result.positions = new NativeArray<Vector3>(positions, allocator);
                }

                if (confidenceValues != null)
                {
                    result.confidenceValues = new NativeArray<float>(confidenceValues, allocator);
                }

                if (identifiers != null)
                {
                    result.identifiers = new NativeArray<ulong>(identifiers, allocator);
                }

                return result;
            }
        }
    }
}
