# Unity.XR.Mock for ARFoundation 2.x

Unity Editor plugin that let's you mock AR environment for development &amp; testing on your PC using Unity 2019 and AR Foundation 2.x

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
