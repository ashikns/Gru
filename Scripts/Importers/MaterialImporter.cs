using Gru.Extensions;
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

        private readonly Lazy<Task<Material>>[] _materials;

        public MaterialImporter(
            IList<GLTF.Schema.Material> materials,
            TextureImporter textureImporter)
        {
            _materialSchemas = materials;
            _textureImporter = textureImporter;

            _materials = new Lazy<Task<Material>>[_materialSchemas.Count];
        }

        // Should be run from main thread
        public Task<Material> GetMaterialAsync(GLTF.Schema.GLTFId materialId)
        {
            return _materials.ThreadSafeGetOrAdd(
                materialId.Key, () => ConstructMaterial(_materialSchemas[materialId.Key], _textureImporter));
        }

        private static async Task<Material> ConstructMaterial(
            GLTF.Schema.Material materialSchema,
            TextureImporter textureImporter)
        {
            BaseMaterialMap materialMap;

            if (materialSchema.Extensions != null &&
                materialSchema.Extensions.ContainsKey(GLTF.Schema.GLTFExtension.KHR_materials_pbrSpecularGlossiness))
            {
                if (!(materialSchema.Extensions[GLTF.Schema.GLTFExtension.KHR_materials_pbrSpecularGlossiness]
                    is GLTF.Extensions.KHR_materials_pbrSpecularGlossiness specGlossSchema))
                {
                    throw new Exception($"Expected instance of type {nameof(GLTF.Extensions.KHR_materials_pbrSpecularGlossiness)}");
                }

                var specGlossMap = new SpecularGlossinessMap
                {
                    DiffuseFactor = specGlossSchema.DiffuseFactor.ToUnityColor(),
                    SpecularFactor = specGlossSchema.SpecularFactor.ToUnityVector3Raw(),
                    GlossinessFactor = specGlossSchema.GlossinessFactor
                };

                if (specGlossSchema.DiffuseTexture != null)
                {
                    var texture = await textureImporter.GetTextureAsync(specGlossSchema.DiffuseTexture.Index, false);

                    specGlossMap.DiffuseTexture = texture;
                    specGlossMap.DiffuseTexCoord = specGlossSchema.DiffuseTexture.TexCoord;
                }

                if (specGlossSchema.SpecularGlossinessTexture != null)
                {
                    var texture = await textureImporter.GetTextureAsync(specGlossSchema.SpecularGlossinessTexture.Index, false);

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
                    var texture = await textureImporter.GetTextureAsync(metallicSchema.BaseColorTexture.Index, false);

                    metallicMap.BaseColorTexture = texture;
                    metallicMap.BaseColorTexCoord = metallicSchema.BaseColorTexture.TexCoord;
                }

                if (metallicSchema.MetallicRoughnessTexture != null)
                {
                    var texture = await textureImporter.GetTextureAsync(metallicSchema.MetallicRoughnessTexture.Index, false);

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
                var texture = await textureImporter.GetTextureAsync(materialSchema.NormalTexture.Index, true);

                materialMap.NormalTexture = texture;
                materialMap.NormalTexCoord = materialSchema.NormalTexture.TexCoord;
                materialMap.NormalTexScale = materialSchema.NormalTexture.Scale;
            }

            if (materialSchema.OcclusionTexture != null)
            {
                var texture = await textureImporter.GetTextureAsync(materialSchema.OcclusionTexture.Index, true);

                materialMap.OcclusionTexture = texture;
                materialMap.OcclusionTexCoord = materialSchema.OcclusionTexture.TexCoord;
                materialMap.OcclusionTexStrength = materialSchema.OcclusionTexture.Strength;
            }

            if (materialSchema.EmissiveTexture != null)
            {
                var texture = await textureImporter.GetTextureAsync(materialSchema.EmissiveTexture.Index, false);

                materialMap.EmissiveTexture = texture;
                materialMap.EmissiveTexCoord = materialSchema.EmissiveTexture.TexCoord;
            }

            return materialMap.Material;
        }
    }
}