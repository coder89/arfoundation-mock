﻿namespace UnityEngine.XR.Mock.Example
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        float m_MoveSpeed = 1f;

        [SerializeField]
        float m_TurnSpeed = 5f;

        [SerializeField]
        KeyCode m_KeyForward = KeyCode.W;

        [SerializeField]
        KeyCode m_KeyLeft = KeyCode.A;

        [SerializeField]
        KeyCode m_KeyBack = KeyCode.S;

        [SerializeField]
        KeyCode m_KeyRight = KeyCode.D;

        [SerializeField]
        KeyCode m_KeyUp = KeyCode.Q;

        [SerializeField]
        KeyCode m_KeyDown = KeyCode.E;

        [SerializeField]
        KeyCode m_KeyTiltLeft = KeyCode.Z;

        [SerializeField]
        KeyCode m_KeyTiltRight = KeyCode.X;

        [SerializeField]
        MouseButton m_LookMouseButton = MouseButton.Right;

        enum MouseButton
        {
            Left = 0,
            Middle = 2,
            Right = 1
        }

        Vector3 m_PreviousMousePosition;

        void Move(Vector3 direction)
        {
            transform.position += direction * (Time.deltaTime * m_MoveSpeed);
        }

        void Tilt(float direction)
        {
            transform.localRotation = transform.localRotation * Quaternion.AngleAxis(direction * Time.deltaTime * m_TurnSpeed, Vector3.forward);
        }

        void Awake()
        {
            this.transform.position = Camera.main.transform.position;
            this.transform.rotation = Camera.main.transform.rotation;
        }

        void Update()
        {
            int mouseButton = (int)m_LookMouseButton;
            if (Input.GetMouseButton(mouseButton))
            {
                if (Input.GetMouseButtonDown(mouseButton))
                    m_PreviousMousePosition = Input.mousePosition;

                var delta = (Input.mousePosition - m_PreviousMousePosition) * Time.deltaTime * m_TurnSpeed;
                var euler = transform.rotation.eulerAngles;
                euler.x -= delta.y;
                euler.y += delta.x;
                transform.rotation = Quaternion.Euler(euler);
                m_PreviousMousePosition = Input.mousePosition;
            }

            if (Input.GetKey(m_KeyForward))
                Move(transform.forward);
            if (Input.GetKey(m_KeyLeft))
                Move(-transform.right);
            if (Input.GetKey(m_KeyRight))
                Move(transform.right);
            if (Input.GetKey(m_KeyBack))
                Move(-transform.forward);
            if (Input.GetKey(m_KeyDown))
                Move(-transform.up);
            if (Input.GetKey(m_KeyUp))
                Move(transform.up);
            if (Input.GetKey(m_KeyTiltLeft))
                Tilt(-4);
            if (Input.GetKey(m_KeyTiltRight))
                Tilt(4);

            InputApi.pose = new Pose(transform.position, transform.rotation);
        }
    }
}
