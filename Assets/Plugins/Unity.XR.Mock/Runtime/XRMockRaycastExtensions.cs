using System.Collections.Generic;
using UnityEngine.Experimental;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    internal static class XRMockRaycastExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            List<XRRaycastSubsystemDescriptor> raycastSubsystemDescriptors = new List<XRRaycastSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(raycastSubsystemDescriptors);
            if (raycastSubsystemDescriptors.Count > 0)
            {
                UnityXRMockRaycastSubsytem.RegisterDescriptor(raycastSubsystemDescriptors[0]);
            }
            else
            {
                UnityXRMockRaycastSubsytem.RegisterDescriptor(null);
            }
        }
    }
}
