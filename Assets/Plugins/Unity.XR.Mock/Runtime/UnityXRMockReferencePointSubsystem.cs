//using UnityEngine.XR.ARExtensions;
using System;
using System.Linq;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockReferencePointSubsystem : XRReferencePointSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        class Provider : IProvider
        {
            public override void Start() { }

            public override void Stop() { }

            public override void Destroy()
            {
                NativeApi.UnityXRMock_referencePointReset();
            }

            public override unsafe TrackableChanges<XRReferencePoint> GetChanges(
                XRReferencePoint defaultReferencePoint,
                Allocator allocator)
            {
                try
                {
                    return TrackableChanges<XRReferencePoint>.CopyFrom(
                        new NativeArray<XRReferencePoint>(
                            NativeApi.addedRefPoints.Select(m => m.ToXRReferencePoint(defaultReferencePoint)).ToArray(), allocator),
                        new NativeArray<XRReferencePoint>(
                            NativeApi.updatedRefPoints.Select(m => m.ToXRReferencePoint(defaultReferencePoint)).ToArray(), allocator),
                        new NativeArray<TrackableId>(
                            NativeApi.removedRefPoints.Select(m => m.id).ToArray(), allocator),
                        allocator);
                }
                finally
                {
                    NativeApi.UnityXRMock_consumedReferencePointChanges();
                }
            }

            public override bool TryAddReferencePoint(
                Pose pose,
                out XRReferencePoint referencePoint)
            {
                var trackableId = NativeApi.UnityXRMock_attachReferencePoint(TrackableId.invalidId, pose);
                if (NativeApi.refPoints.TryGetValue(trackableId, out NativeApi.RefPointInfo refPointInfo))
                {
                    referencePoint = refPointInfo.ToXRReferencePoint(XRReferencePoint.GetDefault());
                    return true;
                }

                referencePoint = default;
                return false;
            }

            public override bool TryAttachReferencePoint(
                TrackableId trackableToAffix,
                Pose pose,
                out XRReferencePoint referencePoint)
            {
                return this.TryAddReferencePoint(pose, out referencePoint);
            }

            public override bool TryRemoveReferencePoint(TrackableId referencePointId)
            {
                if (NativeApi.refPoints.TryGetValue(referencePointId, out NativeApi.RefPointInfo refPointInfo))
                {
                    NativeApi.UnityXRMock_removeReferencePoint(referencePointId);
                    return true;
                }

                return false;
            }
        }

        internal static void RegisterDescriptor(XRReferencePointSubsystemDescriptor descriptor, Func<XRReferencePointSubsystem, TrackableId, Pose, TrackableId> attachReferencePoint)
        {
            if (descriptor != null)
            {
                descriptor.subsystemImplementationType = typeof(UnityXRMockReferencePointSubsystem);
            }
            else
            {
                XRReferencePointSubsystemDescriptor.Create(new XRReferencePointSubsystemDescriptor.Cinfo
                {
                    id = "UnityXRMock-ReferencePoint",
                    subsystemImplementationType = typeof(UnityXRMockReferencePointSubsystem),
                    supportsTrackableAttachments = true
                });
            }
        }
    }
}
