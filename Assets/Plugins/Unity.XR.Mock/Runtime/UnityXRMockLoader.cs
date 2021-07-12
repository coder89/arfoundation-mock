using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.Mock
{
    public class UnityXRMockLoader : XRLoaderHelper
    {
        private static List<XRSessionSubsystemDescriptor> s_SessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();
        private static List<XRCameraSubsystemDescriptor> s_CameraSubsystemDescriptors = new List<XRCameraSubsystemDescriptor>();
        private static List<XRDepthSubsystemDescriptor> s_DepthSubsystemDescriptors = new List<XRDepthSubsystemDescriptor>();
        private static List<XROcclusionSubsystemDescriptor> s_OcclusionSubsystemDescriptors = new List<XROcclusionSubsystemDescriptor>();
        private static List<XRPlaneSubsystemDescriptor> s_PlaneSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();
        private static List<XRAnchorSubsystemDescriptor> s_AnchorSubsystemDescriptors = new List<XRAnchorSubsystemDescriptor>();
        private static List<XRRaycastSubsystemDescriptor> s_RaycastSubsystemDescriptors = new List<XRRaycastSubsystemDescriptor>();

        public static bool IsPreferred { get; set; } = false;

        public XRSessionSubsystem sessionSubsystem => this.GetLoadedSubsystem<XRSessionSubsystem>();
        public XRCameraSubsystem cameraSubsystem => this.GetLoadedSubsystem<XRCameraSubsystem>();
        public XRDepthSubsystem depthSubsystem => this.GetLoadedSubsystem<XRDepthSubsystem>();
        public XROcclusionSubsystem occlusionSubsystem => this.GetLoadedSubsystem<XROcclusionSubsystem>();
        public XRPlaneSubsystem planeSubsystem => this.GetLoadedSubsystem<XRPlaneSubsystem>();
        public XRAnchorSubsystem anchorSubsystem => this.GetLoadedSubsystem<XRAnchorSubsystem>();
        public XRRaycastSubsystem raycastSubsystem => this.GetLoadedSubsystem<XRRaycastSubsystem>();

        public override bool Initialize()
        {
            if (sessionSubsystem != null)
            { return true; }

            Debug.unityLogger.Log("ar-mock", "Initializing UnityXRMock.");

#if !UNITY_EDITOR
            if (XRGeneralSettings.Instance?.Manager?.activeLoaders?.Count > 1 && !IsPreferred) { return false; }
#endif

            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionSubsystemDescriptors, typeof(UnityXRMockSessionSubsystem).FullName);
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(s_CameraSubsystemDescriptors, typeof(UnityXRMockCameraSubsystem).FullName);
            CreateSubsystem<XRDepthSubsystemDescriptor, XRDepthSubsystem>(s_DepthSubsystemDescriptors, typeof(UnityXRMockDepthSubsystem).FullName);
            CreateSubsystem<XROcclusionSubsystemDescriptor, XROcclusionSubsystem>(s_OcclusionSubsystemDescriptors, typeof(UnityXRMockOcclusionSubsystem).FullName);
            CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneSubsystemDescriptors, typeof(UnityXRMockPlaneSubsystem).FullName);
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(s_AnchorSubsystemDescriptors, typeof(UnityXRMockAnchorSubsystem).FullName);
            CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastSubsystemDescriptors, typeof(UnityXRMockRaycastSubsystem).FullName);

            if (sessionSubsystem == null)
            {
                Debug.unityLogger.LogError("ar-mock", "Failed to load session subsystem.");
            }

            return sessionSubsystem != null;
        }

        public override bool Start()
        {
            Debug.unityLogger.Log("ar-mock", "Starting UnityXRMock.");

            var settings = GetSettings();
            if (settings != null && settings.startAndStopSubsystems)
            {
                StartSubsystem<XRSessionSubsystem>();
                StartSubsystem<XRCameraSubsystem>();
                StartSubsystem<XRDepthSubsystem>();
                StartSubsystem<XROcclusionSubsystem>();
                StartSubsystem<XRPlaneSubsystem>();
                StartSubsystem<XRAnchorSubsystem>();
                StartSubsystem<XRRaycastSubsystem>();
            }

            return base.Start();
        }

        public override bool Stop()
        {
            Debug.unityLogger.Log("ar-mock", "Stopping UnityXRMock.");

            var settings = GetSettings();
            if (settings != null && settings.startAndStopSubsystems)
            {
                StopSubsystem<XRRaycastSubsystem>();
                StopSubsystem<XRAnchorSubsystem>();
                StopSubsystem<XRPlaneSubsystem>();
                StopSubsystem<XROcclusionSubsystem>();
                StopSubsystem<XRDepthSubsystem>();
                StopSubsystem<XRCameraSubsystem>();
                StopSubsystem<XRSessionSubsystem>();
            }

            return base.Stop();
        }

        public override bool Deinitialize()
        {
            Debug.unityLogger.Log("ar-mock", "Deinitializing UnityXRMock.");

            DestroySubsystem<XRRaycastSubsystem>();
            DestroySubsystem<XRAnchorSubsystem>();
            DestroySubsystem<XRPlaneSubsystem>();
            DestroySubsystem<XROcclusionSubsystem>();
            DestroySubsystem<XRDepthSubsystem>();
            DestroySubsystem<XRCameraSubsystem>();
            DestroySubsystem<XRSessionSubsystem>();

            return base.Deinitialize();
        }

        UnityXRMockLoaderSettings GetSettings()
        {
            UnityXRMockLoaderSettings settings = null;
            // When running in the Unity Editor, we have to load user's customization of configuration data directly from
            // EditorBuildSettings. At runtime, we need to grab it from the static instance field instead.
#if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(UnityXRMockLoaderConstants.k_SettingsKey, out settings);
#else
            settings = UnityXRMockLoaderSettings.s_RuntimeInstance;
#endif
            return settings;
        }
    }
}