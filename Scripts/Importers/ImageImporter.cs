using Gru.FileLoaders;
using Gru.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gru.Importers
{
    public class ImageImporter
    {
        private IList<GLTF.Schema.Image> _imageSchemas;
        private BufferImporter _bufferImporter;
        private IFileLoader _fileLoader;

        private readonly ConcurrentDictionary<int, Lazy<Task<byte[]>>> _images;

        public ImageImporter()
        {
            _images = new ConcurrentDictionary<int, Lazy<Task<byte[]>>>();
        }

        public void Assign(
            IList<GLTF.Schema.Image> images,
            BufferImporter bufferImporter,
            IFileLoader fileLoader)
        {
            _images.Clear();

            _imageSchemas = images;
            _bufferImporter = bufferImporter;
            _fileLoader = fileLoader;
        }

        public Task<byte[]> GetImageAsync(GLTF.Schema.GLTFId imageId)
        {
            var lazyResult = _images.GetOrAdd(
                imageId.Key, new Lazy<Task<byte[]>>(
                    () => ReadImageData(_imageSchemas[imageId.Key], _bufferImporter, _fileLoader)));
            return lazyResult.Value;
        }

        private static async Task<byte[]> ReadImageData(GLTF.Schema.Image image, BufferImporter bufferImporter, IFileLoader fileLoader)
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
    }
}