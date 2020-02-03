using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockCameraSubsystem : XRCameraSubsystem
    {
        public const string ID = "UnityXRMock-Camera";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            // Clone descriptor
            var cinfo = new XRCameraSubsystemCinfo
            {
                id = ID,
                implementationType = typeof(UnityXRMockCameraSubsystem),
                supportsAverageBrightness = true,
                supportsAverageColorTemperature = true,
                supportsAverageIntensityInLumens = true,
                supportsCameraConfigurations = false,
                supportsCameraImage = true,
                supportsColorCorrection = true,
                supportsDisplayMatrix = true,
                supportsFocusModes = false,
                supportsProjectionMatrix = true,
                supportsTimestamp = true,
            };

            Register(cinfo);
        }

        protected override Provider CreateProvider() => new MockProvider();

        private class MockProvider : Provider
        {
            private XRCameraParams cameraParams;
            private long? timestamp;
            private Vector2? screenSize;
            private ScreenOrientation? screenOrientation;

            public override bool permissionGranted => CameraApi.permissionGranted;

            public override void Start()
            {
                this.ResetState();
                base.Start();
            }

            public override void Stop()
            {
                this.ResetState();
                base.Stop();
            }

            public override void Destroy()
            {
                base.Destroy();
            }

            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                var timestamp = CameraApi.timestamp;
                if (this.cameraParams != cameraParams
                    || this.timestamp != timestamp
                    || this.screenSize != CameraApi.screenSize
                    || this.screenOrientation != CameraApi.screenOrientation)
                {
                    try
                    {
                        var result = new XRCameraFrameMock();

                        if (CameraApi.timestamp.HasValue)
                        {
                            result.m_TimestampNs = CameraApi.timestamp.Value;
                            result.m_Properties = result.m_Properties | XRCameraFrameProperties.Timestamp;
                        }

                        if (CameraApi.averageBrightness.HasValue)
                        {
                            result.m_AverageColorTemperature = CameraApi.averageBrightness.Value;
                            result.m_Properties = result.m_Properties | XRCameraFrameProperties.AverageBrightness;
                        }

                        if (CameraApi.averageColorTemperature.HasValue)
                        {
                            result.m_AverageColorTemperature = CameraApi.averageColorTemperature.Value;
                            result.m_Properties = result.m_Properties | XRCameraFrameProperties.AverageColorTemperature;
                        }

                        if (CameraApi.colorCorrection.HasValue)
                        {
                            result.m_ColorCorrection = CameraApi.colorCorrection.Value;
                            result.m_Properties = result.m_Properties | XRCameraFrameProperties.ColorCorrection;
                        }

                        if (CameraApi.projectionMatrix.HasValue)
                        {
                            Matrix4x4 screenMatrix = Matrix4x4.identity;
                            if (CameraApi.screenSize.HasValue)
                            {
                                var sourceScreenSize = CameraApi.screenSize.Value;
                                var sourceAspect = sourceScreenSize.x / sourceScreenSize.y;
                                var screenAspect = cameraParams.screenWidth / cameraParams.screenHeight;
                                if (sourceAspect < screenAspect)
                                {
                                    screenMatrix.m00 = sourceAspect / screenAspect;
                                }
                                else
                                {
                                    screenMatrix.m11 = screenAspect / sourceAspect;
                                }
                            }

                            result.m_ProjectionMatrix = screenMatrix * CameraApi.projectionMatrix.Value;
                            result.m_Properties = result.m_Properties | XRCameraFrameProperties.ProjectionMatrix;
                        }

                        if (CameraApi.displayMatrix.HasValue)
                        {
                            result.m_DisplayMatrix = CameraApi.displayMatrix.Value;
                            result.m_Properties = result.m_Properties | XRCameraFrameProperties.DisplayMatrix;
                        }

                        result.m_TrackingState = TrackingState.Tracking;
                        result.m_NativePtr = IntPtr.Zero;

                        result.Convert(out cameraFrame);
                        return true;
                    }
                    finally
                    {
                        this.timestamp = timestamp;
                        this.cameraParams = cameraParams;
                        this.screenSize = CameraApi.screenSize;
                        this.screenOrientation = CameraApi.screenOrientation;
                    }
                }

                cameraFrame = default;
                return false;
            }

            private void ResetState()
            {
                this.cameraParams = default;
                this.timestamp = null;
                this.screenSize = null;
                this.screenOrientation = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XRCameraFrameMock
        {
            /// <summary>
            /// The timestamp, in nanoseconds, associated with this frame.
            /// </summary>
            /// <value>
            /// The timestamp, in nanoseconds, associated with this frame.
            /// </value>
            public long m_TimestampNs;

            /// <summary>
            /// The estimated brightness of the scene.
            /// </summary>
            /// <value>
            /// The estimated brightness of the scene.
            /// </value>
            public float m_AverageBrightness;

            /// <summary>
            /// The estimated color temperature of the scene.
            /// </summary>
            /// <value>
            /// The estimated color temperature of the scene.
            /// </value>
            public float m_AverageColorTemperature;

            /// <summary>
            /// The estimated color correction value of the scene.
            /// </summary>
            /// <value>
            /// The estimated color correction value of the scene.
            /// </value>
            public Color m_ColorCorrection;

            /// <summary>
            /// The 4x4 projection matrix for the camera frame.
            /// </summary>
            /// <value>
            /// The 4x4 projection matrix for the camera frame.
            /// </value>
            public Matrix4x4 m_ProjectionMatrix;

            /// <summary>
            /// The 4x4 display matrix for the camera frame.
            /// </summary>
            /// <value>
            /// The 4x4 display matrix for the camera frame.
            /// </value>
            public Matrix4x4 m_DisplayMatrix;

            /// <summary>
            /// The <see cref="TrackingState"/> associated with the camera.
            /// </summary>
            /// <value>
            /// The tracking state associated with the camera.
            /// </value>
            public TrackingState m_TrackingState;

            /// <summary>
            /// A native pointer associated with this frame. The data
            /// pointed to by this pointer is specific to provider implementation.
            /// </summary>
            /// <value>
            /// The native pointer associated with this frame.
            /// </value>
            public IntPtr m_NativePtr;

            /// <summary>
            /// The set of all flags indicating which properties are included in the frame.
            /// </summary>
            /// <value>
            /// The set of all flags indicating which properties are included in the frame.
            /// </value>
            public XRCameraFrameProperties m_Properties;

            public unsafe void Convert(out XRCameraFrame target)
            {
                fixed (XRCameraFrame* targetPtr = &target)
                fixed (XRCameraFrameMock* selfPtr = &this)
                {
                    UnsafeUtility.MemCpy(targetPtr, selfPtr, sizeof(XRCameraFrame));
                }
            }
        }
    }
}
