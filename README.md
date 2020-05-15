# Unity.XR.Mock for ARFoundation 3.x

Unity Editor plugin that let's you mock AR environment for development &amp; testing on your PC using Unity 2019.2 and AR Foundation 3.x

# How to use it #

## Release package ##

1. Import latest unity package from: https://github.com/coder89/arfoundation-mock/releases
2. Add mocked trackables using scripts provided in "/Assets/Plugins/Unity.XR.Mock/Editor". See "AR Mock" GameObject in "SampleScene" for reference.

## Unity Package Manager ##

Alternatively, you can clone this repository into a location relative to your Unity project and add it as a Package Manager package.

1. Clone this repository.
2. Open `<UNITY_PROJECT_DIR>/Packages/manifest.json`
3. Add below line under `dependencies`:
```json
{
  "dependencies": {
    // ....
    // ------- ADD THIS & CHANGE PATH --------
    "com.unity.xr.mock": "file:<PATH_TO_ARFOUNCATION_MOCK>/arfoundation-mock/Assets/Plugins/Unity.XR.Mock",
    // -------------------------
    // ....
  },
  // ...
}
```
4. Re-open Unity Editor.

## Detailed Guidelines ##

->Before using "XR Mock" make sure the scene has "AR Session Origin" and "AR Session" gameObjects
from ARFoundation Unity package.

->The "AR Session Origin" by default contains an "AR Camera" gameObject as a child. Make sure it
is tagged as "Main Camera"

->The Mocking gameObjects such "Mock Plane", "Mock PointCloud" etc contained as a child in
"AR Mock -> Mock Trackables" itself will not be simulated but rather their properties will be copied
and actual simulated objects will be created in "AR Session Origin -> Trackables" as child gameObjcts at runtime.
The prefab used to represent these runtime AR planes can be found at "AR Session Origin -> ARPlaneManager(script) -> m_PlanePrefab(variable)"

->AR Mock -> Mock Trackables (SimulateSession.cs) -> Mock Plane 1, Mock PointCloud
(Contains scripts like-SimulatePlanes.cs, SimulatePointCloud.cs etc)

->Set the position of "AR Mock" and its child "Mock Trackables" at origit (0,0,0). Then position the mocking gameObjects 
such as "Mock Plane", "Mock PointCloud" (which are child of "AR Mock -> Mock Trackables") in front of "AR Camera" by moving
their local position w.r.t parent gameObject "AR Mock -> Mock Trackables" so that mocking gameObjects will be visible when running the scene in Editor.

->"AR Camera" can also be moved and rotated while doing runtime testing.
The movement is controlled by "AR Mock -> Mock Camera (CameraController.cs)" script
See the attached script in the inspector for assigned keys for navigation.

