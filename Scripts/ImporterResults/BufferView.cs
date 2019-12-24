using System;

namespace Gru.ImporterResults
{
    public class BufferView
    {
        public ArraySegment<byte> Data { get; set; }
        public uint Stride { get; set; } = 0;
    }
}