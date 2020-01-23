using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class AnchorApi
    {
        public static TrackableId Attach(Pose pose)
        {
            var trackableId = NativeApi.NewTrackableId();
            return NativeApi.UnityXRMock_attachAnchor(trackableId, pose);
        }

        public static void Attach(TrackableId trackableId, Pose pose)
        {
            NativeApi.UnityXRMock_attachAnchor(trackableId, pose);
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState = TrackingState.Tracking)
        {
            NativeApi.UnityXRMock_updateAnchor(trackableId, pose, trackingState);
        }

        public static void Remove(TrackableId trackableId)
        {
            NativeApi.UnityXRMock_removeAnchor(trackableId);
        }
    }
}
