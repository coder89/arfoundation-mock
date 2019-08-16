using UnityEngine.Scripting;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    [DefaultExecutionOrder(int.MinValue)]
    public sealed class UnityXRMockActivator : MonoBehaviour
    {
        public static bool Active { get; private set; } = false;

        public UnityXRMockActivator()
        {
            Active = true;
        }

        private void Awake()
        {
            Active = true;
        }

        private void OnEnable()
        {
            Active = true;
        }

        private void OnDisable()
        {
            Active = false;
        }

        private void OnDestroy()
        {
            Active = false;
        }
    }
}
