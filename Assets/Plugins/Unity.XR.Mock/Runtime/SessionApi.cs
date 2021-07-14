using System;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class SessionApi
    {
        public static TrackingState trackingState { get; set; } = TrackingState.None;

        public static void Start()
        {
            trackingState = TrackingState.Tracking;
        }

        public static void Stop()
        {
            trackingState = TrackingState.None;
        }

        public static void Reset()
        {
            trackingState = TrackingState.None;
        }
    }
}
