using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Experimental;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.XR.Mock
{
    public sealed class UnityXRMockRaycastSubsytem : XRRaycastSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        class Provider : IProvider
        {
            public override void Start() { base.Start(); }

            public override void Stop() { base.Stop(); }

            public override void Destroy() { }

            public override NativeArray<XRRaycastHit> Raycast(XRRaycastHit defaultRaycastHit, Ray ray,
                                TrackableType trackableTypeMask, Allocator allocator)
            {
                var hits = new NativeArray<XRRaycastHit>();
                return hits;
            }

            public override NativeArray<XRRaycastHit> Raycast(XRRaycastHit defaultRaycastHit, Vector2 screenPoint,
                                TrackableType trackableTypeMask, Allocator allocator)
            {
                var hits = new NativeArray<XRRaycastHit>();
                return hits;
            }
        }

        internal static void RegisterDescriptor(XRRaycastSubsystemDescriptor descriptor)
        {
            if (descriptor != null)
            {
                descriptor.subsystemImplementationType = typeof(UnityXRMockRaycastSubsytem);
            }
            else
            {
                XRRaycastSubsystemDescriptor.RegisterDescriptor(new XRRaycastSubsystemDescriptor.Cinfo
                {
                    id = "UnityXRMock-Raycast",
                    subsystemImplementationType = typeof(UnityXRMockRaycastSubsytem),
                });
            }
        }

    }
}
