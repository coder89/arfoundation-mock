using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.Mock.Example
{
    public class SimulateAnchors : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ARAnchorManager m_AnchorManager;

        [SerializeField]
        int m_Count = 4;

        [SerializeField]
        float m_Radius = 5f;
#pragma warning restore

        List<ARAnchor> m_Anchors;

        IEnumerator Start()
        {
            m_Anchors = new List<ARAnchor>();

            yield return new WaitForSeconds(1f);

            for (int i = 0; i < m_Count; ++i)
            {
                var position = Random.insideUnitSphere * m_Radius + transform.position;
                var rotation = Quaternion.AngleAxis(Random.Range(0, 360), Random.onUnitSphere);

                var anchor = m_AnchorManager.AddAnchor(new Pose(position, rotation));
                if (anchor != null)
                    m_Anchors.Add(anchor);

                yield return new WaitForSeconds(.5f);
            }

            var previousPosition = transform.localPosition;

            while (enabled)
            {
                if (transform.hasChanged)
                {
                    var delta = transform.position - previousPosition;
                    previousPosition = transform.position;

                    foreach (var anchor in m_Anchors)
                    {
                        var pose = new Pose(anchor.transform.position + delta, anchor.transform.rotation);
                        AnchorApi.Update(anchor.trackableId, pose, TrackingState.Tracking);

                        yield return new WaitForSeconds(.5f);
                    }

                    transform.hasChanged = false;
                }

                yield return null;
            }
        }
    }
}
