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
            private TrackingState? prevTrackingState;
            private Guid m_sessionId;

            [Preserve]
            public MockProvider()
            {
                this.m_sessionId = Guid.NewGuid();
            }

            public override Guid sessionId => this.m_sessionId;

            public override Feature currentTrackingMode => Feature.AnyTrackingMode;

            public override int frameRate => Mathf.RoundToInt(1.0f / Time.deltaTime);

            public override IntPtr nativePtr => IntPtr.Zero;

            public override Feature requestedFeatures
                => Feature.AnyTrackingMode
                | Feature.AnyCamera
                | Feature.AnyLightEstimation
                | Feature.EnvironmentDepth
                | Feature.EnvironmentProbes
                | Feature.MeshClassification
                | Feature.PlaneTracking
                | Feature.PointCloud;

            public override Feature requestedTrackingMode
            {
                get => Feature.AnyTrackingMode;
                set { }
            }

            public override TrackingState trackingState => SessionApi.trackingState;

            public override Promise<SessionInstallationStatus> InstallAsync() => new SessionInstallationPromise();

            public override Promise<SessionAvailability> GetAvailabilityAsync() => new SessionAvailabilityPromise();

            public override void Start()
            {
                SessionApi.Start();
                base.Start();
            }

            public override void Stop()
            {
                SessionApi.Stop();
                base.Stop();
            }

            public override void Destroy()
            {
                SessionApi.Reset();
                base.Destroy();
            }

            public override void OnApplicationPause()
            {
                prevTrackingState = SessionApi.trackingState;
                SessionApi.trackingState = TrackingState.None;
                base.OnApplicationPause();
            }

            public override void OnApplicationResume()
            {
                SessionApi.trackingState = prevTrackingState ?? TrackingState.Tracking;
                base.OnApplicationResume();
            }
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
