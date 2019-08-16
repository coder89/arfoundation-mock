using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.XR.Mock
{
    public static class UnityXRMockSettingsProvider
    {
        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            GUIContent s_WarningToCreateSettings = EditorGUIUtility.TrTextContent(
                "This controls the Build Settings for UnityXRMock.\n\nYou must create a serialized instance of the settings data in order to modify the settings in this UI. Until then only default settings set by the provider will be available.");

            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("Project/XR/UnityXRMock", SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "UnityXRMock Build Settings",

                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    if (UnityXRMockSettings.currentSettings == null)
                    {
                        EditorGUILayout.HelpBox(s_WarningToCreateSettings);
                        if (GUILayout.Button(EditorGUIUtility.TrTextContent("Create")))
                        {
                            Create();
                        }
                        else
                        {
                            return;
                        }
                    }

                    var serializedSettings = UnityXRMockSettings.GetSerializedSettings();

                    EditorGUILayout.PropertyField(serializedSettings.FindProperty("m_Requirement"), new GUIContent(
                        "Requirement",
                        "Toggles whether UnityXRMock is required for this app. This will make the app only downloadable by devices with UnityXRMock support if set to 'Required'."));

                    serializedSettings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "UnityXRMock", "optional", "required" })
            };

            return provider;
        }

        static void Create()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save UnityXRMock Settings", "UnityXRMockSettings", "asset", "Please enter a filename to save the UnityXRMock settings.");
            if (string.IsNullOrEmpty(path))
                return;

            var settings = ScriptableObject.CreateInstance<UnityXRMockSettings>();
            AssetDatabase.CreateAsset(settings, path);
            UnityXRMockSettings.currentSettings = settings;
        }
    }
}
