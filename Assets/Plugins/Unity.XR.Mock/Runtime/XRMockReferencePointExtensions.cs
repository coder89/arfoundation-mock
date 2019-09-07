using System.Collections.Generic;
using UnityEngine.Experimental;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    internal class MockReferencePointExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            List<XRReferencePointSubsystemDescriptor> referencePointSubsystemDescriptors = new List<XRReferencePointSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(referencePointSubsystemDescriptors);
            if (referencePointSubsystemDescriptors.Count > 0)
            {
                UnityXRMockReferencePointSubsystem.RegisterDescriptor(referencePointSubsystemDescriptors[0], AttachReferencePoint);
            }
            else
            {
                UnityXRMockReferencePointSubsystem.RegisterDescriptor(null, AttachReferencePoint);
            }
        }

        static TrackableId AttachReferencePoint(XRReferencePointSubsystem referencePointSubsystem,
            TrackableId trackableId, Pose pose)
        {
            return NativeApi.UnityXRMock_attachReferencePoint(trackableId, pose);
        }
    }
}
