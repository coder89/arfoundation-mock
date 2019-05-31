using System.Collections.Generic;
using UnityEngine.Experimental;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    internal static class MockPlaneExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            List<XRPlaneSubsystemDescriptor> planeSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(planeSubsystemDescriptors);
            if (planeSubsystemDescriptors.Count > 0)
            {
                UnityXRMockPlaneProvider.RegisterDescriptor(planeSubsystemDescriptors[0], GetTrackingState);
            }
            else
            {
                UnityXRMockPlaneProvider.RegisterDescriptor(null, GetTrackingState);
            }
        }

        static TrackingState GetTrackingState(XRPlaneSubsystem planeSubsystem, TrackableId planeId)
        {
            TrackingState trackingState;
            PlaneApi.TryGetTrackingState(planeId, out trackingState);
            return trackingState;
        }
    }
}
