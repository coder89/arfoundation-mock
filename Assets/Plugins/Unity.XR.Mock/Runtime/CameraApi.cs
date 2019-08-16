namespace UnityEngine.XR.Mock
{
    public static class CameraApi
    {
        public static bool permissionGranted { get; set; } = true;

        public static Matrix4x4? projectionMatrix
        {
            get => NativeApi.UnityXRMock_getProjectionMatrix();

            set
            {
                if (value.HasValue)
                {
                    NativeApi.UnityXRMock_setProjectionMatrix(
                        value.Value, value.Value.inverse, true);
                }
                else
                {
                    NativeApi.UnityXRMock_setProjectionMatrix(
                        Matrix4x4.identity, Matrix4x4.identity, false);
                }
            }
        }

        public static Matrix4x4? displayMatrix { get; set; }

        public static float? averageBrightness { get; set; }

        public static float? averageColorTemperature { get; set; }

        public static Color? colorCorrection { internal get; set; }

        public static long? timestamp { get; set; }

        public static Vector2? screenSize { get; set; }

        public static ScreenOrientation? screenOrientation { get; set; }
    }
}
