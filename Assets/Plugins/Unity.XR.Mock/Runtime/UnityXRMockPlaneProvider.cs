using System;
using System.Linq;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockPlaneProvider : XRPlaneSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        class Provider : IProvider
        {
            public override void Start() { }

            public override void Stop() { }

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

            public override void Destroy() { }
        }

        internal static void RegisterDescriptor(XRPlaneSubsystemDescriptor descriptor, Func<XRPlaneSubsystem, TrackableId, TrackingState> getTrackingState)
        {
            if (descriptor != null)
            {
                descriptor.subsystemImplementationType = typeof(UnityXRMockPlaneProvider);
            }
            else
            {
                XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
                {
                    id = "UnityXRMock-Plane",
                    subsystemImplementationType = typeof(UnityXRMockPlaneProvider),
                    supportsHorizontalPlaneDetection = true,
                    supportsVerticalPlaneDetection = true,
                    supportsArbitraryPlaneDetection = true,
                    supportsBoundaryVertices = true
                });
            }
        }
    }
}
