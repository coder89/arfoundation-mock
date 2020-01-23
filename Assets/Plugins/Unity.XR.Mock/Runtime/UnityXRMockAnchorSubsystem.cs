using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockAnchorSubsystem : XRAnchorSubsystem
    {
        #region Constants

        public const string ID = "UnityXRMock-Anchor";

        #endregion

        #region Fields

        private bool isInitialized;
        private XRAnchorSubsystem wrappedSubsystem;
        private static XRAnchorSubsystemDescriptor originalDescriptor;

        #endregion

        #region Constructors

        public UnityXRMockAnchorSubsystem()
        {
            this.Initialize();
        }

        #endregion

        #region XRAnchorSubsystem

        public override TrackableChanges<XRAnchor> GetChanges(Allocator allocator)
        {
            if (this.wrappedSubsystem != null)
            {
                return this.wrappedSubsystem.GetChanges(allocator);
            }

            return base.GetChanges(allocator);
        }

        protected override Provider CreateProvider()
        {
            this.Initialize();
            return this.wrappedSubsystem?.GetType()
                                         .GetMethod(nameof(CreateProvider), BindingFlags.NonPublic | BindingFlags.Instance)
                                         .Invoke(this.wrappedSubsystem, null) as Provider ?? new MockProvider();
        }

        #endregion

        #region Internal methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Register()
        {
            var descriptor = GetSubsystemDescriptor();
            RegisterDescriptor(descriptor);
        }

        #endregion

        #region Private methods

        private void Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }

            if (!UnityXRMockActivator.Active)
            {
                if (originalDescriptor == null)
                {
                    originalDescriptor = GetSubsystemDescriptor();
                }

                this.wrappedSubsystem = originalDescriptor?.Create();
            }

            this.isInitialized = true;
        }

        private static void RegisterDescriptor(XRAnchorSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                // Clone descriptor
                var cinfo = new XRAnchorSubsystemDescriptor.Cinfo
                {
                    id = overrideDescriptor.id,
                    subsystemImplementationType = overrideDescriptor.subsystemImplementationType,
                    supportsTrackableAttachments = overrideDescriptor.supportsTrackableAttachments
                };

                originalDescriptor = typeof(XRAnchorSubsystemDescriptor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                                                                                .Invoke(new object[] { cinfo }) as XRAnchorSubsystemDescriptor;

                // Override subsystem
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockAnchorSubsystem);
            }
            else
            {
                XRAnchorSubsystemDescriptor.Create(new XRAnchorSubsystemDescriptor.Cinfo
                {
                    id = ID,
                    subsystemImplementationType = typeof(UnityXRMockAnchorSubsystem),
                    supportsTrackableAttachments = true
                });
            }
        }

        private static XRAnchorSubsystemDescriptor GetSubsystemDescriptor()
        {
            List<XRAnchorSubsystemDescriptor> descriptors = new List<XRAnchorSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            return descriptors.FirstOrDefault(d => d.id != ID);
        }

        #endregion

        #region Types

        private class MockProvider : Provider
        {
            public override void Destroy()
            {
                NativeApi.UnityXRMock_anchorReset();
            }

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

        #endregion
    }
}
