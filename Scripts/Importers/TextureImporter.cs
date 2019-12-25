using Gru.FileLoaders;
using Gru.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class TextureImporter
    {
        private IList<GLTF.Schema.Texture> _textureSchemas;
        private IList<GLTF.Schema.Image> _imageSchemas;
        private IList<GLTF.Schema.Sampler> _samplerSchemas;
        private BufferImporter _bufferImporter;
        private IFileLoader _fileLoader;

        private readonly ConcurrentDictionary<int, Lazy<Task<Texture2D>>> _images;

        public TextureImporter()
        {
            _images = new ConcurrentDictionary<int, Lazy<Task<Texture2D>>>();
        }

        public void Assign(
            IList<GLTF.Schema.Texture> textures,
            IList<GLTF.Schema.Image> images,
            IList<GLTF.Schema.Sampler> samplers,
            BufferImporter bufferImporter,
            IFileLoader fileLoader)
        {
            _images.Clear();

            _textureSchemas = textures;
            _imageSchemas = images;
            _samplerSchemas = samplers;
            _bufferImporter = bufferImporter;
            _fileLoader = fileLoader;
        }

        public Task<Texture2D> GetTextureAsync(GLTF.Schema.GLTFId textureId, bool isLinearColorSpace)
        {
            var lazyResult = _images.GetOrAdd(
                textureId.Key, new Lazy<Task<Texture2D>>(() => ConstructTexture(
                    _textureSchemas[textureId.Key], isLinearColorSpace, _imageSchemas, _samplerSchemas, _bufferImporter, _fileLoader)));
            return lazyResult.Value;
        }

        private static async Task<Texture2D> ConstructTexture(
            GLTF.Schema.Texture textureSchema,
            bool isLinearColorSpace,
            IList<GLTF.Schema.Image> images,
            IList<GLTF.Schema.Sampler> samplers,
            BufferImporter bufferImporter,
            IFileLoader fileLoader)
        {
            var imageData = await Task.Run(() => ReadImageData(images[textureSchema.Source.Key], bufferImporter, fileLoader));

            var texture = new Texture2D(0, 0, TextureFormat.RGBA32, true, isLinearColorSpace);
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

        private static async Task<byte[]> ReadImageData(
            GLTF.Schema.Image image,
            BufferImporter bufferImporter,
            IFileLoader fileLoader)
        {
            if (!string.IsNullOrEmpty(image.Uri))
            {
                if (UriHelper.TryParseDataUri(image.Uri, out var embeddedData))
                {
                    return embeddedData;
                }
                else
                {
                    if (fileLoader == null)
                    {
                        throw new Exception($"{nameof(fileLoader)} is null. Can't read buffer data.");
                    }

                    using (var stream = await fileLoader.OpenFile(image.Uri))
                    {
                        var bufferData = new byte[stream.Length];
                        if (await stream.ReadAsync(bufferData, 0, (int)stream.Length) != stream.Length)
                        {
                            throw new Exception($"Failed to read buffer data");
                        }
                        return bufferData;
                    }
                }
            }
            else if (image.BufferView != null)
            {
                var bufferView = await bufferImporter.GetBufferViewAsync(image.BufferView);
                var imageData = new byte[bufferView.Data.Count];
                Array.Copy(bufferView.Data.Array, bufferView.Data.Offset, imageData, 0, bufferView.Data.Count);
                return imageData;
            }
            else
            {
                throw new Exception($"Data source for image {image.Name} not found.");
            }
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