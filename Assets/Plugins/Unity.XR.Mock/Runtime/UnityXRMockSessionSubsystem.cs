using System;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockSessionSubsystem : XRSessionSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = typeof(UnityXRMockSessionSubsystem).FullName,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            });
        }

        private class MockProvider : Provider
        {
            private bool isPaused = false;

            [Preserve]
            public MockProvider() { }

            public override Promise<SessionInstallationStatus> InstallAsync() => new SessionInstallationPromise();

            public override Promise<SessionAvailability> GetAvailabilityAsync() => new SessionAvailabilityPromise();

            public override void Start()
            {
                NativeApi.UnityXRMock_setTrackingState(TrackingState.Tracking);

                base.Start();
            }

            public override void Stop()
            {
                NativeApi.UnityXRMock_setTrackingState(TrackingState.None);

                base.Stop();
            }

            //public override void Update(XRSessionUpdateParams updateParams)
            //{
            //    if (this.trackingState == TrackingState.Limited && !this.isPaused)
            //    {
            //        NativeApi.UnityXRMock_setTrackingState(TrackingState.Tracking);
            //    }
            //
            //    base.Update(updateParams);
            //}

            public override void OnApplicationPause()
            {
                this.isPaused = true;
                NativeApi.UnityXRMock_setTrackingState(TrackingState.None);

                base.OnApplicationPause();
            }

            public override void OnApplicationResume()
            {
                NativeApi.UnityXRMock_setTrackingState(TrackingState.Tracking);
                this.isPaused = false;

                base.OnApplicationResume();
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
