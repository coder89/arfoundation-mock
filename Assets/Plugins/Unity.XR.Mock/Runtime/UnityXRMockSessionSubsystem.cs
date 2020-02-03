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
        public const string ID = "UnityXRMock-Session";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = ID,
                subsystemImplementationType = typeof(UnityXRMockSessionSubsystem),
                supportsInstall = false
            });
        }

        protected override Provider CreateProvider() => new MockProvider();

        private class MockProvider : Provider
        {
            private bool isPaused = false;

            public MockProvider() { }

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
    }
}
