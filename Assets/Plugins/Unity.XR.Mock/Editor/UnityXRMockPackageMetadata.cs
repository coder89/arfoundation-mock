using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.XR.Mock;

using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;

namespace UnityEditor.XR.Mock
{
    class XRPackage : IXRPackage
    {
        class UnityXRMockLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        class UnityXRMockPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; }
        }

        static IXRPackageMetadata s_Metadata = new UnityXRMockPackageMetadata()
        {
            packageName = "UnityXRMock Plugin",
            packageId = "com.unity.xr.mock",
            settingsType = typeof(UnityXRMockSettings).FullName,
            loaderMetadata = new List<IXRLoaderMetadata>()
            {
                new UnityXRMockLoaderMetadata()
                {
                    loaderName = "UnityXRMock",
                    loaderType = typeof(UnityXRMockLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup>()
                    {
                        BuildTargetGroup.iOS
                    }
                },
            }
        };

        public IXRPackageMetadata metadata => s_Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            if (obj is UnityXRMockSettings settings)
            {
                UnityXRMockSettings.currentSettings = settings;
                settings.requirement = UnityXRMockSettings.Requirement.Required;
                return true;
            }

            return false;
        }
    }
}
