using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockPlaneSubsystem : XRPlaneSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = typeof(UnityXRMockPlaneSubsystem).FullName,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockPlaneSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = true,
                supportsBoundaryVertices = true,
                supportsClassification = true
            });
        }

        private class MockProvider : Provider
        {
            private PlaneDetectionMode _currentPlaneDetectionMode;

            [Preserve]
            public MockProvider() { }

            public override void Start() { }

            public override void Destroy()
            {
                PlaneApi.Reset();
            }

            public override void Stop() { }

            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get => this._currentPlaneDetectionMode;
                set => this._currentPlaneDetectionMode = value;
            }

            public override PlaneDetectionMode currentPlaneDetectionMode => this._currentPlaneDetectionMode;

            public override void GetBoundary(TrackableId trackableId, Allocator allocator, ref NativeArray<Vector2> boundary)
            {
                if (PlaneApi.TryGetPlaneData(trackableId, out Vector2[] boundaryPoints))
                {
                    CreateOrResizeNativeArrayIfNecessary(boundaryPoints.Length, allocator, ref boundary);
                    boundary.CopyFrom(boundaryPoints);
                }
                else if (boundary.IsCreated)
                {
                    boundary.Dispose();
                }
            }

            public override TrackableChanges<BoundedPlane> GetChanges(BoundedPlane defaultPlane, Allocator allocator)
                => PlaneApi.ConsumeChanges(defaultPlane, allocator);
        }
    }
}
