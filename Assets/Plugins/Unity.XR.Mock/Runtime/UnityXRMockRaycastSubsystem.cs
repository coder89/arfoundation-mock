using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public sealed class UnityXRMockRaycastSubsytem : XRRaycastSubsystem
    {
        public const string ID = "UnityXRMock-Raycast";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
            {
                id = ID,
                subsystemImplementationType = typeof(UnityXRMockRaycastSubsytem),
                supportedTrackableTypes = TrackableType.All,
                supportsViewportBasedRaycast = true,
                supportsWorldBasedRaycast = true
            });
        }

        protected override Provider CreateProvider() => new MockProvider();

        private class MockProvider : Provider
        {
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
