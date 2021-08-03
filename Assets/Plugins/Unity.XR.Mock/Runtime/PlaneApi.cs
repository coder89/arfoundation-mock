using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class PlaneApi
    {
        private readonly static object stateLock = new object();
        private readonly static Dictionary<TrackableId, PlaneInfo> all = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, PlaneInfo> added = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, PlaneInfo> updated = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, PlaneInfo> removed = new Dictionary<TrackableId, PlaneInfo>();
        private readonly static Dictionary<TrackableId, TrackingState> datas = new Dictionary<TrackableId, TrackingState>();

        public static TrackableId Add(
            Pose pose,
            Vector2 center,
            Vector2 size,
            TrackingState trackingState,
            PlaneAlignment? alignment,
            PlaneClassification? classification)
        {
            lock (stateLock)
            {
                var planeId = NativeApi.NewTrackableId();
                datas[planeId] = trackingState;
                OnSetPlaneData(planeId, pose, center, size, null, trackingState, alignment, classification);
                return planeId;
            }
        }

        public static void Update(
            TrackableId planeId,
            Pose pose,
            PlaneAlignment? alignment,
            PlaneClassification? classification,
            Vector2 center,
            Vector2 size)
        {
            lock (stateLock)
            {
                OnSetPlaneData(planeId, pose, center, size, null, datas[planeId], alignment, classification);
            }
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
            lock (stateLock)
            {
                if (boundaryPoints == null)
                    throw new ArgumentNullException("boundaryPoints");

                var planeId = NativeApi.UnityXRMock_createTrackableId(Guid.NewGuid());
                return AddOrUpdate(planeId, TrackableId.invalidId, pose, boundaryPoints, trackingState, alignment, classification, center, size);
            }
        }

        public static TrackableId AddOrUpdate(
            TrackableId planeId,
            TrackableId? subsumedById,
            Pose pose,
            Vector2[] boundaryPoints,
            TrackingState trackingState,
            PlaneAlignment? alignment,
            PlaneClassification? classification,
            Vector3? center,
            Vector2? size)
        {
            lock (stateLock)
            {
                if (boundaryPoints == null)
                {
                    throw new ArgumentNullException("boundaryPoints");
                }

                if (planeId == TrackableId.invalidId)
                {
                    planeId = NativeApi.NewTrackableId();
                }

                if (!datas.ContainsKey(planeId))
                {
                    datas[planeId] = trackingState;
                }

                SetPlaneData(planeId, pose, boundaryPoints, alignment, classification, center, size);

                if (subsumedById.HasValue)
                {
                    OnSubsumedPlane(planeId, subsumedById.Value);
                }

                return planeId;
            }
        }

        public static void Merge(TrackableId planeId, TrackableId subsumedById)
        {
            lock (stateLock)
            {
                OnSubsumedPlane(planeId, subsumedById);
            }
        }

        public static void Update(TrackableId planeId, Pose pose, Vector2[] boundaryPoints, PlaneAlignment? alignment, PlaneClassification? classification, Vector3? center, Vector2? size)
        {
            lock (stateLock)
            {
                SetPlaneData(planeId, pose, boundaryPoints, alignment, classification, center, size);
            }
        }

        public static void SetTrackingState(TrackableId planeId, TrackingState trackingState)
        {
            lock (stateLock)
            {
                if (!datas.ContainsKey(planeId))
                    return;

                EnsurePlaneCreated(planeId);

                datas[planeId] = trackingState;

                var planeInfo = all[planeId];
                planeInfo.trackingState = trackingState;
            }
        }

        public static bool TryGetTrackingState(TrackableId planeId, out TrackingState trackingState)
        {
            lock (stateLock)
            {
                return datas.TryGetValue(planeId, out trackingState);
            }
        }

        public static void Remove(TrackableId planeId)
        {
            lock (stateLock)
            {
                if (all.ContainsKey(planeId))
                {
                    if (!added.Remove(planeId))
                    {
                        removed[planeId] = all[planeId];
                    }

                    all.Remove(planeId);
                    updated.Remove(planeId);
                }

                datas.Remove(planeId);
            }
        }

        public static void RemoveAll()
        {
            lock (stateLock)
            {
                added.Clear();
                updated.Clear();
                datas.Clear();

                foreach (var p in all)
                {
                    removed[p.Key] = p.Value;
                }

                all.Clear();
            }
        }

        public static TrackableChanges<BoundedPlane> ConsumeChanges(BoundedPlane defaultPlane, Allocator allocator)
        {
            lock (stateLock)
            {
                try
                {
                    if (allocator != Allocator.None)
                    {
                        T[] EfficientArray<T>(IEnumerable<PlaneApi.PlaneInfo> collection, Func<PlaneApi.PlaneInfo, T> converter)
                            => collection.Any(m => true) ? collection.Select(converter).ToArray() : Array.Empty<T>();

                        return TrackableChanges<BoundedPlane>.CopyFrom(
                            new NativeArray<BoundedPlane>(
                                EfficientArray(PlaneApi.added.Values, m => m.ToBoundedPlane(defaultPlane)), allocator),
                            new NativeArray<BoundedPlane>(
                                EfficientArray(PlaneApi.updated.Values, m => m.ToBoundedPlane(defaultPlane)), allocator),
                            new NativeArray<TrackableId>(
                                EfficientArray(PlaneApi.removed.Values, m => m.id), allocator),
                            allocator);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    added.Clear();
                    updated.Clear();
                    removed.Clear();
                }
            }
        }

        public static void Reset()
        {
            lock (stateLock)
            {
                ConsumeChanges(default, Allocator.None);
                all.Clear();
                datas.Clear();
            }
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

            OnSetPlaneData(planeId, pose, _center, _size, boundaryPoints, datas[planeId], alignment, classification);
        }

        public static bool TryGetPlaneData(TrackableId trackableId, out Vector2[] boundary)
        {
            lock (stateLock)
            {
                if (PlaneApi.all.TryGetValue(trackableId, out PlaneApi.PlaneInfo planeInfo) &&
                    planeInfo.boundaryPoints != null &&
                    planeInfo.boundaryPoints.Length > 0)
                {
                    boundary = planeInfo.boundaryPoints;
                    return true;
                }

                boundary = null;
                return false;
            }
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
            EnsurePlaneCreated(planeId);

            var planeInfo = all[planeId];
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
            EnsurePlaneCreated(planeId);

            var planeInfo = all[planeId];
            planeInfo.subsumedById = subsumedById;
        }

        private static void EnsurePlaneCreated(TrackableId planeId)
        {
            if (!all.ContainsKey(planeId))
            {
                if (added.ContainsKey(planeId))
                {
                    all[planeId] = added[planeId];
                }
                else
                {
                    added[planeId] = all[planeId] = new PlaneInfo()
                    {
                        id = planeId
                    };
                }
            }
            else if (!added.ContainsKey(planeId))
            {
                updated[planeId] = all[planeId];
            }
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
