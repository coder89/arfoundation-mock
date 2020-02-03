using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.Mock
{
    public class UnityXRMockLoader : XRLoaderHelper
    {
        private static List<XRSessionSubsystemDescriptor> s_SessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();
        private static List<XRCameraSubsystemDescriptor> s_CameraSubsystemDescriptors = new List<XRCameraSubsystemDescriptor>();
        private static List<XRAnchorSubsystemDescriptor> s_AnchorSubsystemDescriptors = new List<XRAnchorSubsystemDescriptor>();
        private static List<XRPlaneSubsystemDescriptor> s_PlaneSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();
        private static List<XRRaycastSubsystemDescriptor> s_RaycastSubsystemDescriptors = new List<XRRaycastSubsystemDescriptor>();

        public static bool IsActive { get; set; } = false;

        public XRSessionSubsystem sessionSubsystem => this.GetLoadedSubsystem<XRSessionSubsystem>();
        public XRCameraSubsystem cameraSubsystem => this.GetLoadedSubsystem<XRCameraSubsystem>();
        public XRAnchorSubsystem anchorSubsystem => this.GetLoadedSubsystem<XRAnchorSubsystem>();
        public XRPlaneSubsystem planeSubsystem => this.GetLoadedSubsystem<XRPlaneSubsystem>();
        public XRRaycastSubsystem raycastSubsystem => this.GetLoadedSubsystem<XRRaycastSubsystem>();

        public override bool Initialize()
        {
#if !UNITY_EDITOR
            if (!IsActive) { return false; }
#endif

            Debug.Log("Initializing UnityXRMock.");

            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionSubsystemDescriptors, UnityXRMockSessionSubsystem.ID);
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(s_CameraSubsystemDescriptors, UnityXRMockCameraSubsystem.ID);
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(s_AnchorSubsystemDescriptors, UnityXRMockAnchorSubsystem.ID);
            CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneSubsystemDescriptors, UnityXRMockPlaneSubsystem.ID);
            CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastSubsystemDescriptors, UnityXRMockRaycastSubsytem.ID);

            if (sessionSubsystem == null)
            {
                Debug.LogError("Failed to load session subsystem.");
            }

            return sessionSubsystem != null;
        }

        public override bool Start()
        {
            var settings = GetSettings();
            if (settings != null && settings.startAndStopSubsystems)
            {
                StartSubsystem<XRSessionSubsystem>();
                StartSubsystem<XRCameraSubsystem>();
                StartSubsystem<XRAnchorSubsystem>();
                StartSubsystem<XRPlaneSubsystem>();
                StartSubsystem<XRRaycastSubsystem>();
            }

            return true;
        }

        public override bool Stop()
        {
            var settings = GetSettings();
            if (settings != null && settings.startAndStopSubsystems)
            {
                StopSubsystem<XRRaycastSubsystem>();
                StopSubsystem<XRPlaneSubsystem>();
                StopSubsystem<XRAnchorSubsystem>();
                StopSubsystem<XRCameraSubsystem>();
                StopSubsystem<XRSessionSubsystem>();
            }

            return true;
        }

        public override bool Deinitialize()
        {
            Debug.Log("Deinitializing UnityXRMock.");

            DestroySubsystem<XRRaycastSubsystem>();
            DestroySubsystem<XRPlaneSubsystem>();
            DestroySubsystem<XRAnchorSubsystem>();
            DestroySubsystem<XRCameraSubsystem>();
            DestroySubsystem<XRSessionSubsystem>();
            return true;
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
