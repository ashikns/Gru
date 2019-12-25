using Gru.FileLoaders;
using Gru.Helpers;
using Gru.ImporterResults;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gru.Importers
{
    public class BufferImporter
    {
        private readonly IList<GLTF.Schema.Buffer> _bufferSchemas;
        private readonly IList<GLTF.Schema.BufferView> _bufferViewSchemas;
        private readonly IFileLoader _fileLoader;
        private readonly string _modelFileName;

        private readonly ConcurrentDictionary<int, Lazy<Task<byte[]>>> _buffers;
        private readonly ConcurrentDictionary<int, Lazy<Task<BufferView>>> _bufferViews;

        public BufferImporter(
            IList<GLTF.Schema.Buffer> buffers,
            IList<GLTF.Schema.BufferView> bufferViews,
            IFileLoader fileLoader,
            string modelFileName)
        {
            _buffers = new ConcurrentDictionary<int, Lazy<Task<byte[]>>>();
            _bufferViews = new ConcurrentDictionary<int, Lazy<Task<BufferView>>>();

            _bufferSchemas = buffers;
            _bufferViewSchemas = bufferViews;
            _fileLoader = fileLoader;
            _modelFileName = modelFileName;
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
                bufferViewSchema.Buffer.Key, new Lazy<Task<byte[]>>(
                    () => ReadBuffer(bufferSchema, _fileLoader, _modelFileName)));
            var buffer = await lazyResult.Value;

            var bufferView = new BufferView
            {
                Data = new ArraySegment<byte>(buffer, bufferViewSchema.ByteOffset, bufferViewSchema.ByteLength),
                Stride = (uint)(bufferViewSchema.ByteStride ?? 0)
            };

            return bufferView;
        }

        private static async Task<byte[]> ReadBuffer(GLTF.Schema.Buffer buffer, IFileLoader fileLoader, string modelFile)
        {
            if (string.IsNullOrEmpty(buffer.Uri))
            {
                if (string.IsNullOrEmpty(modelFile))
                {
                    throw new Exception("No model file provided");
                }
                if (fileLoader == null)
                {
                    throw new Exception($"{nameof(fileLoader)} is null. Can't read buffer data.");
                }

                using (var stream = await fileLoader.OpenFile(modelFile))
                {
                    var bufferData = new byte[buffer.ByteLength];
                    var bufferBounds = GlbParser.GetBufferChunkBounds(stream);

                    if (buffer.ByteLength > bufferBounds.Count)
                    {
                        throw new Exception("Specified buffer size is bigger than glb chunk.");
                    }

                    stream.Position = bufferBounds.Offset;

                    if (await stream.ReadAsync(bufferData, 0, buffer.ByteLength) != buffer.ByteLength)
                    {
                        throw new Exception($"Failed to read buffer data");
                    }
                    return bufferData;
                }
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

                using (var stream = await fileLoader.OpenFile(buffer.Uri))
                {
                    var bufferData = new byte[buffer.ByteLength];
                    if (await stream.ReadAsync(bufferData, 0, buffer.ByteLength) != buffer.ByteLength)
                    {
                        throw new Exception($"Failed to read buffer data");
                    }
                    return bufferData;
                }
            }
        }
    }
}