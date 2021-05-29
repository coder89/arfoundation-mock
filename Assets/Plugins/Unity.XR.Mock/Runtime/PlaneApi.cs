using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class PlaneApi
    {
        private readonly static Dictionary<TrackableId, PlaneInfo> s_planes = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, PlaneInfo> s_addedPlanes = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, PlaneInfo> s_updatedPlanes = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, PlaneInfo> s_removedPlanes = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, TrackingState> s_TrackingStates = new Dictionary<TrackableId, TrackingState>();

        public static IDictionary<TrackableId, PlaneInfo> planes => s_planes;
        public static IEnumerable<PlaneInfo> addedPlanes => s_addedPlanes.Values;
        public static IEnumerable<PlaneInfo> updatedPlanes => s_updatedPlanes.Values.OrderBy(m => m.subsumedById != TrackableId.invalidId);
        public static IEnumerable<PlaneInfo> removedPlanes => s_removedPlanes.Values;

        public static TrackableId Add(
            Pose pose,
            Vector2 center,
            Vector2 size,
            TrackingState trackingState,
            PlaneAlignment? alignment,
            PlaneClassification? classification)
        {
            var planeId = NativeApi.NewTrackableId();
            s_TrackingStates[planeId] = trackingState;
            OnSetPlaneData(planeId, pose, center, size, null, trackingState, alignment, classification);
            return planeId;
        }

        public static void Update(
            TrackableId planeId,
            Pose pose,
            PlaneAlignment? alignment,
            PlaneClassification? classification,
            Vector2 center,
            Vector2 size)
        {
            OnSetPlaneData(planeId, pose, center, size, null, s_TrackingStates[planeId], alignment, classification);
        }

        public static TrackableId Add(
            Pose pose,
            Vector2[] boundaryPoints,
            TrackingState trackingState,
            PlaneAlignment? alignment,
            PlaneClassification? classification,
            Vector3? center,
            Vector2? size)
        {
            if (boundaryPoints == null)
                throw new ArgumentNullException("boundaryPoints");

            var planeId = NativeApi.UnityXRMock_createTrackableId(Guid.NewGuid());
            return AddOrUpdate(planeId, TrackableId.invalidId, pose, boundaryPoints, trackingState, alignment, classification, center, size);
        }

        public static TrackableId AddOrUpdate(
            TrackableId planeId,
            TrackableId subsumedById,
            Pose pose,
            Vector2[] boundaryPoints,
            TrackingState trackingState,
            PlaneAlignment? alignment,
            PlaneClassification? classification,
            Vector3? center,
            Vector2? size)
        {
            if (boundaryPoints == null)
            {
                throw new ArgumentNullException("boundaryPoints");
            }

            if (planeId == TrackableId.invalidId)
            {
                planeId = NativeApi.NewTrackableId();
            }

            if (!s_TrackingStates.ContainsKey(planeId))
            {
                s_TrackingStates[planeId] = trackingState;
            }

            SetPlaneData(planeId, pose, boundaryPoints, alignment, classification, center, size);

            if (subsumedById != TrackableId.invalidId)
            {
                OnSubsumedPlane(planeId, subsumedById);
            }

            return planeId;
        }

        public static void Merge(TrackableId planeId, TrackableId subsumedById)
        {
            OnSubsumedPlane(planeId, subsumedById);
        }

        public static void Update(TrackableId planeId, Pose pose, Vector2[] boundaryPoints, PlaneAlignment? alignment, PlaneClassification? classification, Vector3? center, Vector2? size)
        {
            SetPlaneData(planeId, pose, boundaryPoints, alignment, classification, center, size);
        }

        public static void SetTrackingState(TrackableId planeId, TrackingState trackingState)
        {
            if (!s_TrackingStates.ContainsKey(planeId))
                return;

            s_TrackingStates[planeId] = trackingState;

            if (!s_planes.ContainsKey(planeId) || s_addedPlanes.ContainsKey(planeId))
            {
                if (!s_planes.ContainsKey(planeId))
                {
                    s_planes[planeId] = new PlaneInfo();
                }

                s_addedPlanes[planeId] = s_planes[planeId];
            }
            else
            {
                s_updatedPlanes[planeId] = s_planes[planeId];
            }

            var planeInfo = s_planes[planeId];
            planeInfo.trackingState = trackingState;
        }

        public static bool TryGetTrackingState(TrackableId planeId, out TrackingState trackingState)
        {
            return s_TrackingStates.TryGetValue(planeId, out trackingState);
        }

        public static void Remove(TrackableId planeId)
        {
            if (s_planes.ContainsKey(planeId))
            {
                if (!s_addedPlanes.Remove(planeId))
                {
                    s_removedPlanes[planeId] = s_planes[planeId];
                }

                s_planes.Remove(planeId);
                s_updatedPlanes.Remove(planeId);
            }

            s_TrackingStates.Remove(planeId);
        }

        public static void ConsumedChanges()
        {
            s_addedPlanes.Clear();
            s_updatedPlanes.Clear();
            s_removedPlanes.Clear();
        }

        public static void Reset()
        {
            ConsumedChanges();
            s_planes.Clear();
        }

        private static Vector2 ComputeCenter(Vector2[] boundaryPoints)
        {
            var center = Vector2.zero;
            foreach (var point in boundaryPoints)
            {
                center += point;
            }
            center /= (float)boundaryPoints.Length;

            return center;
        }

        private static void SetPlaneData(
            TrackableId planeId,
            Pose pose,
            Vector2[] boundaryPoints,
            PlaneAlignment? alignment,
            PlaneClassification? classification,
            Vector3? center,
            Vector2? size)
        {
            var _center = center ?? ComputeCenter(boundaryPoints);
            var _size = size ?? ComputeSize(boundaryPoints);

            OnSetPlaneData(planeId, pose, _center, _size, boundaryPoints, s_TrackingStates[planeId], alignment, classification);
        }

        private static Vector2 ComputeSize(Vector2[] boundaryPoints)
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

        private static void OnSetPlaneData(
            TrackableId planeId,
            Pose pose,
            Vector2 center,
            Vector2 size,
            Vector2[] boundaryPoints,
            TrackingState trackingState,
            PlaneAlignment? alignment,
            PlaneClassification? classification)
        {
            if (!s_planes.ContainsKey(planeId) || s_addedPlanes.ContainsKey(planeId))
            {
                if (!s_planes.ContainsKey(planeId))
                {
                    s_planes[planeId] = new PlaneInfo()
                    {
                        id = planeId
                    };
                }

                s_addedPlanes[planeId] = s_planes[planeId];
            }
            else
            {
                s_updatedPlanes[planeId] = s_planes[planeId];
            }

            var planeInfo = s_planes[planeId];
            planeInfo.pose = pose;
            planeInfo.center = center;
            planeInfo.size = size;
            planeInfo.boundaryPoints = boundaryPoints;
            planeInfo.trackingState = trackingState;
            planeInfo.planeAlignment = alignment;
            planeInfo.planeClassification = classification;
        }

        private static void OnSubsumedPlane(TrackableId planeId, TrackableId subsumedById)
        {
            if (!s_planes.ContainsKey(planeId) || s_addedPlanes.ContainsKey(planeId))
            {
                if (!s_planes.ContainsKey(planeId))
                {
                    s_planes[planeId] = new PlaneInfo()
                    {
                        id = planeId
                    };
                }

                s_addedPlanes[planeId] = s_planes[planeId];
            }
            else
            {
                s_updatedPlanes[planeId] = s_planes[planeId];
            }

            var planeInfo = s_planes[planeId];
            planeInfo.subsumedById = subsumedById;
        }

        public class PlaneInfo
        {
            private const float AxisAlignmentEpsilon = 0.25f; // ~15 deg (arccos(0.25) ~= 75.52 deg

            public TrackableId id;
            public TrackableId subsumedById;
            public Pose pose;
            public Vector2 center;
            public Vector2 size;
            public Vector2[] boundaryPoints;
            public TrackingState trackingState;
            public PlaneAlignment? planeAlignment;
            public PlaneClassification? planeClassification;

            public BoundedPlane ToBoundedPlane(BoundedPlane defaultPlane)
            {
                return new BoundedPlane(
                    this.id,
                    this.subsumedById,
                    this.pose,
                    this.center,
                    this.size,
                    planeAlignment ?? GetAlignment(this.pose),
                    this.trackingState,
                    defaultPlane.nativePtr,
                    planeClassification ?? defaultPlane.classification);
            }

            public static PlaneAlignment GetAlignment(Pose pose)
            {
                var normal = pose.up;
                if (Mathf.Abs(normal.y) < AxisAlignmentEpsilon)
                {
                    return PlaneAlignment.Vertical;
                }
                else if (Mathf.Abs(normal.y) > (1.0f - AxisAlignmentEpsilon))
                {
                    return PlaneAlignment.HorizontalUp;
                }

                return PlaneAlignment.NotAxisAligned;
            }
        }
    }
}
