using System.Linq;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockPlaneSubsystem : XRPlaneSubsystem
    {
        public const string ID = "UnityXRMock-Plane";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = ID,
                subsystemImplementationType = typeof(UnityXRMockPlaneSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = true,
                supportsBoundaryVertices = true
            });
        }

        protected override Provider CreateProvider() => new MockProvider();

        private class MockProvider : Provider
        {
            public override void Destroy()
            {
                NativeApi.UnityXRMock_planesReset();
            }

            public override void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> boundary)
            {
                if (NativeApi.planes.TryGetValue(trackableId, out NativeApi.PlaneInfo planeInfo) &&
                    planeInfo.boundaryPoints != null &&
                    planeInfo.boundaryPoints.Length > 0)
                {
                    CreateOrResizeNativeArrayIfNecessary(planeInfo.boundaryPoints.Length, allocator, ref boundary);
                    boundary.CopyFrom(planeInfo.boundaryPoints);
                }
                else if (boundary.IsCreated)
                {
                    boundary.Dispose();
                }
            }

            public override TrackableChanges<BoundedPlane> GetChanges(
                BoundedPlane defaultPlane,
                Allocator allocator)
            {
                try
                {
                    return TrackableChanges<BoundedPlane>.CopyFrom(
                        new NativeArray<BoundedPlane>(
                            NativeApi.addedPlanes.Select(m => m.ToBoundedPlane(defaultPlane)).ToArray(), allocator),
                        new NativeArray<BoundedPlane>(
                            NativeApi.updatedPlanes.Select(m => m.ToBoundedPlane(defaultPlane)).ToArray(), allocator),
                        new NativeArray<TrackableId>(
                            NativeApi.removedPlanes.Select(m => m.id).ToArray(), allocator),
                        allocator);
                }
                finally
                {
                    NativeApi.UnityXRMock_consumedPlaneChanges();
                }
            }
        }
    }
}
