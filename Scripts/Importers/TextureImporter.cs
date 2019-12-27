using Gru.Extensions;
using Gru.Helpers;
using Gru.Loaders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class TextureImporter
    {
        private readonly IList<GLTF.Schema.Texture> _textureSchemas;
        private readonly IList<GLTF.Schema.Image> _imageSchemas;
        private readonly IList<GLTF.Schema.Sampler> _samplerSchemas;
        private readonly BufferImporter _bufferImporter;
        private readonly ITextureLoader _textureLoader;

        private readonly Lazy<Task<Texture2D>>[] _textures;

        public TextureImporter(
            IList<GLTF.Schema.Texture> textures,
            IList<GLTF.Schema.Image> images,
            IList<GLTF.Schema.Sampler> samplers,
            BufferImporter bufferImporter,
            ITextureLoader textureLoader)
        {
            _textureSchemas = textures;
            _imageSchemas = images;
            _samplerSchemas = samplers;
            _bufferImporter = bufferImporter;
            _textureLoader = textureLoader;

            _textures = new Lazy<Task<Texture2D>>[_textureSchemas.Count];
        }

        public Task<Texture2D> GetTextureAsync(GLTF.Schema.GLTFId textureId, bool isLInear)
        {
            return _textures.ThreadSafeGetOrAdd(
                textureId.Key, () => ConstructTexture(_textureSchemas[textureId.Key], isLInear));
        }

        private async Task<Texture2D> ConstructTexture(GLTF.Schema.Texture textureSchema, bool isLinear)
        {
            var image = _imageSchemas[textureSchema.Source.Key];
            Texture2D texture;

            if (!string.IsNullOrEmpty(image.Uri))
            {
                if (UriHelper.TryParseDataUri(image.Uri, out var embeddedData))
                {
                    texture = await _textureLoader.CreateTexture(new ArraySegment<byte>(embeddedData), image.MimeType.Value.ToString(), isLinear);
                }
                else
                {
                    texture = await _textureLoader.CreateTexture(image.Uri, isLinear);
                }
            }
            else if (image.BufferView != null)
            {
                var bufferView = await Task.Run(() => _bufferImporter.GetBufferViewAsync(image.BufferView));
                return await _textureLoader.CreateTexture(bufferView.Data, image.MimeType.Value.ToString(), isLinear);
            }
            else
            {
                throw new Exception($"Data source for image {image.Name} not found.");
            }

            texture.name = textureSchema.Name;

            if (textureSchema.Sampler != null)
            {
                var sampler = _samplerSchemas[textureSchema.Sampler.Key];
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