//using UnityEngine.XR.ARExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine.Experimental;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockReferencePointSubsystem : XRReferencePointSubsystem
    {
        #region Constants

        public const string ID = "UnityXRMock-ReferencePoint";

        #endregion

        #region Fields

        private bool isInitialized;
        private XRReferencePointSubsystem wrappedSubsystem;
        private static XRReferencePointSubsystemDescriptor originalDescriptor;

        #endregion

        #region Constructors

        public UnityXRMockReferencePointSubsystem()
        {
            this.Initialize();
        }

        #endregion

        #region XRReferencePointSubsystem

        public override void Start()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Start();
            }

            base.Start();
        }

        public override TrackableChanges<XRReferencePoint> GetChanges(Allocator allocator)
        {
            if (this.wrappedSubsystem != null)
            {
                return this.wrappedSubsystem.GetChanges(allocator);
            }

            return base.GetChanges(allocator);
        }

        public override void Stop()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Stop();
            }

            base.Stop();
        }

        public override void Destroy()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Destroy();
            }

            base.Destroy();
        }

        protected override IProvider CreateProvider()
        {
            return new Provider();
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

        private static void RegisterDescriptor(XRReferencePointSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                // Clone descriptor
                var cinfo = new XRReferencePointSubsystemDescriptor.Cinfo
                {
                    id = overrideDescriptor.id,
                    subsystemImplementationType = overrideDescriptor.subsystemImplementationType,
                    supportsTrackableAttachments = overrideDescriptor.supportsTrackableAttachments
                };

                originalDescriptor = typeof(XRReferencePointSubsystemDescriptor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                                                                                .Invoke(new object[] { cinfo }) as XRReferencePointSubsystemDescriptor;

                // Override subsystem
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockReferencePointSubsystem);
            }
            else
            {
                XRReferencePointSubsystemDescriptor.Create(new XRReferencePointSubsystemDescriptor.Cinfo
                {
                    id = ID,
                    subsystemImplementationType = typeof(UnityXRMockReferencePointSubsystem),
                    supportsTrackableAttachments = true
                });
            }
        }

        private static XRReferencePointSubsystemDescriptor GetSubsystemDescriptor()
        {
            List<XRReferencePointSubsystemDescriptor> descriptors = new List<XRReferencePointSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            return descriptors.FirstOrDefault(d => d.id != ID);
        }

        #endregion

        #region Types

        private class Provider : IProvider
        {
            public override void Start() { }

            public override void Stop() { }

            public override void Destroy()
            {
                NativeApi.UnityXRMock_referencePointReset();
            }

            public override unsafe TrackableChanges<XRReferencePoint> GetChanges(
                XRReferencePoint defaultReferencePoint,
                Allocator allocator)
            {
                try
                {
                    return TrackableChanges<XRReferencePoint>.CopyFrom(
                        new NativeArray<XRReferencePoint>(
                            NativeApi.addedRefPoints.Select(m => m.ToXRReferencePoint(defaultReferencePoint)).ToArray(), allocator),
                        new NativeArray<XRReferencePoint>(
                            NativeApi.updatedRefPoints.Select(m => m.ToXRReferencePoint(defaultReferencePoint)).ToArray(), allocator),
                        new NativeArray<TrackableId>(
                            NativeApi.removedRefPoints.Select(m => m.id).ToArray(), allocator),
                        allocator);
                }
                finally
                {
                    NativeApi.UnityXRMock_consumedReferencePointChanges();
                }
            }

            public override bool TryAddReferencePoint(
                Pose pose,
                out XRReferencePoint referencePoint)
            {
                var trackableId = NativeApi.UnityXRMock_attachReferencePoint(TrackableId.invalidId, pose);
                if (NativeApi.refPoints.TryGetValue(trackableId, out NativeApi.RefPointInfo refPointInfo))
                {
                    referencePoint = refPointInfo.ToXRReferencePoint(XRReferencePoint.GetDefault());
                    return true;
                }

                referencePoint = default;
                return false;
            }

            public override bool TryAttachReferencePoint(
                TrackableId trackableToAffix,
                Pose pose,
                out XRReferencePoint referencePoint)
            {
                return this.TryAddReferencePoint(pose, out referencePoint);
            }

            public override bool TryRemoveReferencePoint(TrackableId referencePointId)
            {
                if (NativeApi.refPoints.TryGetValue(referencePointId, out NativeApi.RefPointInfo refPointInfo))
                {
                    NativeApi.UnityXRMock_removeReferencePoint(referencePointId);
                    return true;
                }

                return false;
            }
        }

        internal static void RegisterDescriptor(XRReferencePointSubsystemDescriptor descriptor, Func<XRReferencePointSubsystem, TrackableId, Pose, TrackableId> attachReferencePoint)
        {
            if (descriptor != null)
            {
                descriptor.subsystemImplementationType = typeof(UnityXRMockReferencePointSubsystem);
            }
            else
            {
                XRReferencePointSubsystemDescriptor.Create(new XRReferencePointSubsystemDescriptor.Cinfo
                {
                    id = "UnityXRMock-ReferencePoint",
                    subsystemImplementationType = typeof(UnityXRMockReferencePointSubsystem),
                    supportsTrackableAttachments = true
                });
            }
        }

        #endregion
    }
}
