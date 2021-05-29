using System;
using System.Linq;
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

            public override void Stop() { }

            public override void Destroy()
            {
                DepthApi.reset();
            }

            public override TrackableChanges<XRPointCloud> GetChanges(XRPointCloud defaultPointCloud, Allocator allocator)
            {
                try
                {
                    return TrackableChanges<XRPointCloud>.CopyFrom(
                        new NativeArray<XRPointCloud>(
                            DepthApi.added.Select(m => m.ToPointCloud(defaultPointCloud)).ToArray(), allocator),
                        new NativeArray<XRPointCloud>(
                            DepthApi.updated.Select(m => m.ToPointCloud(defaultPointCloud)).ToArray(), allocator),
                        new NativeArray<TrackableId>(
                            DepthApi.removed.Select(m => m.trackableId).ToArray(), allocator),
                        allocator);
                }
                finally
                {
                    DepthApi.consumedChanges();
                }
            }

            public override XRPointCloudData GetPointCloudData(TrackableId trackableId, Allocator allocator)
            {
                var tmp = DepthApi.datas.FirstOrDefault(m => m.Key.trackableId == trackableId);
                return (tmp.Key == null
                    ? new XRPointCloudData()
                    : tmp.Value.ToPointCloudData());
            }
        }
    }
}
