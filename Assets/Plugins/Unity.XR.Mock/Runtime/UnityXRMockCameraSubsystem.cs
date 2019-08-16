using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockCameraSubsystem : XRCameraSubsystem
    {

        #region Constants

        public const string ID = "UnityXRMock-Camera";

        #endregion

        #region Fields

        private bool isInitialized;
        private XRCameraSubsystem wrappedSubsystem;
        private static XRCameraSubsystemDescriptor originalDescriptor;

        #endregion

        #region Constructors

        public UnityXRMockCameraSubsystem()
        {
            this.Initialize();
        }

        #endregion

        #region XRCameraSubsystem

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

        private static void RegisterDescriptor(XRCameraSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                // Clone descriptor
                var cinfo = new XRCameraSubsystemCinfo
                {
                    id = overrideDescriptor.id,
                    implementationType = overrideDescriptor.subsystemImplementationType,
                    supportsAverageBrightness = overrideDescriptor.supportsAverageBrightness,
                    supportsAverageColorTemperature = overrideDescriptor.supportsAverageColorTemperature,
                    supportsCameraConfigurations = overrideDescriptor.supportsCameraConfigurations,
                    supportsCameraImage = overrideDescriptor.supportsCameraImage,
                    //supportsColorCorrection = overrideDescriptor.supportsColorCorrection,
                    supportsDisplayMatrix = overrideDescriptor.supportsDisplayMatrix,
                    supportsProjectionMatrix = overrideDescriptor.supportsProjectionMatrix,
                    //supportsTimestamp = overrideDescriptor.supportsTimestampsupportsTimestamp,
                };

                originalDescriptor = typeof(XRCameraSubsystemDescriptor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                                                                        .Invoke(new object[] { cinfo }) as XRCameraSubsystemDescriptor;

                // Override subsystem
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockCameraSubsystem);
            }
            else
            {
                // Clone descriptor
                var cinfo = new XRCameraSubsystemCinfo
                {
                    id = ID,
                    implementationType = typeof(UnityXRMockCameraSubsystem),
                    supportsAverageBrightness = true,
                    supportsAverageColorTemperature = true,
                    supportsCameraConfigurations = false,
                    supportsCameraImage = true,
                    supportsColorCorrection = false,
                    supportsDisplayMatrix = true,
                    supportsProjectionMatrix = true,
                    supportsTimestamp = true,
                };

                Register(cinfo);
            }
        }

        private static XRCameraSubsystemDescriptor GetSubsystemDescriptor()
        {
            List<XRCameraSubsystemDescriptor> descriptors = new List<XRCameraSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            return descriptors.FirstOrDefault(d => d.id != ID);
        }

        #endregion

        #region Types

        private class Provider : IProvider
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

        #endregion
    }
}
