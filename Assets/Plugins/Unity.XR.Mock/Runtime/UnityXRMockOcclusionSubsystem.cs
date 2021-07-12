using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Mock
{
    [Preserve]
    public sealed class UnityXRMockOcclusionSubsystem : XROcclusionSubsystem
    {
        /// <summary>
        /// The shader property name for the human segmentation stencil texture.
        /// </summary>
        /// <value>
        /// The shader property name for the human segmentation stencil texture.
        /// </value>
        internal const string k_TextureHumanStencilPropertyName = "_HumanStencil";

        /// <summary>
        /// The shader property name for the human segmentation depth texture.
        /// </summary>
        /// <value>
        /// The shader property name for the human segmentation depth texture.
        /// </value>
        internal const string k_TextureHumanDepthPropertyName = "_HumanDepth";

        /// <summary>
        /// The shader property name for the environment depth texture.
        /// </summary>
        /// <value>
        /// The shader property name for the environment depth texture.
        /// </value>
        internal const string k_TextureEnvironmentDepthPropertyName = "_EnvironmentDepth";

        /// <summary>
        /// The shader property name for the environment depth confidence texture.
        /// </summary>
        /// <value>
        /// The shader property name for the environment depth confidence texture.
        /// </value>
        internal const string k_TextureEnvironmentDepthConfidencePropertyName = "_EnvironmentDepthConfidence";

        /// <summary>
        /// The shader keyword for enabling human segmentation rendering.
        /// </summary>
        /// <value>
        /// The shader keyword for enabling human segmentation rendering.
        /// </value>
        internal const string k_HumanEnabledMaterialKeyword = "MOCK_HUMAN_SEGMENTATION_ENABLED";

        /// <summary>
        /// The shader keyword for enabling environment depth rendering.
        /// </summary>
        /// <value>
        /// The shader keyword for enabling environment depth rendering.
        /// </value>
        internal const string k_EnvironmentDepthEnabledMaterialKeyword = "ARCORE_ENVIRONMENT_DEPTH_ENABLED";

        /// <summary>
        /// The shader property name identifier for the human segmentation stencil texture.
        /// </summary>
        /// <value>
        /// The shader property name identifier for the human segmentation stencil texture.
        /// </value>
        internal static readonly int k_TextureHumanStencilPropertyId = Shader.PropertyToID(k_TextureHumanStencilPropertyName);

        /// <summary>
        /// The shader property name identifier for the human segmentation depth texture.
        /// </summary>
        /// <value>
        /// The shader property name identifier for the human segmentation depth texture.
        /// </value>
        internal static readonly int k_TextureHumanDepthPropertyId = Shader.PropertyToID(k_TextureHumanDepthPropertyName);

        /// <summary>
        /// The shader property name identifier for the environment depth texture.
        /// </summary>
        /// <value>
        /// The shader property name identifier for the environment depth texture.
        /// </value>
        internal static readonly int k_TextureEnvironmentDepthPropertyId = Shader.PropertyToID(k_TextureEnvironmentDepthPropertyName);

        /// <summary>
        /// The shader property name identifier for the environment depth texture.
        /// </summary>
        /// <value>
        /// The shader property name identifier for the environment depth texture.
        /// </value>
        internal static readonly int k_TextureEnvironmentDepthConfidencePropertyId = Shader.PropertyToID(k_TextureEnvironmentDepthConfidencePropertyName);

        internal static readonly List<string> m_NoKeywords = new List<string>();
        internal static readonly List<string> m_HumanEnabledMaterialKeywords = new List<string>() { k_HumanEnabledMaterialKeyword };
        internal static readonly List<string> m_EnvironmentDepthEnabledMaterialKeywords = new List<string>() { k_EnvironmentDepthEnabledMaterialKeyword };
        internal static readonly List<string> m_AllDisabledMaterialKeywords = new List<string>() { k_HumanEnabledMaterialKeyword, k_EnvironmentDepthEnabledMaterialKeyword };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Register()
        {
            XROcclusionSubsystem.Register(new XROcclusionSubsystemCinfo
            {
                id = typeof(UnityXRMockOcclusionSubsystem).FullName,
                providerType = typeof(MockProvider),
                subsystemTypeOverride = typeof(UnityXRMockOcclusionSubsystem),
                queryForSupportsEnvironmentDepthConfidenceImage = () => true,
                queryForSupportsEnvironmentDepthImage = () => true,
                supportsHumanSegmentationDepthImage = true,
                supportsHumanSegmentationStencilImage = true
            });
        }

        private class MockProvider : Provider
        {
            private XRTextureDescriptor[] descriptors;
            private OcclusionApi.TextureInfo[] previousDescriptors;

            [Preserve]
            public MockProvider() { }

            public override void Start() { }

            public override void Stop() { }

            public override void Destroy()
            {
                DepthApi.Reset();
            }

            public override HumanSegmentationDepthMode currentHumanDepthMode => requestedHumanDepthMode;

            public override EnvironmentDepthMode currentEnvironmentDepthMode => requestedEnvironmentDepthMode;

            public override HumanSegmentationStencilMode currentHumanStencilMode => requestedHumanStencilMode;

            public override OcclusionPreferenceMode currentOcclusionPreferenceMode => requestedOcclusionPreferenceMode;

            public override EnvironmentDepthMode requestedEnvironmentDepthMode { get; set; }

            public override HumanSegmentationDepthMode requestedHumanDepthMode { get; set; }

            public override HumanSegmentationStencilMode requestedHumanStencilMode { get; set; }

            public override OcclusionPreferenceMode requestedOcclusionPreferenceMode { get; set; }

            /* TODO */
            public override XRCpuImage.Api environmentDepthConfidenceCpuImageApi => base.environmentDepthConfidenceCpuImageApi;
            /* TODO */
            public override XRCpuImage.Api environmentDepthCpuImageApi => base.environmentDepthCpuImageApi;
            /* TODO */
            public override XRCpuImage.Api humanDepthCpuImageApi => base.humanDepthCpuImageApi;
            /* TODO */
            public override XRCpuImage.Api humanStencilCpuImageApi => base.humanStencilCpuImageApi;

            public override NativeArray<XRTextureDescriptor> GetTextureDescriptors(XRTextureDescriptor defaultDescriptor, Allocator allocator)
            {
                //var currentDescriptors = OcclusionApi.GetTextures();
                //if (previousDescriptors != currentDescriptors)
                //{
                //    descriptors = currentDescriptors.Select(m => GetTextureDescriptor(m).Convert()).ToArray();
                //    previousDescriptors = currentDescriptors;
                //}
                //
                //return new NativeArray<XRTextureDescriptor>(descriptors, allocator);
                return base.GetTextureDescriptors(defaultDescriptor, allocator);
            }

            public override void GetMaterialKeywords(out List<string> enabledKeywords, out List<string> disabledKeywords)
            {
                var enableEnvironment = (OcclusionApi.TryGetEnvironmentDepth(out var environmentDepth) && environmentDepth != null);
                var enableHuman = (OcclusionApi.TryGetHumanDepth(out var humanDepth) && humanDepth != null);

                if (enableEnvironment && enableHuman)
                {
                    enabledKeywords = m_AllDisabledMaterialKeywords;
                    disabledKeywords = m_NoKeywords;
                }
                else if (enableEnvironment)
                {
                    enabledKeywords = m_EnvironmentDepthEnabledMaterialKeywords;
                    disabledKeywords = m_HumanEnabledMaterialKeywords;
                }
                else if (enableHuman)
                {
                    enabledKeywords = m_HumanEnabledMaterialKeywords;
                    disabledKeywords = m_EnvironmentDepthEnabledMaterialKeywords;
                }
                else
                {
                    enabledKeywords = m_NoKeywords;
                    disabledKeywords = m_AllDisabledMaterialKeywords;
                }
            }

            /* TODO */
            public override bool TryAcquireEnvironmentDepthConfidenceCpuImage(out XRCpuImage.Cinfo cinfo) => base.TryAcquireEnvironmentDepthConfidenceCpuImage(out cinfo);
            /* TODO */
            public override bool TryAcquireEnvironmentDepthCpuImage(out XRCpuImage.Cinfo cinfo) => base.TryAcquireEnvironmentDepthCpuImage(out cinfo);
            /* TODO */
            public override bool TryAcquireHumanDepthCpuImage(out XRCpuImage.Cinfo cinfo) => base.TryAcquireHumanDepthCpuImage(out cinfo);
            /* TODO */
            public override bool TryAcquireHumanStencilCpuImage(out XRCpuImage.Cinfo cinfo) => base.TryAcquireHumanStencilCpuImage(out cinfo);

            public override bool TryGetEnvironmentDepth(out XRTextureDescriptor environmentDepthDescriptor)
            {
                if (OcclusionApi.TryGetEnvironmentDepth(out var t))
                {
                    GetTextureDescriptor(t).Convert(out environmentDepthDescriptor);
                    return true;
                }

                environmentDepthDescriptor = new XRTextureDescriptor();
                return false;
            }

            public override bool TryGetEnvironmentDepthConfidence(out XRTextureDescriptor environmentDepthConfidenceDescriptor)
            {
                if (OcclusionApi.TryGetEnvironmentDepthConfidence(out var t))
                {
                    GetTextureDescriptor(t).Convert(out environmentDepthConfidenceDescriptor);
                    return true;
                }

                environmentDepthConfidenceDescriptor = new XRTextureDescriptor();
                return false;
            }

            public override bool TryGetHumanDepth(out XRTextureDescriptor humanDepthDescriptor)
            {
                if (OcclusionApi.TryGetHumanDepth(out var t))
                {
                    GetTextureDescriptor(t).Convert(out humanDepthDescriptor);
                    return true;
                }

                humanDepthDescriptor = new XRTextureDescriptor();
                return false;
            }

            public override bool TryGetHumanStencil(out XRTextureDescriptor humanStencilDescriptor)
            {
                if (OcclusionApi.TryGetHumanStencil(out var t))
                {
                    GetTextureDescriptor(t).Convert(out humanStencilDescriptor);
                    return true;
                }

                humanStencilDescriptor = new XRTextureDescriptor();
                return false;
            }

            private XRTextureDescriptorMock GetTextureDescriptor(OcclusionApi.TextureInfo t)
            {
                return new XRTextureDescriptorMock
                {
                    m_Depth = t.depth,
                    m_Dimension = t.dimension,
                    m_Format = t.format,
                    m_Width = t.width,
                    m_Height = t.height,
                    m_MipmapCount = t.mipmapCount,
                    m_PropertyNameId = t.propertyNameId,
                    m_NativeTexture = t.GetNativeTexturePtr()
                };
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XRTextureDescriptorMock
        {
            /// <summary>
            /// A pointer to the native texture object.
            /// </summary>
            /// <value>
            /// A pointer to the native texture object.
            /// </value>
            public IntPtr m_NativeTexture;

            /// <summary>
            /// Specifies the width dimension of the native texture object.
            /// </summary>
            /// <value>
            /// The width of the native texture object.
            /// </value>
            public int m_Width;

            /// <summary>
            /// Specifies the height dimension of the native texture object.
            /// </summary>
            /// <value>
            /// The height of the native texture object.
            /// </value>
            public int m_Height;

            /// <summary>
            /// Specifies the number of mipmap levels in the native texture object.
            /// </summary>
            /// <value>
            /// The number of mipmap levels in the native texture object.
            /// </value>
            public int m_MipmapCount;

            /// <summary>
            /// Specifies the texture format of the native texture object.
            /// </summary>
            /// <value>
            /// The format of the native texture object.
            /// </value>
            public TextureFormat m_Format;

            /// <summary>
            /// Specifies the unique shader property name ID for the material shader texture.
            /// </summary>
            /// <value>
            /// The unique shader property name ID for the material shader texture.
            /// </value>
            /// <remarks>
            /// Use the static method <c>Shader.PropertyToID(string name)</c> to get the unique identifier.
            /// </remarks>
            public int m_PropertyNameId;

            /// <summary>
            /// This specifies the depth dimension of the native texture. For a 3D texture, depth would be greater than zero.
            /// For any other kind of valid texture, depth is one.
            /// </summary>
            /// <value>
            /// The depth dimension of the native texture object.
            /// </value>
            public int m_Depth;

            /// <summary>
            /// Specifies the [texture dimension](https://docs.unity3d.com/ScriptReference/Rendering.TextureDimension.html) of the native texture object.
            /// </summary>
            /// <value>
            /// The texture dimension of the native texture object.
            /// </value>
            public TextureDimension m_Dimension;

            public unsafe XRTextureDescriptor Convert()
            {
                var result = new XRTextureDescriptor();
                Convert(out result);
                return result;
            }

            public unsafe void Convert(out XRTextureDescriptor target)
            {
                fixed (XRTextureDescriptor* targetPtr = &target)
                fixed (XRTextureDescriptorMock* selfPtr = &this)
                {
                    UnsafeUtility.MemCpy(targetPtr, selfPtr, sizeof(XRTextureDescriptor));
                }
            }
        }
    }
}
