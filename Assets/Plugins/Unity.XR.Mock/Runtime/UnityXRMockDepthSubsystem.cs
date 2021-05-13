using System;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockDepthSubsystem : XRDepthSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRDepthSubsystemDescriptor.RegisterDescriptor(new XRDepthSubsystemDescriptor.Cinfo
            {
                id = typeof(UnityXRMockDepthSubsystem).FullName,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockDepthSubsystem),
                supportsConfidence = false,
                supportsFeaturePoints = false,
                supportsUniqueIds = true
            });
        }

        private class MockProvider : Provider
        {
            [Preserve]
            public MockProvider() { }

            public override void Start() { }

            public override void Destroy() { }

            public override void Stop() { }

            public override TrackableChanges<XRPointCloud> GetChanges(XRPointCloud defaultPointCloud, Allocator allocator)
            {
                return TrackableChanges<XRPointCloud>.CopyFrom(
                        new NativeArray<XRPointCloud>(Array.Empty<XRPointCloud>(), allocator),
                        new NativeArray<XRPointCloud>(Array.Empty<XRPointCloud>(), allocator),
                        new NativeArray<TrackableId>(Array.Empty<TrackableId>(), allocator),
                        allocator);
            }

            public override XRPointCloudData GetPointCloudData(TrackableId trackableId, Allocator allocator)
            {
                return new XRPointCloudData()
                {

                };
            }
        }
    }
}
