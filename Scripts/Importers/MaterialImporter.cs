using Gru.Extensions;
using Gru.MaterialMaps;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class MaterialImporter
    {
        private IList<GLTF.Schema.Material> _materialSchemas;
        private IList<GLTF.Schema.Texture> _textures;
        private IList<GLTF.Schema.Sampler> _samplers;
        private ImageImporter _imageImporter;

        private readonly ConcurrentDictionary<int, Lazy<Task<Material>>> _materials;

        public MaterialImporter()
        {
            _materials = new ConcurrentDictionary<int, Lazy<Task<Material>>>();
        }

        public void Assign(
            IList<GLTF.Schema.Material> materials,
            IList<GLTF.Schema.Texture> textures,
            IList<GLTF.Schema.Sampler> samplers,
            ImageImporter imageImporter)
        {
            _materials.Clear();

            _materialSchemas = materials;
            _textures = textures;
            _samplers = samplers;
            _imageImporter = imageImporter;
        }

        // Should be run from main thread
        public Task<Material> GetMaterialAsync(GLTF.Schema.GLTFId materialId)
        {
            var lazyResult = _materials.GetOrAdd(
                materialId.Key, new Lazy<Task<Material>>(
                    () => ConstructMaterial(_materialSchemas[materialId.Key], _textures, _samplers, _imageImporter)));
            return lazyResult.Value;
        }

        private static async Task<Material> ConstructMaterial(
            GLTF.Schema.Material materialSchema,
            IList<GLTF.Schema.Texture> textures,
            IList<GLTF.Schema.Sampler> samplers,
            ImageImporter imageImporter)
        {
            BaseMaterialMap materialMap;

            if (materialSchema.Extensions != null &&
                materialSchema.Extensions.ContainsKey(GLTF.Schema.GLTFExtension.KHR_materials_pbrSpecularGlossiness))
            {
                if (!(materialSchema.Extensions[GLTF.Schema.GLTFExtension.KHR_materials_pbrSpecularGlossiness]
                    is GLTF.Extensions.KHR_materials_pbrSpecularGlossiness specGlossSchema))
                {
                    throw new System.Exception($"Expected instance of type {nameof(GLTF.Extensions.KHR_materials_pbrSpecularGlossiness)}");
                }

                var specGlossMap = new SpecularGlossinessMap
                {
                    DiffuseFactor = specGlossSchema.DiffuseFactor.ToUnityColor(),
                    SpecularFactor = specGlossSchema.SpecularFactor.ToUnityVector3Raw(),
                    GlossinessFactor = specGlossSchema.GlossinessFactor
                };

                if (specGlossSchema.DiffuseTexture != null)
                {
                    var textureSchema = textures[specGlossSchema.DiffuseTexture.Index.Key];
                    var texture = await ConstructTexture(textureSchema, samplers, imageImporter, false);

                    specGlossMap.DiffuseTexture = texture;
                    specGlossMap.DiffuseTexCoord = specGlossSchema.DiffuseTexture.TexCoord;
                }

                if (specGlossSchema.SpecularGlossinessTexture != null)
                {
                    var textureSchema = textures[specGlossSchema.SpecularGlossinessTexture.Index.Key];
                    var texture = await ConstructTexture(textureSchema, samplers, imageImporter, false);

                    specGlossMap.SpecularGlossinessTexture = texture;
                    specGlossMap.SpecularGlossinessTexCoord = specGlossSchema.SpecularGlossinessTexture.TexCoord;
                }

                materialMap = specGlossMap;
            }
            else
            {
                var metallicSchema = materialSchema.PbrMetallicRoughness;

                var metallicMap = new MetallicRoughnessMap
                {
                    BaseColorFactor = metallicSchema.BaseColorFactor.ToUnityColor(),
                    MetallicFactor = metallicSchema.MetallicFactor,
                    RoughnessFactor = metallicSchema.RoughnessFactor
                };

                if (metallicSchema.BaseColorTexture != null)
                {
                    var textureSchema = textures[metallicSchema.BaseColorTexture.Index.Key];
                    var texture = await ConstructTexture(textureSchema, samplers, imageImporter, false);

                    metallicMap.BaseColorTexture = texture;
                    metallicMap.BaseColorTexCoord = metallicSchema.BaseColorTexture.TexCoord;
                }

                if (metallicSchema.MetallicRoughnessTexture != null)
                {
                    var textureSchema = textures[metallicSchema.MetallicRoughnessTexture.Index.Key];
                    var texture = await ConstructTexture(textureSchema, samplers, imageImporter, false);

                    metallicMap.MetallicRoughnessTexture = texture;
                    metallicMap.BaseColorTexCoord = metallicSchema.MetallicRoughnessTexture.TexCoord;
                }

                materialMap = metallicMap;
            }

            materialMap.Material.name = materialSchema.Name;
            materialMap.AlphaMode = materialSchema.AlphaMode;
            materialMap.AlphaCutoff = materialSchema.AlphaCutoff;
            materialMap.VertexColorsEnabled = true;

            materialMap.EmissiveFactor = materialSchema.EmissiveFactor.ToUnityColor();

            if (materialSchema.NormalTexture != null)
            {
                var textureSchema = textures[materialSchema.NormalTexture.Index.Key];
                var texture = await ConstructTexture(textureSchema, samplers, imageImporter, true);

                materialMap.NormalTexture = texture;
                materialMap.NormalTexCoord = materialSchema.NormalTexture.TexCoord;
                materialMap.NormalTexScale = materialSchema.NormalTexture.Scale;
            }

            if (materialSchema.OcclusionTexture != null)
            {
                var textureSchema = textures[materialSchema.OcclusionTexture.Index.Key];
                var texture = await ConstructTexture(textureSchema, samplers, imageImporter, true);

                materialMap.OcclusionTexture = texture;
                materialMap.OcclusionTexCoord = materialSchema.OcclusionTexture.TexCoord;
                materialMap.OcclusionTexStrength = materialSchema.OcclusionTexture.Strength;
            }

            if (materialSchema.EmissiveTexture != null)
            {
                var textureSchema = textures[materialSchema.EmissiveTexture.Index.Key];
                var texture = await ConstructTexture(textureSchema, samplers, imageImporter, false);

                materialMap.EmissiveTexture = texture;
                materialMap.EmissiveTexCoord = materialSchema.EmissiveTexture.TexCoord;
            }

            return materialMap.Material;
        }

        private static async Task<Texture2D> ConstructTexture(
            GLTF.Schema.Texture textureSchema,
            IList<GLTF.Schema.Sampler> samplers,
            ImageImporter imageImporter,
            bool isLinear)
        {
            var imageData = await Task.Run(() => imageImporter.GetImageAsync(textureSchema.Source));

            var texture = new Texture2D(0, 0, TextureFormat.RGBA32, true, isLinear);
            texture.LoadImage(imageData, true);
            texture.name = textureSchema.Name;

            if (textureSchema.Sampler != null)
            {
                var sampler = samplers[textureSchema.Sampler.Key];
                texture.filterMode = GetUnityEquivalent(sampler.MinFilter);
                texture.wrapMode = GetUnityEquivalent(sampler.WrapS);
            }

            return texture;
        }

        private static FilterMode GetUnityEquivalent(GLTF.Schema.FilterMode? filterMode)
        {
            if (filterMode == null)
            {
                return FilterMode.Trilinear;
            }

            switch (filterMode.Value)
            {
                case GLTF.Schema.FilterMode.NEAREST:
                case GLTF.Schema.FilterMode.NEAREST_MIPMAP_NEAREST:
                case GLTF.Schema.FilterMode.LINEAR_MIPMAP_NEAREST:
                    return FilterMode.Point;
                case GLTF.Schema.FilterMode.LINEAR:
                case GLTF.Schema.FilterMode.NEAREST_MIPMAP_LINEAR:
                    return FilterMode.Bilinear;
                case GLTF.Schema.FilterMode.LINEAR_MIPMAP_LINEAR:
                    return FilterMode.Trilinear;
                default:
                    return FilterMode.Trilinear;
            }
        }

        private static TextureWrapMode GetUnityEquivalent(GLTF.Schema.WrapMode wrapMode)
        {
            switch (wrapMode)
            {
                case GLTF.Schema.WrapMode.CLAMP_TO_EDGE:
                    return TextureWrapMode.Clamp;
                case GLTF.Schema.WrapMode.MIRRORED_REPEAT:
                    return TextureWrapMode.Mirror;
                case GLTF.Schema.WrapMode.REPEAT:
                    return TextureWrapMode.Repeat;
                default:
                    return TextureWrapMode.Repeat;
            }
        }
    }
}