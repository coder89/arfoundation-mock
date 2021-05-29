using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    public static class DepthApi
    {
        public static readonly Dictionary<TrackableId, DepthInfo> all = new Dictionary<TrackableId, DepthInfo>();
        public static readonly List<DepthInfo> added = new List<DepthInfo>();
        public static readonly List<DepthInfo> updated = new List<DepthInfo>();
        public static readonly List<DepthInfo> removed = new List<DepthInfo>();
        public static readonly Dictionary<DepthInfo, DepthData> datas = new Dictionary<DepthInfo, DepthData>();

        public static TrackableId Add(Pose pose, TrackingState trackingState)
        {
            var result = NativeApi.NewTrackableId();
            Add(result, pose, trackingState);
            return result;
        }

        public static void Add(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            var tmp = new DepthInfo()
            {
                trackableId = trackableId,
                pose = pose,
                trackingState = trackingState
            };

            all[trackableId] = tmp;
            added.Add(tmp);
        }

        public static void Update(TrackableId trackableId, Pose pose, TrackingState trackingState)
        {
            if (all.TryGetValue(trackableId, out var tmp))
            {
                tmp.pose = pose;
                tmp.trackingState = trackingState;

                if (!added.Contains(tmp) && !updated.Contains(tmp))
                {
                    updated.Add(tmp);
                }
            }
            else
            {
                throw new ArgumentException("Unknown trackableID");
            }
        }

        public static void Remove(TrackableId trackableId)
        {
            if (all.TryGetValue(trackableId, out var tmp))
            {
                all.Remove(trackableId);
                added.Remove(tmp);
                updated.Remove(tmp);
                removed.Add(tmp);
                datas.Remove(tmp);
            }
            else
            {
                throw new ArgumentException("Unknown trackableID");
            }
        }

        public static void SetDepthData(TrackableId trackableId, Vector3[] positions, float[] confidenceValues = null, ulong[] identifiers = null)
        {
            if (all.TryGetValue(trackableId, out var tmp))
            {
                if (positions == null)
                    throw new ArgumentNullException("positions");

                if (confidenceValues != null && positions.Length != confidenceValues.Length)
                    throw new ArgumentException("confidenceValues must be the same length as positions");

                if (identifiers != null && positions.Length != identifiers.Length)
                    throw new ArgumentException("positions must be the same length as positions");

                if (!datas.TryGetValue(tmp, out var data))
                {
                    data = new DepthData();
                    datas[tmp] = data;
                }

                data.positions = positions;
                data.confidenceValues = confidenceValues;
                data.identifiers = identifiers;
            }
            else
            {
                throw new ArgumentException("Unknown trackableID");
            }
        }

        public static void consumedChanges()
        {
            added.Clear();
            updated.Clear();
            removed.Clear();
        }

        public static void reset()
        {
            removed.AddRange(all.Values);
            all.Clear();
            added.Clear();
            updated.Clear();
            datas.Clear();
        }

        public class DepthInfo
        {
            public TrackableId trackableId;
            public Pose pose;
            public TrackingState trackingState;

            public XRPointCloud ToPointCloud(XRPointCloud defaultPointCloud) => new XRPointCloud(this.trackableId, this.pose, this.trackingState, IntPtr.Zero);
        }

        public class DepthData
        {
            public Vector3[] positions;
            public float[] confidenceValues;
            public ulong[] identifiers;

            public XRPointCloudData ToPointCloudData()
            {
                var result = new XRPointCloudData()
                {
                    positions = new NativeArray<Vector3>(positions, Allocator.Persistent)
                };

                if (confidenceValues != null)
                {
                    result.confidenceValues = new NativeArray<float>(confidenceValues, Allocator.Persistent);
                }

                if (identifiers != null)
                {
                    result.identifiers = new NativeArray<ulong>(identifiers, Allocator.Persistent);
                }

                return result;
            }
        }
    }
}
