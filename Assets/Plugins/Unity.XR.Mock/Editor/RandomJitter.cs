namespace UnityEngine.XR.Mock.Example
{
    public class RandomJitter : MonoBehaviour
    {
        [SerializeField]
        float m_Probability = .1f;

        [SerializeField]
        Vector3 m_MaxDisplacementPerJitter = new Vector3(.1f, 0, .1f);

        [SerializeField]
        Vector3 m_MaxRotationPerJitter = new Vector3(0, 0, 0);

        void Update()
        {
            if (Random.value < this.m_Probability)
            {
                var translation = Random.insideUnitSphere;
                translation.Scale(this.m_MaxDisplacementPerJitter);
                this.transform.localPosition += translation;

                var rotation = Random.insideUnitSphere;
                rotation.Scale(this.m_MaxRotationPerJitter);
                this.transform.localRotation *= Quaternion.Euler(rotation);
            }
        }
    }
}
