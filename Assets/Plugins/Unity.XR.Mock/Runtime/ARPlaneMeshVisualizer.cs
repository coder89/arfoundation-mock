using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    /// <summary>
    /// Generates a mesh for an <see cref="ARPlane"/>.
    /// </summary>
    /// <remarks>
    /// If this <c>GameObject</c> has a <c>MeshFilter</c> and/or <c>MeshCollider</c>,
    /// this component will generate a mesh from the underlying <c>BoundedPlane</c>.
    ///
    /// It will also update a <c>LineRenderer</c> with the boundary points, if present.
    /// </remarks>
    [RequireComponent(typeof(ARPlane))]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/api/UnityEngine.XR.ARFoundation.ARPlaneMeshVisualizer.html")]
    public sealed class ARPlaneMeshVisualizer : MonoBehaviour
    {
        /// <summary>
        /// Get the <c>Mesh</c> that this visualizer creates and manages.
        /// </summary>
        public Mesh mesh { get; private set; }

        unsafe void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
        {
            var boundary = this.m_Plane.boundary;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            try
            {
                NativeArray<Vector2> newBoundary = new NativeArray<Vector2>(boundary.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                Vector2* boundaryPointer = (Vector2*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(boundary);
                UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(newBoundary), boundaryPointer, boundary.Length * (long)UnsafeUtility.SizeOf<Vector2>());
                boundary = newBoundary;
#endif
                if (!ARPlaneMeshGenerators.GenerateMesh(this.mesh, new Pose(this.transform.localPosition, this.transform.localRotation), boundary))
                    return;

                var lineRenderer = this.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = boundary.Length;
                    for (int i = 0; i < boundary.Length; ++i)
                    {
                        var point2 = boundary[i];
                        lineRenderer.SetPosition(i, new Vector3(point2.x, 0, point2.y));
                    }
                }

                var meshFilter = this.GetComponent<MeshFilter>();
                if (meshFilter != null)
                    meshFilter.sharedMesh = this.mesh;

                var meshCollider = this.GetComponent<MeshCollider>();
                if (meshCollider != null)
                    meshCollider.sharedMesh = this.mesh;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            }
            finally
            { boundary.Dispose(); }
#endif
        }

        void DisableComponents()
        {
            this.enabled = false;

            var meshCollider = this.GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.enabled = false;

            this.UpdateVisibility();
        }

        void SetVisible(bool visible)
        {
            var meshRenderer = this.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = visible;

            var lineRenderer = this.GetComponent<LineRenderer>();
            if (lineRenderer != null)
                lineRenderer.enabled = visible;
        }

        void UpdateVisibility()
        {
            var visible = this.enabled &&
                (this.m_Plane.trackingState != TrackingState.None) &&
                (ARSession.state > ARSessionState.Ready) &&
                (this.m_Plane.subsumedBy == null);

            this.SetVisible(visible);
        }

        void Awake()
        {
            this.mesh = new Mesh();
            this.m_Plane = this.GetComponent<ARPlane>();
        }

        void OnEnable()
        {
            this.m_Plane.boundaryChanged += this.OnBoundaryChanged;
            this.UpdateVisibility();
            this.OnBoundaryChanged(default(ARPlaneBoundaryChangedEventArgs));
        }

        void OnDisable()
        {
            this.m_Plane.boundaryChanged -= this.OnBoundaryChanged;
            this.UpdateVisibility();
        }

        void Update()
        {
            if (this.transform.hasChanged)
            {
                var lineRenderer = this.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    if (!this.m_InitialLineWidthMultiplier.HasValue)
                        this.m_InitialLineWidthMultiplier = lineRenderer.widthMultiplier;

                    lineRenderer.widthMultiplier = this.m_InitialLineWidthMultiplier.Value * this.transform.lossyScale.x;
                }
                else
                {
                    this.m_InitialLineWidthMultiplier = null;
                }

                this.transform.hasChanged = false;
            }

            if (this.m_Plane.subsumedBy != null)
            {
                this.DisableComponents();
            }
            else
            {
                this.UpdateVisibility();
            }
        }

        float? m_InitialLineWidthMultiplier;

        ARPlane m_Plane;
    }
}
