using Gru.GLTF.Schema;
using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Extensions
{
    /// <summary>
    /// glTF extension that defines the specular-glossiness material model from Physically-Based Rendering (PBR) methodology.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_pbrSpecularGlossiness/schema/glTF.KHR_materials_pbrSpecularGlossiness.schema.json"/>
    /// </summary>
    public class KHR_materials_pbrSpecularGlossiness : GLTFProperty
    {
        [Preserve]
        public KHR_materials_pbrSpecularGlossiness()
        {
            DiffuseFactor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            SpecularFactor = new float[] { 1.0f, 1.0f, 1.0f };
            GlossinessFactor = 1.0f;
        }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// The RGBA components of the reflected diffuse color of the material.
        /// Metals have a diffuse value of `[0.0, 0.0, 0.0]`.
        /// The fourth component (A) is the alpha coverage of the material.
        /// The `alphaMode` property specifies how alpha is interpreted. The values are linear.
        /// </summary>
        [JsonProperty(PropertyName = "diffuseFactor")]
        public float[] DiffuseFactor
        {
            get => diffuseFactor;
            set
            {
                if (value == null)
                {
                    diffuseFactor = value;
                }
                else if (value.Length != 4 || !Array.TrueForAll(value, v => v >= 0.0f && v <= 1.0f))
                {
                    throw new Exception($"Length of {nameof(DiffuseFactor)} should be 4 and values should lie between [0.0, 1.0]");
                }
                else
                {
                    diffuseFactor = value;
                }
            }
        }

        /// <summary>
        /// The diffuse texture. This texture contains RGB components of the reflected diffuse color
        /// of the material encoded with the sRGB transfer function. If the fourth component (A) is present,
        /// it represents the linear alpha coverage of the material. Otherwise, an alpha of 1.0 is assumed.
        /// The `alphaMode` property specifies how alpha is interpreted. The stored texels must not be premultiplied.
        /// </summary>
        [JsonProperty(PropertyName = "diffuseTexture")]
        public TextureInfo DiffuseTexture { get; set; }

        /// <summary>
        /// The specular RGB color of the material. This value is linear.
        /// </summary>
        [JsonProperty(PropertyName = "specularFactor")]
        public float[] SpecularFactor
        {
            get => specularFactor;
            set
            {
                if (value == null)
                {
                    specularFactor = value;
                }
                else if (value.Length != 3 || !Array.TrueForAll(value, v => v >= 0.0f && v <= 1.0f))
                {
                    throw new Exception($"Length of {nameof(DiffuseFactor)} should be 3 and values should lie between [0.0, 1.0]");
                }
                else
                {
                    specularFactor = value;
                }
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// The glossiness or smoothness of the material.
        /// A value of 1.0 means the material has full glossiness or is perfectly smooth.
        /// A value of 0.0 means the material has no glossiness or is completely rough. This value is linear.
        /// </summary>
        [JsonProperty(PropertyName = "glossinessFactor")]
        public float GlossinessFactor
        {
            get => glossinessFactor;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new Exception($"{nameof(GlossinessFactor)} must lie in [0,0, 1.0]");
                }
                glossinessFactor = value;
            }
        }

        /// <summary>
        /// The specular-glossiness texture is an RGBA texture,
        /// containing the specular color (RGB) encoded with the sRGB transfer function and the linear glossiness value (A).
        /// </summary>
        [JsonProperty(PropertyName = "specularGlossinessTexture")]
        public TextureInfo SpecularGlossinessTexture { get; set; }


        private float[] diffuseFactor;
        private float[] specularFactor;
        private float glossinessFactor;
    }
}