using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// The material appearance of a primitive.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/material.schema.json"/>
    /// </summary>
    public class Material : GLTFChildOfRootProperty
    {
        [Preserve]
        public Material()
        {
            PbrMetallicRoughness = new MaterialPbrMetallicRoughness();
            EmissiveFactor = new float[] { 0.0f, 0.0f, 0.0f };
            AlphaMode = AlphaMode.OPAQUE;
            AlphaCutoff = 0.5f;
            DoubleSided = false;
        }

        /// <summary>
        /// A set of parameter values that are used to define the metallic-roughness material model
        /// from Physically-Based Rendering (PBR) methodology. When not specified,
        /// all the default values of `pbrMetallicRoughness` apply.
        /// </summary>
        [JsonProperty(PropertyName = "pbrMetallicRoughness")]
        public MaterialPbrMetallicRoughness PbrMetallicRoughness
        {
            get => pbrMetallicRoughness;
            set
            {
                pbrMetallicRoughness = value ?? throw new Exception($"{nameof(MaterialPbrMetallicRoughness)} cannot be null");
            }
        }

        /// <summary>
        /// A tangent space normal map. The texture contains RGB components in linear space.
        /// Each texel represents the XYZ components of a normal vector in tangent space.
        /// Red [0 to 255] maps to X [-1 to 1]. Green [0 to 255] maps to Y [-1 to 1]. Blue [128 to 255] maps to Z [1/255 to 1].
        /// The normal vectors use OpenGL conventions where +X is right and +Y is up. +Z points toward the viewer.
        /// Client implementations should normalize the normal vectors before using them in lighting equations.
        /// </summary>
        [JsonProperty(PropertyName = "normalTexture")]
        public MaterialNormalTextureInfo NormalTexture { get; set; }

        /// <summary>
        /// The occlusion map texture. The occlusion values are sampled from the R channel.
        /// Higher values indicate areas that should receive full indirect lighting
        /// and lower values indicate no indirect lighting. These values are linear.
        /// If other channels are present (GBA), they are ignored for occlusion calculations.
        /// </summary>
        [JsonProperty(PropertyName = "occlusionTexture")]
        public MaterialOcclusionTextureInfo OcclusionTexture { get; set; }

        /// <summary>
        /// The emissive map controls the color and intensity of the light being emitted by the material.
        /// This texture contains RGB components encoded with the sRGB transfer function.
        /// If a fourth component (A) is present, it is ignored.
        /// </summary>
        [JsonProperty(PropertyName = "emissiveTexture")]
        public TextureInfo EmissiveTexture { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// The RGB components of the emissive color of the material. These values are linear.
        /// If an emissiveTexture is specified, this value is multiplied with the texel values.
        /// </summary>
        [JsonProperty(PropertyName = "emissiveFactor")]
        public float[] EmissiveFactor
        {
            get => emissiveFactor;
            set
            {
                if (value != null && value.Length != 3)
                {
                    throw new Exception($"Length of {nameof(EmissiveFactor)} must be 3");
                }
                if (value != null && !Array.TrueForAll(value, v => v >= 0.0f && v <= 1.0f))
                {
                    throw new Exception($"Elements of {nameof(EmissiveFactor)} must lie between 0.0 and 1.0");
                }
                emissiveFactor = value;
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// The material's alpha rendering mode enumeration specifying the interpretation of the alpha value of the main factor and texture.
        /// </summary>
        [JsonProperty(PropertyName = "alphaMode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AlphaMode AlphaMode { get; set; }

        /// <summary>
        /// Specifies the cutoff threshold when in `MASK` mode. If the alpha value is greater than or equal to this value
        /// then it is rendered as fully opaque, otherwise, it is rendered as fully transparent.
        /// A value greater than 1.0 will render the entire material as fully transparent.
        /// This value is ignored for other modes.
        /// </summary>
        [JsonProperty(PropertyName = "alphaCutoff")]
        public float AlphaCutoff
        {
            get => alphaCutoff;
            set
            {
                if (value < 0) { throw new Exception($"{nameof(AlphaCutoff)} cannot be less than zero."); }
                alphaCutoff = value;
            }
        }

        /// <summary>
        /// Specifies whether the material is double sided. When this value is false, back-face culling is enabled.
        /// When this value is true, back-face culling is disabled and double sided lighting is enabled.
        /// The back-face must have its normals reversed before the lighting equation is evaluated.
        /// </summary>
        [JsonProperty(PropertyName = "doubleSided")]
        public bool DoubleSided { get; set; }


        private MaterialPbrMetallicRoughness pbrMetallicRoughness;
        private float[] emissiveFactor;
        private float alphaCutoff;
    }

    /// <summary>
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/material.normalTextureInfo.schema.json"/>
    /// </summary>
    public class MaterialNormalTextureInfo : TextureInfo
    {
        [Preserve]
        public MaterialNormalTextureInfo()
        {
            Scale = 1.0f;
        }

        /// <summary>
        /// The scalar multiplier applied to each normal vector of the texture.
        /// This value is ignored if normalTexture is not specified. This value is linear.
        /// </summary>
        [JsonProperty(PropertyName = "scale")]
        public float Scale { get; set; }
    }

    /// <summary>
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/material.occlusionTextureInfo.schema.json"/>
    /// </summary>
    public class MaterialOcclusionTextureInfo : TextureInfo
    {
        [Preserve]
        public MaterialOcclusionTextureInfo()
        {
            Strength = 1.0f;
        }

        /// <summary>
        /// A scalar multiplier controlling the amount of occlusion applied.
        /// A value of 0.0 means no occlusion. A value of 1.0 means full occlusion.
        /// This value is ignored if the corresponding texture is not specified. This value is linear.
        /// </summary>
        [JsonProperty(PropertyName = "strength")]
        public float Strength
        {
            get => strength;
            set
            {
                if (value < 0 || value > 1) { throw new Exception($"{nameof(Strength)} should be between 0.0 and 1.0"); }
                strength = value;
            }
        }


        private float strength;
    }

    /// <summary>
    /// A set of parameter values that are used to define the metallic-roughness material model
    /// from Physically-Based Rendering (PBR) methodology.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/material.pbrMetallicRoughness.schema.json"/>
    /// </summary>
    public class MaterialPbrMetallicRoughness : GLTFProperty
    {
        [Preserve]
        public MaterialPbrMetallicRoughness()
        {
            BaseColorFactor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            MetallicFactor = 1.0f;
            RoughnessFactor = 1.0f;
        }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// The RGBA components of the base color of the material.
        /// The fourth component (A) is the alpha coverage of the material.
        /// The `alphaMode` property specifies how alpha is interpreted. These values are linear.
        /// If a baseColorTexture is specified, this value is multiplied with the texel values.
        /// </summary>
        [JsonProperty(PropertyName = "baseColorFactor")]
        public float[] BaseColorFactor
        {
            get => baseColorFactor;
            set
            {
                if (value != null && value.Length != 4)
                {
                    throw new Exception($"Length of {nameof(BaseColorFactor)} must be 4");
                }
                if (value != null && !Array.TrueForAll(value, v => v >= 0.0f && v <= 1.0f))
                {
                    throw new Exception($"Elements of {nameof(BaseColorFactor)} must lie between 0.0 and 1.0");
                }
                baseColorFactor = value;
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// The base color texture. The first three components (RGB) are encoded with the sRGB transfer function.
        /// They specify the base color of the material. If the fourth component (A) is present,
        /// it represents the linear alpha coverage of the material. Otherwise, an alpha of 1.0 is assumed.
        /// The `alphaMode` property specifies how alpha is interpreted. The stored texels must not be premultiplied.
        /// </summary>
        [JsonProperty(PropertyName = "baseColorTexture")]
        public TextureInfo BaseColorTexture { get; set; }

        /// <summary>
        /// The metalness of the material. A value of 1.0 means the material is a metal. A value of 0.0 means the material is a dielectric.
        /// Values in between are for blending between metals and dielectrics such as dirty metallic surfaces.
        /// This value is linear. If a metallicRoughnessTexture is specified, this value is multiplied with the metallic texel values.
        /// </summary>
        [JsonProperty(PropertyName = "metallicFactor")]
        public float MetallicFactor
        {
            get => metallicFactor;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new Exception($"Value of {nameof(MetallicFactor)} must lie between 0.0 and 1.0");
                }
                metallicFactor = value;
            }
        }

        /// <summary>
        /// The roughness of the material. A value of 1.0 means the material is completely rough. A value of 0.0 means the material is completely smooth.
        /// This value is linear. If a metallicRoughnessTexture is specified, this value is multiplied with the roughness texel values.
        /// </summary>
        [JsonProperty(PropertyName = "roughnessFactor")]
        public float RoughnessFactor
        {
            get => roughnessFactor;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new Exception($"Value of {nameof(RoughnessFactor)} must lie between 0.0 and 1.0");
                }
                roughnessFactor = value;
            }
        }

        /// <summary>
        /// The metallic-roughness texture. The metalness values are sampled from the B channel.
        /// The roughness values are sampled from the G channel. These values are linear.
        /// If other channels are present (R or A), they are ignored for metallic-roughness calculations.
        /// </summary>
        [JsonProperty(PropertyName = "metallicRoughnessTexture")]
        public TextureInfo MetallicRoughnessTexture { get; set; }


        private float[] baseColorFactor;
        private float metallicFactor;
        private float roughnessFactor;
    }

    public enum AlphaMode
    {
        /// <summary>
        /// The alpha value is ignored and the rendered output is fully opaque.
        /// </summary>
        OPAQUE,
        /// <summary>
        /// The rendered output is either fully opaque or fully transparent depending on the alpha value
        /// and the specified alpha cutoff value.
        /// </summary>
        MASK,
        /// <summary>
        /// The alpha value is used to composite the source and destination areas.
        /// The rendered output is combined with the background using the normal painting operation
        /// (i.e. the Porter and Duff over operator).
        /// </summary>
        BLEND
    }
}