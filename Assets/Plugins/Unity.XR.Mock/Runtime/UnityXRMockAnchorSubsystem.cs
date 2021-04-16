using System.Linq;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockAnchorSubsystem : XRAnchorSubsystem
    {
        public const string ID = "UnityXRMock-Anchor";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = ID,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockAnchorSubsystem),
                supportsTrackableAttachments = true
            });
        }

        private class MockProvider : Provider
        {
            public override void Start() { }

            public override void Destroy()
            {
                NativeApi.UnityXRMock_anchorReset();
            }

            public override void Stop() { }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(
                XRAnchor defaultAnchor,
                Allocator allocator)
            {
                try
                {
                    return TrackableChanges<XRAnchor>.CopyFrom(
                        new NativeArray<XRAnchor>(
                            NativeApi.addedAnchors.Select(m => m.ToXRAnchor(defaultAnchor)).ToArray(), allocator),
                        new NativeArray<XRAnchor>(
                            NativeApi.updatedAnchors.Select(m => m.ToXRAnchor(defaultAnchor)).ToArray(), allocator),
                        new NativeArray<TrackableId>(
                            NativeApi.removedAnchors.Select(m => m.id).ToArray(), allocator),
                        allocator);
                }
                finally
                {
                    NativeApi.UnityXRMock_consumedAnchorChanges();
                }
            }

            public override bool TryAddAnchor(
                Pose pose,
                out XRAnchor anchor)
            {
                var trackableId = NativeApi.UnityXRMock_attachAnchor(TrackableId.invalidId, pose);
                if (NativeApi.anchors.TryGetValue(trackableId, out NativeApi.AnchorInfo anchorInfo))
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
                if (NativeApi.anchors.TryGetValue(anchorId, out NativeApi.AnchorInfo refPointInfo))
                {
                    NativeApi.UnityXRMock_removeAnchor(anchorId);
                    return true;
                }

                return false;
            }
        }
    }
}
