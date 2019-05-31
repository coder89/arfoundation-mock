using System;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    /// <summary>
    /// ARCore implementation of the <c>XRSessionSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class UnityXRMockSessionSubsystem : XRSessionSubsystem
    {
        /// <summary>
        /// Creates the provider interface.
        /// </summary>
        /// <returns>The provider interface for ARCore</returns>
        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        class Provider : IProvider
        {
            public Provider()
            { }

            public override void Resume() { }

            public override void Pause() { }

            public override Promise<SessionInstallationStatus> InstallAsync() => new SessionInstallationPromise();

            public override Promise<SessionAvailability> GetAvailabilityAsync() => new SessionAvailabilityPromise();

            public override void Update(XRSessionUpdateParams updateParams) { }

            public override void Destroy() { }

            public override void Reset() { }

            public override void OnApplicationPause() { }

            public override void OnApplicationResume() { }

            public override IntPtr nativePtr => IntPtr.Zero;

            public override TrackingState trackingState => NativeApi.UnityXRMock_getTrackingState();
        }

        class SessionInstallationPromise : Promise<SessionInstallationStatus>
        {
            public SessionInstallationPromise()
            {
                this.Resolve(SessionInstallationStatus.Success);
            }

            public override bool keepWaiting => false;

            protected override void OnKeepWaiting() { }
        }

        class SessionAvailabilityPromise : Promise<SessionAvailability>
        {
            public SessionAvailabilityPromise()
            {
                this.Resolve(SessionAvailability.Supported | SessionAvailability.Installed);
            }

            public override bool keepWaiting => false;

            protected override void OnKeepWaiting() { }
        }

        internal static void RegisterDescriptor(XRSessionSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockSessionSubsystem);
            }
            else
            {
                XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
                {
                    id = "UnityXRMock-Session",
                    subsystemImplementationType = typeof(UnityXRMockSessionSubsystem),
                    supportsInstall = false
                });
            }
        }
    }
}
