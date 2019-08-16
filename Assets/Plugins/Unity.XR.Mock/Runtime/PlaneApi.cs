using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class PlaneApi
    {
        public static TrackableId Add(Pose pose, Vector2 center, Vector2 size, TrackingState trackingState = TrackingState.Tracking)
        {
            var planeId = NativeApi.NewTrackableId();
            s_TrackingStates[planeId] = trackingState;
            NativeApi.UnityXRMock_setPlaneData(planeId, pose, center, size, null, 0, trackingState);
            return planeId;
        }

        public static void Update(TrackableId planeId, Pose pose, Vector2 center, Vector2 size)
        {
            NativeApi.UnityXRMock_setPlaneData(planeId, pose, center, size, null, 0, s_TrackingStates[planeId]);
        }

        public static TrackableId Add(Pose pose, Vector2[] boundaryPoints, TrackingState trackingState = TrackingState.Tracking)
        {
            if (boundaryPoints == null)
                throw new ArgumentNullException("boundaryPoints");

            var planeId = NativeApi.UnityXRMock_createTrackableId(Guid.NewGuid());
            return AddOrUpdate(planeId, TrackableId.invalidId, pose, boundaryPoints, trackingState);
        }

        public static TrackableId AddOrUpdate(TrackableId planeId, TrackableId subsumedById, Pose pose, Vector2[] boundaryPoints, TrackingState trackingState = TrackingState.Tracking)
        {
            if (boundaryPoints == null)
                throw new ArgumentNullException("boundaryPoints");

            if (planeId == TrackableId.invalidId)
            {
                planeId = NativeApi.NewTrackableId();
            }

            if (!s_TrackingStates.ContainsKey(planeId))
            {
                s_TrackingStates[planeId] = trackingState;
            }

            SetPlaneData(planeId, pose, boundaryPoints);

            if (subsumedById != TrackableId.invalidId)
            {
                NativeApi.UnityXRMock_subsumedPlane(planeId, subsumedById);
            }

            return planeId;
        }

        public static void Merge(TrackableId planeId, TrackableId subsumedById)
        {
            NativeApi.UnityXRMock_subsumedPlane(planeId, subsumedById);
        }

        public static void Update(TrackableId planeId, Pose pose, Vector2[] boundaryPoints)
        {
            SetPlaneData(planeId, pose, boundaryPoints);
        }

        public static void SetTrackingState(TrackableId planeId, TrackingState trackingState)
        {
            if (!s_TrackingStates.ContainsKey(planeId))
                return;

            s_TrackingStates[planeId] = trackingState;
            NativeApi.UnityXRMock_setPlaneTrackingState(planeId, s_TrackingStates[planeId]);
        }

        public static bool TryGetTrackingState(TrackableId planeId, out TrackingState trackingState)
        {
            return s_TrackingStates.TryGetValue(planeId, out trackingState);
        }

        public static void Remove(TrackableId planeId)
        {
            NativeApi.UnityXRMock_removePlane(planeId);
            s_TrackingStates.Remove(planeId);
        }

        static Vector2 ComputeCenter(Vector2[] boundaryPoints)
        {
            var center = Vector2.zero;
            foreach (var point in boundaryPoints)
            {
                center += point;
            }
            center /= (float)boundaryPoints.Length;

            return center;
        }

        static void SetPlaneData(TrackableId planeId, Pose pose, Vector2[] boundaryPoints)
        {
            var center = ComputeCenter(boundaryPoints);
            var size = ComputeSize(boundaryPoints);

            NativeApi.UnityXRMock_setPlaneData(planeId, pose, center,
                size, boundaryPoints, boundaryPoints.Length,
                s_TrackingStates[planeId]);
        }

        static Vector2 ComputeSize(Vector2[] boundaryPoints)
        {
            Vector2 min = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 max = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

            foreach (var point in boundaryPoints)
            {
                min.x = Mathf.Min(min.x, point.x);
                min.y = Mathf.Min(min.y, point.y);

                max.x = Mathf.Max(max.x, point.x);
                max.y = Mathf.Max(max.y, point.y);
            }

            return new Vector2(max.x - min.x, max.y - min.y);
        }

        static Dictionary<TrackableId, TrackingState> s_TrackingStates = new Dictionary<TrackableId, TrackingState>();
    }
}
