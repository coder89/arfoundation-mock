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
                => AnchorApi.Reset();

            public override void Stop() { }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
                => AnchorApi.ConsumeChanges(defaultAnchor, allocator);

            public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                anchor = AnchorApi.Attach(pose, TrackingState.Tracking, Guid.Empty);
                return (anchor != null);
            }

            public override bool TryAttachAnchor(TrackableId trackableToAffix, Pose pose, out XRAnchor anchor)
                => this.TryAddAnchor(pose, out anchor);

            public override bool TryRemoveAnchor(TrackableId anchorId)
                => AnchorApi.Remove(anchorId);
        }
    }
}
