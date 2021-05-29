using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.XR.Mock
{
    public static class OcclusionApi
    {
        public static readonly List<string> enabledKeywords = new List<string>();
        public static readonly List<string> disabledKeywords = new List<string>();
        private static TextureInfo[] textures = Array.Empty<TextureInfo>();
        private static TextureInfo environmentDepth;
        private static TextureInfo environmentDepthConfidence;
        private static TextureInfo humanDepth;
        private static TextureInfo humanStencil;

        public static TextureInfo[] GetTextures() => textures;

        public static void SetTextures(
            List<Texture2D> textures,
            List<int> propertyNameIds,
            List<string> enabledMaterialKeywords,
            List<string> disabledMaterialKeywords)
        {
            OcclusionApi.textures = textures.Select((m, i) => new TextureInfo
            {
                texture = m,
                depth = 1,
                propertyNameId = propertyNameIds[i]
            }).ToArray();

            enabledKeywords.AddRange(enabledMaterialKeywords);
            disabledKeywords.AddRange(disabledMaterialKeywords);
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
