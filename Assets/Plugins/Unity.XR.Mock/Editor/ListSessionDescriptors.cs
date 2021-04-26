using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock.Example
{
    static class ListSessionDescriptors
    {
        [MenuItem("Sessions/List Available Sessions")]
        static void ListSessions()
        {
            var descriptors = new List<XRSessionSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            foreach (var descriptor in descriptors)
            {
                Debug.unityLogger.Log("ar-mock", $"Session: {descriptor.id}");
            }

            if (descriptors.Count == 0)
                Debug.unityLogger.Log("ar-mock", "No sessions available.");
        }
    }
}
