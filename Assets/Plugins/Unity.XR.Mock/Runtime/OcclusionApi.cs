using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.XR.Mock
{
    public static class OcclusionApi
    {
        public static Material material;
        private static TextureInfo[] textures = Array.Empty<TextureInfo>();
        private static TextureInfo environmentDepth;
        private static TextureInfo environmentDepthConfidence;
        private static TextureInfo humanDepth;
        private static TextureInfo humanStencil;

        public static TextureInfo[] GetTextures() => textures;

        public static void SetTextures(
            Texture2D environmentDepth,
            Texture2D environmentDepthConfidence,
            Texture2D humanDepth,
            Texture2D humanStencil)
        {
            var tmp = new List<TextureInfo>();

            if (environmentDepth != null)
            {
                tmp.Add(new TextureInfo()
                {
                    texture = environmentDepth,
                    depth = 1,
                    propertyNameId = UnityXRMockOcclusionSubsystem.k_TextureEnvironmentDepthPropertyId
                });
            }

            if (environmentDepthConfidence != null)
            {
                tmp.Add(new TextureInfo()
                {
                    texture = environmentDepthConfidence,
                    depth = 1,
                    propertyNameId = UnityXRMockOcclusionSubsystem.k_TextureEnvironmentDepthConfidencePropertyId
                });
            }

            if (humanDepth != null)
            {
                tmp.Add(new TextureInfo()
                {
                    texture = humanDepth,
                    depth = 1,
                    propertyNameId = UnityXRMockOcclusionSubsystem.k_TextureHumanDepthPropertyId
                });
            }

            if (humanStencil != null)
            {
                tmp.Add(new TextureInfo()
                {
                    texture = humanStencil,
                    depth = 1,
                    propertyNameId = UnityXRMockOcclusionSubsystem.k_TextureHumanStencilPropertyId
                });
            }

            textures = tmp.ToArray();
        }

        public static bool TryGetEnvironmentDepth(out TextureInfo t) => (t = environmentDepth) != null;
        public static bool TryGetEnvironmentDepthConfidence(out TextureInfo t) => (t = environmentDepthConfidence) != null;
        public static bool TryGetHumanDepth(out TextureInfo t) => (t = humanDepth) != null;
        public static bool TryGetHumanStencil(out TextureInfo t) => (t = humanStencil) != null;

        public sealed class TextureInfo
        {
            public Texture2D texture;

            public int depth;
            public int propertyNameId;
            public TextureDimension dimension => texture.dimension;
            public TextureFormat format => texture.format;
            public int width => texture.width;
            public int height => texture.height;
            public int mipmapCount => texture.mipmapCount;

            internal IntPtr GetNativeTexturePtr() => texture?.GetNativeTexturePtr() ?? IntPtr.Zero;
        }
    }
}
