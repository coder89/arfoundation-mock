using UnityEngine;
using UnityEditor;

namespace UnityEngine.XR.Mock
{
    /// <summary>
    /// Holds settings that are used to configure the Unity UnityXRMock Plugin.
    /// </summary>
    public class UnityXRMockSettings : ScriptableObject
    {
        /// <summary>
        /// Enum which defines whether UnityXRMock is optional or required.
        /// </summary>
        public enum Requirement
        {
            /// <summary>
            /// UnityXRMock is required, which means the app cannot be installed on devices that do not support UnityXRMock.
            /// </summary>
            Required,

            /// <summary>
            /// UnityXRMock is optional, which means the the app can be installed on devices that do not support UnityXRMock.
            /// </summary>
            Optional
        }

        [SerializeField, Tooltip("Toggles whether UnityXRMock is required for this app. Will make app only downloadable by devices with UnityXRMock support if set to 'Required'.")]
        Requirement m_Requirement;

        /// <summary>
        /// Determines whether UnityXRMock is required for this app: will make app only downloadable by devices with UnityXRMock support if set to <see cref="Requirement.Required"/>.
        /// </summary>
        public Requirement requirement
        {
            get { return m_Requirement; }
            set { m_Requirement = value; }
        }

        /// <summary>
        /// Gets the currently selected settings, or create a default one if no <see cref="UnityXRMockSettings"/> has been set in Player Settings.
        /// </summary>
        /// <returns>The UnityXRMock settings to use for the current Player build.</returns>
        public static UnityXRMockSettings GetOrCreateSettings()
        {
            var settings = currentSettings;
            if (settings != null)
                return settings;

            return CreateInstance<UnityXRMockSettings>();
        }

        /// <summary>
        /// Get or set the <see cref="UnityXRMockSettings"/> that will be used for the player build.
        /// </summary>
        public static UnityXRMockSettings currentSettings
        {
            get
            {
                UnityXRMockSettings settings = null;
                EditorBuildSettings.TryGetConfigObject(k_ConfigObjectName, out settings);
                return settings;
            }

            set
            {
                if (value == null)
                {
                    EditorBuildSettings.RemoveConfigObject(k_ConfigObjectName);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(k_ConfigObjectName, value, true);
                }
            }
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        internal static bool TrySelect()
        {
            var settings = currentSettings;
            if (settings == null)
                return false;

            Selection.activeObject = settings;
            return true;
        }

        static readonly string k_ConfigObjectName = "com.unity.xr.mock.PlayerSettings";
    }
}
