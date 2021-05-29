using System;
using System.Collections.Generic;
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
                    T[] EfficientArray<T>(IEnumerable<DepthApi.DepthInfo> collection, Func<DepthApi.DepthInfo, T> converter)
                        => collection.Any(m => true) ? collection.Select(converter).ToArray() : Array.Empty<T>();

                    return TrackableChanges<XRPointCloud>.CopyFrom(
                        new NativeArray<XRPointCloud>(
                            EfficientArray(DepthApi.added, m => m.ToPointCloud(defaultPointCloud)), allocator),
                        new NativeArray<XRPointCloud>(
                            EfficientArray(DepthApi.updated, m => m.ToPointCloud(defaultPointCloud)), allocator),
                        new NativeArray<TrackableId>(
                            EfficientArray(DepthApi.removed, m => m.trackableId), allocator),
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
