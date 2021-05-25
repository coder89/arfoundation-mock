using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class AnchorApi
    {
        public static TrackableId Attach(Pose pose)
        {
            var trackableId = NativeApi.NewTrackableId();
            return NativeApi.UnityXRMock_attachAnchor(trackableId, pose, TrackingState.Tracking, Guid.Empty);
        }

        public static void Attach(TrackableId trackableId, Pose pose, TrackingState trackingState, Guid sessionId)
        {
            NativeApi.UnityXRMock_attachAnchor(trackableId, pose, trackingState, sessionId);
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            NativeApi.UnityXRMock_updateAnchor(trackableId, pose, trackingState);
        }

        public static void Remove(TrackableId trackableId)
        {
            NativeApi.UnityXRMock_removeAnchor(trackableId);
        }
    }
}
