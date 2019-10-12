using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockPlaneSubsystem : XRPlaneSubsystem
    {
        #region Constants

        public const string ID = "UnityXRMock-Plane";

        #endregion

        #region Fields

        private bool isInitialized;
        private XRPlaneSubsystem wrappedSubsystem;
        private static XRPlaneSubsystemDescriptor originalDescriptor;

        #endregion

        #region Constructors

        public UnityXRMockPlaneSubsystem()
        {
            this.Initialize();
        }

        #endregion

        #region XRPlaneSubsystem

        public override void Start()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Start();
            }

            base.Start();
        }

        public override TrackableChanges<BoundedPlane> GetChanges(Allocator allocator)
        {
            if (this.wrappedSubsystem != null)
            {
                return this.wrappedSubsystem.GetChanges(allocator);
            }

            return base.GetChanges(allocator);
        }

        public override void Stop()
        {
            if (this.wrappedSubsystem != null)
            {
                this.wrappedSubsystem.Stop();
            }

            base.Stop();
        }

        //public override void Destroy()
        //{
        //    if (this.wrappedSubsystem != null)
        //    {
        //        this.wrappedSubsystem.Destroy();
        //    }
        //
        //    base.Destroy();
        //}

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

        private static void RegisterDescriptor(XRPlaneSubsystemDescriptor overrideDescriptor = default)
        {
            if (overrideDescriptor != null)
            {
                // Clone descriptor
                var cinfo = new XRPlaneSubsystemDescriptor.Cinfo
                {
                    id = overrideDescriptor.id,
                    subsystemImplementationType = overrideDescriptor.subsystemImplementationType,
                    supportsArbitraryPlaneDetection = overrideDescriptor.supportsArbitraryPlaneDetection,
                    supportsBoundaryVertices = overrideDescriptor.supportsBoundaryVertices,
                    supportsHorizontalPlaneDetection = overrideDescriptor.supportsHorizontalPlaneDetection,
                    supportsVerticalPlaneDetection = overrideDescriptor.supportsVerticalPlaneDetection
                };

                originalDescriptor = typeof(XRPlaneSubsystemDescriptor).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                                                                       .Invoke(new object[] { cinfo }) as XRPlaneSubsystemDescriptor;

                // Override subsystem
                overrideDescriptor.subsystemImplementationType = typeof(UnityXRMockPlaneSubsystem);
            }
            else
            {
                XRPlaneSubsystemDescriptor.Create(new XRPlaneSubsystemDescriptor.Cinfo
                {
                    id = ID,
                    subsystemImplementationType = typeof(UnityXRMockPlaneSubsystem),
                    supportsHorizontalPlaneDetection = true,
                    supportsVerticalPlaneDetection = true,
                    supportsArbitraryPlaneDetection = true,
                    supportsBoundaryVertices = true
                });
            }
        }

        private static XRPlaneSubsystemDescriptor GetSubsystemDescriptor()
        {
            List<XRPlaneSubsystemDescriptor> descriptors = new List<XRPlaneSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            return descriptors.FirstOrDefault(d => d.id != ID);
        }

        #endregion

        #region Types

        private class Provider : IProvider
        {
            public override void Destroy()
            {
                NativeApi.UnityXRMock_planesReset();
            }

            public override void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> boundary)
            {
                if (NativeApi.planes.TryGetValue(trackableId, out NativeApi.PlaneInfo planeInfo) &&
                    planeInfo.boundaryPoints != null &&
                    planeInfo.boundaryPoints.Length > 0)
                {
                    CreateOrResizeNativeArrayIfNecessary(planeInfo.boundaryPoints.Length, allocator, ref boundary);
                    boundary.CopyFrom(planeInfo.boundaryPoints);
                }
                else if (boundary.IsCreated)
                {
                    boundary.Dispose();
                }
            }

            public override TrackableChanges<BoundedPlane> GetChanges(
                BoundedPlane defaultPlane,
                Allocator allocator)
            {
                try
                {
                    return TrackableChanges<BoundedPlane>.CopyFrom(
                        new NativeArray<BoundedPlane>(
                            NativeApi.addedPlanes.Select(m => m.ToBoundedPlane(defaultPlane)).ToArray(), allocator),
                        new NativeArray<BoundedPlane>(
                            NativeApi.updatedPlanes.Select(m => m.ToBoundedPlane(defaultPlane)).ToArray(), allocator),
                        new NativeArray<TrackableId>(
                            NativeApi.removedPlanes.Select(m => m.id).ToArray(), allocator),
                        allocator);
                }
                finally
                {
                    NativeApi.UnityXRMock_consumedPlaneChanges();
                }
            }
        }

        #endregion
    }
}
