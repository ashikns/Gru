using Gru.Extensions;
using Gru.Helpers;
using Gru.ImporterResults;
using Gru.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gru.Importers
{
    public class BufferImporter
    {
        private readonly IList<GLTF.Schema.Buffer> _bufferSchemas;
        private readonly IList<GLTF.Schema.BufferView> _bufferViewSchemas;
        private readonly IBufferLoader _bufferLoader;

        private readonly Lazy<Task<byte[]>>[] _buffers;
        private readonly Lazy<Task<BufferView>>[] _bufferViews;

        public BufferImporter(
            IList<GLTF.Schema.Buffer> buffers,
            IList<GLTF.Schema.BufferView> bufferViews,
            IBufferLoader bufferLoader)
        {
            _bufferSchemas = buffers;
            _bufferViewSchemas = bufferViews;
            _bufferLoader = bufferLoader;

            _buffers = new Lazy<Task<byte[]>>[_bufferSchemas?.Count ?? 0];
            _bufferViews = new Lazy<Task<BufferView>>[_bufferViewSchemas?.Count ?? 0];
        }

        public void SetGlbEmbeddedBuffer(byte[] embeddedBufferData)
        {
            // If there is any buffer without a uri that buffer is contained in the model file
            var noUriBuffer = _bufferSchemas.FirstOrDefault(b => string.IsNullOrEmpty(b.Uri));
            if (noUriBuffer == null)
            {
                return;
            }

            _buffers[_bufferSchemas.IndexOf(noUriBuffer)] = new Lazy<Task<byte[]>>(() => Task.FromResult(embeddedBufferData));
        }

        public Task<BufferView> GetBufferViewAsync(GLTF.Schema.GLTFId bufferViewId)
        {
            return _bufferViews.ThreadSafeGetOrAdd(bufferViewId.Key, () => ConstructBufferViewAsync(bufferViewId));
        }

        private async Task<BufferView> ConstructBufferViewAsync(GLTF.Schema.GLTFId bufferViewId)
        {
            var bufferViewSchema = _bufferViewSchemas[bufferViewId.Key];

            var bufferSchema = _bufferSchemas[bufferViewSchema.Buffer.Key];
            var buffer = await _buffers.ThreadSafeGetOrAdd(
                bufferViewSchema.Buffer.Key, () => ReadBuffer(bufferSchema, _bufferLoader));

            var bufferView = new BufferView
            {
                Data = new ArraySegment<byte>(buffer, bufferViewSchema.ByteOffset, bufferViewSchema.ByteLength),
                Stride = (uint)(bufferViewSchema.ByteStride ?? 0)
            };

            return bufferView;
        }

        private static async Task<byte[]> ReadBuffer(GLTF.Schema.Buffer buffer, IBufferLoader bufferLoader)
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
                if (bufferLoader == null)
                {
                    throw new Exception($"{nameof(bufferLoader)} is null. Can't read buffer data.");
                }

                return await bufferLoader.ReadContentsAsync(buffer.Uri);
            }
        }
    }
}