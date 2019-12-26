using System;
using System.IO;
using UnityEngine;

namespace Gru.Helpers
{
    public struct GlbDetails
    {
        public int JsonChunkStart;
        public int JsonChunkLength;
        public bool HasEmbeddedBuffer;
        public int BufferChunkStart;
        public int BufferChunkLength;
    }

    /// <summary>
    /// <see href="https://github.com/KhronosGroup/glTF/raw/master/specification/2.0/figures/glb2.png"/>
    /// </summary>
    public static class GlbParser
    {
        private const uint MAGIC = 0x46546C67;
        private const uint JSON = 0x4E4F534A;
        private const uint BIN = 0x004E4942;

        private const int HeaderLength = 12;
        private const int ChunkHeaderLength = 8;

        public static GlbDetails Parse(Stream glbData)
        {
            var header = new byte[HeaderLength];
            glbData.Position = 0;

            if (glbData.Read(header, 0, HeaderLength) != HeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var magic = BitConverter.ToUInt32(header, 0);
            var version = BitConverter.ToUInt32(header, 4);
            var length = BitConverter.ToUInt32(header, 8);

            if (magic != MAGIC)
            {
                throw new Exception("Could not read chunk header.");
            }
            if (length != glbData.Length)
            {
                Debug.LogError("Mismatch in glb stored length and actual length");
            }

            Debug.Log("Glb container version: " + version);

            var chunkHeader = new byte[ChunkHeaderLength];
            glbData.Position = HeaderLength;

            if (glbData.Read(chunkHeader, 0, ChunkHeaderLength) != ChunkHeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var jsonChunkLength = BitConverter.ToUInt32(chunkHeader, 0);
            var jsonChunkType = BitConverter.ToUInt32(chunkHeader, 4);

            if (length < HeaderLength + ChunkHeaderLength + jsonChunkLength)
            {
                throw new Exception("Chunk length overshoots stream length.");
            }
            if (jsonChunkType != JSON)
            {
                throw new Exception("Chunktype of first chunk is not JSON");
            }

            // chunks are padded to 4 byte boundary
            var bufferChunkStart = HeaderLength + ChunkHeaderLength + jsonChunkLength + (jsonChunkLength % 4);
            if (bufferChunkStart >= length - 8)
            {
                // Stream is not long enough to have a binary chunk
                return new GlbDetails
                {
                    JsonChunkStart = HeaderLength + ChunkHeaderLength,
                    JsonChunkLength = (int)jsonChunkLength,
                    HasEmbeddedBuffer = false
                };
            }

            glbData.Position = bufferChunkStart;

            if (glbData.Read(chunkHeader, 0, ChunkHeaderLength) != ChunkHeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var bufferChunkLength = BitConverter.ToUInt32(chunkHeader, 0);
            var bufferChunkType = BitConverter.ToUInt32(chunkHeader, 4);

            if (length < bufferChunkStart + ChunkHeaderLength + bufferChunkLength)
            {
                throw new Exception("Chunk length overshoots stream length");
            }
            if (bufferChunkType != BIN)
            {
                throw new Exception("Chunktype of second chunk is not BIN");
            }

            return new GlbDetails
            {
                JsonChunkStart = HeaderLength + ChunkHeaderLength,
                JsonChunkLength = (int)jsonChunkLength,
                HasEmbeddedBuffer = true,
                BufferChunkStart = (int)bufferChunkStart + ChunkHeaderLength,
                BufferChunkLength = (int)bufferChunkLength
            };
        }
    }
}