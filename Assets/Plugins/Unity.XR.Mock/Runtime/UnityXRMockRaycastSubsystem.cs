using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public sealed class UnityXRMockRaycastSubsystem : XRRaycastSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = typeof(UnityXRMockRaycastSubsystem).FullName,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockRaycastSubsystem),
                supportedTrackableTypes = TrackableType.All,
                supportsViewportBasedRaycast = true,
                supportsWorldBasedRaycast = true,
                supportsTrackedRaycasts = true
            });
        }

        private class MockProvider : Provider
        {
            [Preserve]
            public MockProvider() { }

            public override NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Ray ray,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                var hits = new NativeArray<XRRaycastHit>();
                return hits;
            }

            public override NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Vector2 screenPoint,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                var hits = new NativeArray<XRRaycastHit>();
                return hits;
            }
        }
    }
}
