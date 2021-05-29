using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockAnchorSubsystem : XRAnchorSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = typeof(UnityXRMockAnchorSubsystem).FullName,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockAnchorSubsystem),
                supportsTrackableAttachments = true
            });
        }

        private class MockProvider : Provider
        {
            [Preserve]
            public MockProvider() { }

            public override void Start() { }

            public override void Destroy()
            {
                AnchorApi.Reset();
            }

            public override void Stop() { }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(
                XRAnchor defaultAnchor,
                Allocator allocator)
            {
                try
                {
                    T[] EfficientArray<T>(IEnumerable<AnchorApi.AnchorInfo> collection, Func<AnchorApi.AnchorInfo, T> converter)
                        => collection.Any(m => true) ? collection.Select(converter).ToArray() : Array.Empty<T>();

                    return TrackableChanges<XRAnchor>.CopyFrom(
                        new NativeArray<XRAnchor>(
                            EfficientArray(AnchorApi.addedAnchors, m => m.ToXRAnchor(defaultAnchor)), allocator),
                        new NativeArray<XRAnchor>(
                            EfficientArray(AnchorApi.updatedAnchors, m => m.ToXRAnchor(defaultAnchor)), allocator),
                        new NativeArray<TrackableId>(
                            EfficientArray(AnchorApi.removedAnchors, m => m.id), allocator),
                        allocator);
                }
                finally
                {
                    AnchorApi.ConsumedChanges();
                }
            }

            public override bool TryAddAnchor(
                Pose pose,
                out XRAnchor anchor)
            {
                var trackableId = AnchorApi.Attach(pose, TrackingState.Tracking, Guid.Empty);
                if (AnchorApi.anchors.TryGetValue(trackableId, out AnchorApi.AnchorInfo anchorInfo))
                {
                    anchor = anchorInfo.ToXRAnchor(XRAnchor.defaultValue);
                    return true;
                }

                anchor = default;
                return false;
            }

            public override bool TryAttachAnchor(
                TrackableId trackableToAffix,
                Pose pose,
                out XRAnchor anchor)
            {
                return this.TryAddAnchor(pose, out anchor);
            }

            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                if (AnchorApi.anchors.TryGetValue(anchorId, out AnchorApi.AnchorInfo refPointInfo))
                {
                    AnchorApi.Remove(anchorId);
                    return true;
                }

                return false;
            }
        }
    }
}
