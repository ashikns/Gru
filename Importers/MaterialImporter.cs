using Gru.Extensions;
using Gru.Loaders;
using Gru.MaterialMaps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class MaterialImporter
    {
        private readonly IList<GLTF.Schema.Material> _materialSchemas;
        private readonly TextureImporter _textureImporter;

        private readonly Func<IMetallicRoughnessMap> _metalRoughFactory;
        private readonly Func<ISpecularGlossinessMap> _specGlossFactory;

        private readonly Lazy<Task<Material>>[] _materials;

        public MaterialImporter(
            IList<GLTF.Schema.Material> materials,
            TextureImporter textureImporter,
            Func<IMetallicRoughnessMap> metalRoughFactory,
            Func<ISpecularGlossinessMap> specGlossFactory)
        {
            _materialSchemas = materials;
            _textureImporter = textureImporter;

            _materials = new Lazy<Task<Material>>[_materialSchemas?.Count ?? 0];

            _metalRoughFactory = metalRoughFactory;
            _specGlossFactory = specGlossFactory;
        }

        // Should be run from main thread
        public Task<Material> GetMaterialAsync(GLTF.Schema.GLTFId materialId)
        {
            return _materials.ThreadSafeGetOrAdd(
                materialId.Key, () => ConstructMaterial(_materialSchemas[materialId.Key]));
        }

        private async Task<Material> ConstructMaterial(GLTF.Schema.Material materialSchema)
        {
            IBaseMaterialMap materialMap;

            if (materialSchema.Extensions != null &&
                materialSchema.Extensions.ContainsKey(GLTF.Schema.GLTFExtension.KHR_materials_pbrSpecularGlossiness))
            {
                if (!(materialSchema.Extensions[GLTF.Schema.GLTFExtension.KHR_materials_pbrSpecularGlossiness]
                    is GLTF.Extensions.KHR_materials_pbrSpecularGlossiness specGlossSchema))
                {
                    throw new Exception($"Expected instance of type {nameof(GLTF.Extensions.KHR_materials_pbrSpecularGlossiness)}");
                }

                var specGlossMap = _specGlossFactory.Invoke();

                specGlossMap.DiffuseFactor = specGlossSchema.DiffuseFactor.ToUnityColor();
                specGlossMap.SpecularFactor = specGlossSchema.SpecularFactor.ToUnityVector3Raw();
                specGlossMap.GlossinessFactor = specGlossSchema.GlossinessFactor;

                if (specGlossSchema.DiffuseTexture != null)
                {
                    var texture = await _textureImporter.GetTextureAsync(specGlossSchema.DiffuseTexture.Index, TextureTarget.Diffuse);

                    specGlossMap.DiffuseTexture = texture;
                    specGlossMap.DiffuseTexCoord = specGlossSchema.DiffuseTexture.TexCoord;
                }

                if (specGlossSchema.SpecularGlossinessTexture != null)
                {
                    var texture = await _textureImporter.GetTextureAsync(specGlossSchema.SpecularGlossinessTexture.Index, TextureTarget.Specular);

                    specGlossMap.SpecularGlossinessTexture = texture;
                    specGlossMap.SpecularGlossinessTexCoord = specGlossSchema.SpecularGlossinessTexture.TexCoord;
                }

                materialMap = specGlossMap;
            }
            else
            {
                var metallicSchema = materialSchema.PbrMetallicRoughness;

                var metallicMap = _metalRoughFactory.Invoke();

                metallicMap.BaseColorFactor = metallicSchema.BaseColorFactor.ToUnityColor();
                metallicMap.MetallicFactor = metallicSchema.MetallicFactor;
                metallicMap.RoughnessFactor = metallicSchema.RoughnessFactor;

                if (metallicSchema.BaseColorTexture != null)
                {
                    var texture = await _textureImporter.GetTextureAsync(metallicSchema.BaseColorTexture.Index, TextureTarget.Diffuse);

                    metallicMap.BaseColorTexture = texture;
                    metallicMap.BaseColorTexCoord = metallicSchema.BaseColorTexture.TexCoord;
                }

                if (metallicSchema.MetallicRoughnessTexture != null)
                {
                    var texture = await _textureImporter.GetTextureAsync(metallicSchema.MetallicRoughnessTexture.Index, TextureTarget.Metal);

                    metallicMap.MetallicRoughnessTexture = texture;
                    metallicMap.BaseColorTexCoord = metallicSchema.MetallicRoughnessTexture.TexCoord;
                }

                materialMap = metallicMap;
            }

            materialMap.Material.name = materialSchema.Name;
            materialMap.AlphaMode = materialSchema.AlphaMode;
            materialMap.AlphaCutoff = materialSchema.AlphaCutoff;

            materialMap.EmissiveFactor = materialSchema.EmissiveFactor.ToUnityColor();

            if (materialSchema.NormalTexture != null)
            {
                var texture = await _textureImporter.GetTextureAsync(materialSchema.NormalTexture.Index, TextureTarget.Normal);

                materialMap.NormalTexture = texture;
                materialMap.NormalTexCoord = materialSchema.NormalTexture.TexCoord;
                materialMap.NormalTexScale = materialSchema.NormalTexture.Scale;
            }

            if (materialSchema.OcclusionTexture != null)
            {
                var texture = await _textureImporter.GetTextureAsync(materialSchema.OcclusionTexture.Index, TextureTarget.Occlusion);

                materialMap.OcclusionTexture = texture;
                materialMap.OcclusionTexCoord = materialSchema.OcclusionTexture.TexCoord;
                materialMap.OcclusionTexStrength = materialSchema.OcclusionTexture.Strength;
            }

            if (materialSchema.EmissiveTexture != null)
            {
                var texture = await _textureImporter.GetTextureAsync(materialSchema.EmissiveTexture.Index, TextureTarget.Emission);

                materialMap.EmissiveTexture = texture;
                materialMap.EmissiveTexCoord = materialSchema.EmissiveTexture.TexCoord;
            }

            return materialMap.Material;
        }
    }
}