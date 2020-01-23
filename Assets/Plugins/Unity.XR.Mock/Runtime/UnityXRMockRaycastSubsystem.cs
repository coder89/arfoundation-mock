using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public sealed class UnityXRMockRaycastSubsytem : XRRaycastSubsystem
    {
        #region Constants

        public const string ID = "UnityXRMock-Raycast";

        #endregion

        #region Fields

        private bool isInitialized;
        private XRRaycastSubsystem wrappedSubsystem;
        private static XRRaycastSubsystemDescriptor originalDescriptor;

        #endregion

        #region Constructors

        public UnityXRMockRaycastSubsytem()
        {
            this.Initialize();
        }

        #endregion

        #region XRRaycastSubsystem

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

        private static void RegisterDescriptor(XRRaycastSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                // Clone descriptor
                var cinfo = new XRRaycastSubsystemDescriptor.Cinfo
                {
                    id = overrideDescriptor.id,
                    subsystemImplementationType = overrideDescriptor.subsystemImplementationType,
                    supportedTrackableTypes = overrideDescriptor.supportedTrackableTypes,
                    supportsViewportBasedRaycast = overrideDescriptor.supportsViewportBasedRaycast,
                    supportsWorldBasedRaycast = overrideDescriptor.supportsWorldBasedRaycast
                };

                originalDescriptor = typeof(XRRaycastSubsystemDescriptor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                                                                         .Invoke(new object[] { cinfo }) as XRRaycastSubsystemDescriptor;

                // Override subsystem
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockRaycastSubsytem);
            }
            else
            {
                XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
                {
                    id = ID,
                    subsystemImplementationType = typeof(UnityXRMockRaycastSubsytem),
                    supportedTrackableTypes = TrackableType.All,
                    supportsViewportBasedRaycast = true,
                    supportsWorldBasedRaycast = true
                });
            }
        }

        private static XRRaycastSubsystemDescriptor GetSubsystemDescriptor()
        {
            List<XRRaycastSubsystemDescriptor> descriptors = new List<XRRaycastSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            return descriptors.FirstOrDefault(d => d.id != ID);
        }

        #endregion

        #region Types

        private class MockProvider : Provider
        {
            public override NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Ray ray,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                var hits = new NativeArray<XRRaycastHit>();
                return hits;
            }

            public override NativeArray<XRRaycastHit> Raycast(
                XRRaycastHit defaultRaycastHit,
                Vector2 screenPoint,
                TrackableType trackableTypeMask,
                Allocator allocator)
            {
                var hits = new NativeArray<XRRaycastHit>();
                return hits;
            }
        }

        #endregion
    }
}
