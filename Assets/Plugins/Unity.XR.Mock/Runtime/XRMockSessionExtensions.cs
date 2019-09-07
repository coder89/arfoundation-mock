using System.Collections.Generic;
using UnityEngine.Experimental;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    internal static class XRMockSessionExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            List<XRSessionSubsystemDescriptor> sessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(sessionSubsystemDescriptors);
            if (sessionSubsystemDescriptors.Count > 0)
            {
                UnityXRMockSessionSubsystem.RegisterDescriptor(sessionSubsystemDescriptors[0]);
            }
            else
            {
                UnityXRMockSessionSubsystem.RegisterDescriptor();
            }
        }
    }
}
