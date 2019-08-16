using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class ReferencePointApi
    {
        public static TrackableId Attach(Pose pose)
        {
            var trackableId = NativeApi.NewTrackableId();
            return NativeApi.UnityXRMock_attachReferencePoint(trackableId, pose);
        }

        public static void Attach(TrackableId trackableId, Pose pose)
        {
            NativeApi.UnityXRMock_attachReferencePoint(trackableId, pose);
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState = TrackingState.Tracking)
        {
            NativeApi.UnityXRMock_updateReferencePoint(trackableId, pose, trackingState);
        }

        public static void Remove(TrackableId trackableId)
        {
            NativeApi.UnityXRMock_removeReferencePoint(trackableId);
        }
    }
}
