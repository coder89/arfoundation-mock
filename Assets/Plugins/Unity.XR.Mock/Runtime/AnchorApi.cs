using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class AnchorApi
    {
        private readonly static Dictionary<TrackableId, AnchorInfo> s_anchors = new Dictionary<TrackableId, AnchorInfo>();
        private readonly static Dictionary<TrackableId, AnchorInfo> s_addedAnchors = new Dictionary<TrackableId, AnchorInfo>();
        private readonly static Dictionary<TrackableId, AnchorInfo> s_updatedAnchors = new Dictionary<TrackableId, AnchorInfo>();
        private readonly static Dictionary<TrackableId, AnchorInfo> s_removedAnchors = new Dictionary<TrackableId, AnchorInfo>();

        public static IDictionary<TrackableId, AnchorInfo> anchors => s_anchors;
        public static IReadOnlyCollection<AnchorInfo> addedAnchors => s_addedAnchors.Values;
        public static IReadOnlyCollection<AnchorInfo> updatedAnchors => s_updatedAnchors.Values;
        public static IReadOnlyCollection<AnchorInfo> removedAnchors => s_removedAnchors.Values;

        public static TrackableId Attach(Pose pose, TrackingState trackingState, Guid sessionId)
        {
            var trackableId = NativeApi.NewTrackableId();
            return AttachInternal(trackableId, pose, TrackingState.Tracking, Guid.Empty);
        }

        public static void Attach(TrackableId trackableId, Pose pose, TrackingState trackingState, Guid sessionId)
        {
            AttachInternal(trackableId, pose, trackingState, sessionId);
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            var anchorInfo = s_anchors[trackableId];
            anchorInfo.pose = pose;
            anchorInfo.trackingState = trackingState;
            s_updatedAnchors[trackableId] = anchorInfo;
        }

        public static void Remove(TrackableId trackableId)
        {
            if (s_anchors.ContainsKey(trackableId))
            {
                if (!s_addedAnchors.Remove(trackableId))
                {
                    s_removedAnchors[trackableId] = s_anchors[trackableId];
                }

                s_anchors.Remove(trackableId);
                s_updatedAnchors.Remove(trackableId);
            }
        }

        public static void ConsumedChanges()
        {
            s_addedAnchors.Clear();
            s_updatedAnchors.Clear();
            s_removedAnchors.Clear();
        }

        public static void Reset()
        {
            ConsumedChanges();
            s_anchors.Clear();
        }

        private static TrackableId AttachInternal(TrackableId trackableId, Pose pose, TrackingState trackingState, Guid sessionId)
        {
            if (trackableId == TrackableId.invalidId)
            {
                trackableId = NativeApi.NewTrackableId();
            }

            if (!s_anchors.ContainsKey(trackableId) || s_addedAnchors.ContainsKey(trackableId))
            {
                if (!s_anchors.ContainsKey(trackableId))
                {
                    s_anchors[trackableId] = new AnchorInfo()
                    {
                        id = trackableId
                    };
                }

                s_addedAnchors[trackableId] = s_anchors[trackableId];
            }
            else
            {
                s_updatedAnchors[trackableId] = s_anchors[trackableId];
            }

            var anchorInfo = s_anchors[trackableId];
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
