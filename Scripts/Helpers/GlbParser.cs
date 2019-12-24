using System;
using System.IO;

namespace Gru.Helpers
{
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

        public static bool IsValidGlb(Stream glbData)
        {
            var header = new byte[HeaderLength];
            glbData.Position = 0;

            if (glbData.Read(header, 0, HeaderLength) != HeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var magic = BitConverter.ToUInt32(header, 0);
            _ = BitConverter.ToUInt32(header, 4);
            var length = BitConverter.ToUInt32(header, 8);

            if (magic != MAGIC)
            {
                return false;
            }
            if (length != glbData.Length)
            {
                return false;
            }
            return true;
        }

        public static (uint Offset, uint Count) GetJsonChunkBounds(Stream glbData)
        {
            var chunkHeader = new byte[ChunkHeaderLength];
            glbData.Position = HeaderLength;

            if (glbData.Read(chunkHeader, 0, ChunkHeaderLength) != ChunkHeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var chunkLength = BitConverter.ToUInt32(chunkHeader, 0);
            var chunkType = BitConverter.ToUInt32(chunkHeader, 4);

            if (glbData.Length < HeaderLength + ChunkHeaderLength + chunkLength)
            {
                throw new Exception("Chunk length overshoots stream length.");
            }
            if (chunkType != JSON)
            {
                throw new Exception("Chunktype of first chunk is not JSON");
            }

            return (HeaderLength + ChunkHeaderLength, chunkLength);
        }

        public static (uint Offset, uint Count) GetBufferChunkBounds(Stream glbData)
        {
            var chunkHeader = new byte[ChunkHeaderLength];
            glbData.Position = HeaderLength;

            if (glbData.Read(chunkHeader, 0, ChunkHeaderLength) != ChunkHeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var jsonChunkLength = BitConverter.ToUInt32(chunkHeader, 0);
            // chunks are padded to 4 byte boundary
            var bufferChunkStart = HeaderLength + ChunkHeaderLength + jsonChunkLength + (jsonChunkLength % 4);
            if (bufferChunkStart >= glbData.Length - 8)
            {
                throw new Exception("Stream is not long enough to have a binary chunk.");
            }

            glbData.Position = bufferChunkStart;

            if (glbData.Read(chunkHeader, 0, ChunkHeaderLength) != ChunkHeaderLength)
            {
                throw new Exception("Could not read chunk header.");
            }

            var chunkLength = BitConverter.ToUInt32(chunkHeader, 0);
            var chunkType = BitConverter.ToUInt32(chunkHeader, 4);

            if (glbData.Length < bufferChunkStart + ChunkHeaderLength + chunkLength)
            {
                throw new Exception("Chunk length overshoots stream length");
            }
            if (chunkType != BIN)
            {
                throw new Exception("Chunktype of second chunk is not BIN");
            }

            return (bufferChunkStart + ChunkHeaderLength, chunkLength);
        }
    }
}