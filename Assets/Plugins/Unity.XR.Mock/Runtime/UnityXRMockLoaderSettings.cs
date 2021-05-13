using System;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.Mock
{
    /// <summary>
    /// Settings to control the UnityXRMockLoader behavior.
    /// </summary>
    [Serializable]
    [XRConfigurationData("UnityXRMock", UnityXRMockLoaderConstants.k_SettingsKey)]
    public class UnityXRMockLoaderSettings : ScriptableObject
    {
        /// <summary>
        /// Static instance that will hold the runtime asset instance we created in our build process.
        /// </summary>
#if !UNITY_EDITOR
        internal static UnityXRMockLoaderSettings s_RuntimeInstance = null;
#endif

        [SerializeField, Tooltip("Allow the UnityXRMock Loader to start and stop subsystems.")]
        bool m_StartAndStopSubsystems = false;

        public bool startAndStopSubsystems
        {
            get { return m_StartAndStopSubsystems; }
            set { m_StartAndStopSubsystems = value; }
        }

        public void Awake()
        {
#if !UNITY_EDITOR
            s_RuntimeInstance = this;
#endif
        }
    }
}