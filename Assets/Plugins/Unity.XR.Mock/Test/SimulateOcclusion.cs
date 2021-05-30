using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Mock
{
    public sealed class SimulateOcclusion : MonoBehaviour
    {
        public Texture2D environmentDepth;
        public Texture2D environmentDepthConfidence;
        public Texture2D humanDepth;
        public Texture2D humanStencil;
        public Material material;

        public void Start()
        {
            OcclusionApi.material = material;

            OcclusionApi.SetTextures(
                environmentDepth: null,
                environmentDepthConfidence: null,
                humanDepth: null,
                humanStencil: null);
        }
    }
}
