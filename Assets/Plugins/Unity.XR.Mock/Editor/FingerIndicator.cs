using UnityEngine.UI;

namespace UnityEngine.XR.Mock.Example
{
    public class FingerIndicator : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_Button;
#pragma warning restore

        void Start()
        {
            Cursor.visible = false;
        }

        void Update()
        {
            var rectTransform = m_Button.transform as RectTransform;
            rectTransform.position = Input.mousePosition;
        }
    }
}
