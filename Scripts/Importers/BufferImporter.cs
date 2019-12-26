using Gru.Helpers;
using Gru.ImporterResults;
using Gru.Loaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gru.Importers
{
    public class BufferImporter
    {
        private readonly IList<GLTF.Schema.Buffer> _bufferSchemas;
        private readonly IList<GLTF.Schema.BufferView> _bufferViewSchemas;
        private readonly IFileLoader _fileLoader;

        private readonly ConcurrentDictionary<int, Lazy<Task<byte[]>>> _buffers;
        private readonly ConcurrentDictionary<int, Lazy<Task<BufferView>>> _bufferViews;

        public BufferImporter(
            IList<GLTF.Schema.Buffer> buffers,
            IList<GLTF.Schema.BufferView> bufferViews,
            IFileLoader fileLoader)
        {
            _buffers = new ConcurrentDictionary<int, Lazy<Task<byte[]>>>();
            _bufferViews = new ConcurrentDictionary<int, Lazy<Task<BufferView>>>();

            _bufferSchemas = buffers;
            _bufferViewSchemas = bufferViews;
            _fileLoader = fileLoader;
        }

        public void SetGlbEmbeddedBuffer(byte[] embeddedBufferData)
        {
            // If there is any buffer without a uri that buffer is contained in the model file
            var noUriBuffer = _bufferSchemas.FirstOrDefault(b => string.IsNullOrEmpty(b.Uri));
            if (noUriBuffer == null)
            {
                return;
            }

            _buffers.TryAdd(_bufferSchemas.IndexOf(noUriBuffer), new Lazy<Task<byte[]>>(() => Task.FromResult(embeddedBufferData)));
        }

        public Task<BufferView> GetBufferViewAsync(GLTF.Schema.GLTFId bufferViewId)
        {
            var lazyResult = _bufferViews.GetOrAdd(
                bufferViewId.Key, new Lazy<Task<BufferView>>(() => ConstructBufferViewAsync(bufferViewId)));
            return lazyResult.Value;
        }

        private async Task<BufferView> ConstructBufferViewAsync(GLTF.Schema.GLTFId bufferViewId)
        {
            var bufferViewSchema = _bufferViewSchemas[bufferViewId.Key];

            var bufferSchema = _bufferSchemas[bufferViewSchema.Buffer.Key];
            var lazyResult = _buffers.GetOrAdd(
                bufferViewSchema.Buffer.Key, new Lazy<Task<byte[]>>(() => ReadBuffer(bufferSchema, _fileLoader)));
            var buffer = await lazyResult.Value;

            var bufferView = new BufferView
            {
                Data = new ArraySegment<byte>(buffer, bufferViewSchema.ByteOffset, bufferViewSchema.ByteLength),
                Stride = (uint)(bufferViewSchema.ByteStride ?? 0)
            };

            return bufferView;
        }

        private static async Task<byte[]> ReadBuffer(GLTF.Schema.Buffer buffer, IFileLoader fileLoader)
        {
            if (string.IsNullOrEmpty(buffer.Uri))
            {
                throw new Exception("No uri buffer should have been created before hand");
            }
            else if (UriHelper.TryParseDataUri(buffer.Uri, out var embeddedData))
            {
                return embeddedData;
            }
            else
            {
                if (fileLoader == null)
                {
                    throw new Exception($"{nameof(fileLoader)} is null. Can't read buffer data.");
                }

                return await fileLoader.ReadContentsAsync(buffer.Uri);
            }
        }
    }
}