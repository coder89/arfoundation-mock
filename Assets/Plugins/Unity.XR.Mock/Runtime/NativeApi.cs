using AOT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    /// <summary>
    /// Provides functionality to inject data into the XRMock provider.
    /// </summary>
    public static class NativeApi
    {
        static NativeApi()
        {
            UnityXRMock_setTrackableIdGenerator(NewTrackableId);
        }

        public static void UnityXRMock_connectDevice(int id)
        {
            LogNotImplemented();
        }

        public static void UnityXRMock_disconnectDevice(int id)
        {
            LogNotImplemented();
        }

        public static void UnityXRMock_setPose(Pose pose, Matrix4x4 transform)
        {
            Camera.main.transform.position = pose.position;
            Camera.main.transform.rotation = pose.rotation;
            Camera.main.transform.localScale = transform.lossyScale;
        }

        public delegate IntPtr AddAnchorHandler(float px, float py, float pz,
            float rx, float ry, float rz, float rw);

        public delegate bool RequestRemoveAnchorDelegate(UInt64 id1, UInt64 id2);

        public static void UnityARMock_setAddAnchorHandler(
            AddAnchorHandler fp, RequestRemoveAnchorDelegate fp2)
        {
            LogNotImplemented();
        }

        //Test passing all params without need for marshaling
        public static bool UnityXRMock_addReferenceResultData(ulong id0, ulong id1, bool result, int trackingState)
        {
            LogNotImplemented();
            return false;
        }

        public static bool UnityXRMock_processPlaneEvent(IntPtr planeData, int size)
        {
            LogNotImplemented();
            return false;
        }

        public delegate void SetLightEstimationDelegate(bool enabled);

        public delegate void UnityProcessRaycastCallbackDelegate(float x, float y, byte type);

        public static void UnityARMock_setRaycastHandler(UnityProcessRaycastCallbackDelegate fp)
        {
            LogNotImplemented();
        }

        #region CameraProvider
        public static void UnityXRMock_setLightEstimation(SetLightEstimationDelegate fp)
        {
            LogNotImplemented();
        }

        public static void UnityXRMock_setCameraFrameData(IntPtr frameData)
        {
            LogNotImplemented();
        }
        #endregion

        public static TrackableId UnityXRMock_createTrackableId(string trackableId)
        {
            if (!string.IsNullOrWhiteSpace(trackableId))
            {
                string[] bytes = trackableId.Split('-');
                if (bytes.Length == 2)
                {
                    ulong subId1 = Convert.ToUInt64(bytes[0], 16);
                    ulong subId2 = Convert.ToUInt64(bytes[1], 16);
                    return new TrackableId(subId1, subId2);
                }
            }

            return TrackableId.invalidId;
        }

        public static TrackableId UnityXRMock_createTrackableId(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            ulong subId1 = BitConverter.ToUInt64(bytes, 0);
            ulong subId2 = BitConverter.ToUInt64(bytes, 8);
            return new TrackableId(subId1, subId2);
        }

        [MonoPInvokeCallback(typeof(Func<TrackableId>))]
        public static TrackableId NewTrackableId()
        {
            return UnityXRMock_createTrackableId(Guid.NewGuid());
        }

        private static Func<TrackableId> s_trackableIdGenerator;

        public static Func<TrackableId> UnityXRMock_getTrackableIdGenerator()
        {
            return s_trackableIdGenerator;
        }

        public static void UnityXRMock_setTrackableIdGenerator(
            Func<TrackableId> generator)
        {
            s_trackableIdGenerator = generator;
        }

        public static void UnityXRMock_setRaycastHits(
            XRRaycastHit[] hits, int size)
        {
            LogNotImplemented();
        }

        private static void LogNotImplemented([CallerMemberName] string memberName = "")
        {
            Debug.unityLogger.LogError("ar-mock", $"{nameof(NativeApi)}.{memberName} not implemented");
        }
    }
}
