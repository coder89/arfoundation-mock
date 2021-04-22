# Unity.XR.Mock for ARFoundation 4.x

[![openupm](https://img.shields.io/npm/v/com.unity.xr.mock?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.unity.xr.mock/)

Unity Editor plugin that let's you mock AR environment for development &amp; testing on your PC using Unity 2021.1 and AR Foundation 4.x

# How to use it #

## Release package ##

1. Import latest unity package from: https://github.com/coder89/arfoundation-mock/releases
2. Add mocked trackables using scripts provided in "/Assets/Plugins/Unity.XR.Mock/Editor". See "AR Mock" GameObject in "SampleScene" for reference.

## Unity Package Manager ##

The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.unity.xr.mock
```

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
