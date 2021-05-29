using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class CameraApi
    {
        public static long? timestampNs;
        public static float? averageBrightness;
        public static float? averageColorTemperature;
        public static Color? colorCorrection;
        public static Matrix4x4? projectionMatrix;
        public static Matrix4x4? displayMatrix;
        public static TrackingState trackingState = TrackingState.Tracking;
        public static XRCameraFrameProperties? properties;
        public static float? averageIntensityInLumens;
        public static double? exposureDuration;
        public static float? exposureOffset;
        public static float? mainLightIntensityLumens;
        public static Color? mainLightColor;
        public static Vector3? mainLightDirection;
        public static SphericalHarmonicsL2? ambientSphericalHarmonics;
        public static XRTextureDescriptor? cameraGrain;
        public static float? noiseIntensity;

        public static bool permissionGranted { get; set; } = true;
        public static Vector2? screenSize { get; set; }
        public static ScreenOrientation? screenOrientation { get; set; }

        public static bool hasTimestamp => (properties & XRCameraFrameProperties.Timestamp) != 0;
        public static bool hasAverageBrightness => (properties & XRCameraFrameProperties.AverageBrightness) != 0;
        public static bool hasAverageColorTemperature => (properties & XRCameraFrameProperties.AverageColorTemperature) != 0;
        public static bool hasColorCorrection => (properties & XRCameraFrameProperties.ColorCorrection) != 0;
        public static bool hasProjectionMatrix => (properties & XRCameraFrameProperties.ProjectionMatrix) != 0;
        public static bool hasDisplayMatrix => (properties & XRCameraFrameProperties.DisplayMatrix) != 0;
        public static bool hasAverageIntensityInLumens => (properties & XRCameraFrameProperties.AverageIntensityInLumens) != 0;
        public static bool hasExposureDuration => (properties & XRCameraFrameProperties.ExposureDuration) != 0;
        public static bool hasExposureOffset => (properties & XRCameraFrameProperties.ExposureOffset) != 0;
        public static bool hasMainLightIntensityLumens => (properties & XRCameraFrameProperties.MainLightIntensityLumens) != 0;
        public static bool hasMainLightColor => (properties & XRCameraFrameProperties.MainLightColor) != 0;
        public static bool hasMainLightDirection => (properties & XRCameraFrameProperties.MainLightDirection) != 0;
        public static bool hasAmbientSphericalHarmonics => (properties & XRCameraFrameProperties.AmbientSphericalHarmonics) != 0;
        public static bool hasCameraGrain => (properties & XRCameraFrameProperties.CameraGrain) != 0;
        public static bool hasNoiseIntensity => (properties & XRCameraFrameProperties.NoiseIntensity) != 0;
    }
}
