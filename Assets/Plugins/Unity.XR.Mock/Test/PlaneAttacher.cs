using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.Mock.Example
{
    [RequireComponent(typeof(Raycaster))]
    public class PlaneAttacher : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ARSessionOrigin m_ARSessionOrigin;

        [SerializeField]
        float m_DistanceFromPlane = .1f;
#pragma warning restore

        ARAnchor m_Attachment;

        void OnEnable()
        {
            GetComponent<Raycaster>().rayHit += OnRayHit;
        }

        void OnDisable()
        {
            GetComponent<Raycaster>().rayHit -= OnRayHit;
        }

        void OnRayHit(ARRaycastHit hit)
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            var anchorManager = m_ARSessionOrigin.GetComponent<ARAnchorManager>();
            var planeManager = m_ARSessionOrigin.GetComponent<ARPlaneManager>();

            if (anchorManager == null || planeManager == null)
                return;

            var plane = planeManager.GetPlane(hit.trackableId);
            if (plane == null)
                return;

            if (m_Attachment != null)
            {
                anchorManager.RemoveAnchor(m_Attachment);
                m_Attachment = null;
            }

            var planeNormal = plane.transform.up;
            var pose = new Pose(hit.pose.position + planeNormal * m_DistanceFromPlane, hit.pose.rotation);
            m_Attachment = anchorManager.AttachAnchor(plane, pose);
        }
    }
}
