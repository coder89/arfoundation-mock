using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockSessionSubsystem : XRSessionSubsystem
    {
        #region Constants

        public const string ID = "UnityXRMock-Session";

        #endregion

        #region Fields

        private bool isInitialized;
        private XRSessionSubsystem wrappedSubsystem;
        private static XRSessionSubsystemDescriptor originalDescriptor;

        #endregion

        #region Constructors

        public UnityXRMockSessionSubsystem()
        {
            this.Initialize();
        }

        #endregion

        #region XRSessionSubsystem

        public override void Start()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Start();
            }

            base.Start();
        }

        public override void Stop()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Stop();
            }

            base.Stop();
        }

        //public override void Destroy()
        //{
        //    if (this.wrappedSubsystem != null)
        //    {
        //        this.wrappedSubsystem.Destroy();
        //    }
        //
        //    base.Destroy();
        //}

        protected override IProvider CreateProvider()
        {
            this.Initialize();
            return this.wrappedSubsystem?.GetType()
                                         .GetMethod(nameof(CreateProvider), BindingFlags.NonPublic | BindingFlags.Instance)
                                         .Invoke(this.wrappedSubsystem, null) as IProvider ?? new Provider();
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

        private static void RegisterDescriptor(XRSessionSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                // Clone descriptor
                var cinfo = new XRSessionSubsystemDescriptor.Cinfo
                {
                    id = overrideDescriptor.id,
                    subsystemImplementationType = overrideDescriptor.subsystemImplementationType,
                    supportsInstall = overrideDescriptor.supportsInstall
                };

                originalDescriptor = typeof(XRSessionSubsystemDescriptor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                                                                         .Invoke(new object[] { cinfo }) as XRSessionSubsystemDescriptor;

                // Override subsystem
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockSessionSubsystem);
            }
            else
            {
                XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
                {
                    id = ID,
                    subsystemImplementationType = typeof(UnityXRMockSessionSubsystem),
                    supportsInstall = false
                });
            }
        }

        private static XRSessionSubsystemDescriptor GetSubsystemDescriptor()
        {
            List<XRSessionSubsystemDescriptor> descriptors = new List<XRSessionSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            return descriptors.FirstOrDefault(d => d.id != ID);
        }

        #endregion

        #region Types

        private class Provider : IProvider
        {
            private bool isPaused = false;

            public Provider() { }

            public override Promise<SessionInstallationStatus> InstallAsync() => new SessionInstallationPromise();

            public override Promise<SessionAvailability> GetAvailabilityAsync() => new SessionAvailabilityPromise();

            public override void Update(XRSessionUpdateParams updateParams)
            {
                if (this.trackingState == TrackingState.Limited && !this.isPaused)
                {
                    NativeApi.UnityXRMock_setTrackingState(TrackingState.Tracking);
                }
            }

            public override void OnApplicationPause()
            {
                this.isPaused = true;
                NativeApi.UnityXRMock_setTrackingState(TrackingState.Limited);
            }

            public override void OnApplicationResume()
            {
                this.isPaused = false;
            }

            public override IntPtr nativePtr => IntPtr.Zero;

            public override TrackingState trackingState => NativeApi.UnityXRMock_getTrackingState();
        }

        private class SessionInstallationPromise : Promise<SessionInstallationStatus>
        {
            public SessionInstallationPromise()
            {
                this.Resolve(SessionInstallationStatus.Success);
            }

            public override bool keepWaiting => false;

            protected override void OnKeepWaiting() { }
        }

        private class SessionAvailabilityPromise : Promise<SessionAvailability>
        {
            public SessionAvailabilityPromise()
            {
                this.Resolve(SessionAvailability.Supported | SessionAvailability.Installed);
            }

            public override bool keepWaiting => false;

            protected override void OnKeepWaiting() { }
        }

        #endregion
    }
}
