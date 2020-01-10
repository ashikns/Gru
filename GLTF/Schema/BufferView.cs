using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A view into a buffer generally representing a subset of the buffer.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/bufferView.schema.json"/>
    /// </summary>
    public class BufferView : GLTFChildOfRootProperty
    {
        [Preserve]
        public BufferView()
        {
            ByteOffset = 0;
        }

        /// <summary>
        /// The index of the buffer.
        /// </summary>
        [JsonProperty(PropertyName = "buffer", Required = Required.Always)]
        public GLTFId Buffer { get; set; }

        /// <summary>
        /// The offset into the buffer in bytes.
        /// </summary>
        [JsonProperty(PropertyName = "byteOffset")]
        public int ByteOffset
        {
            get => byteOffset;
            set
            {
                if (value < 0) { throw new Exception($"{nameof(ByteOffset)} cannot be less than zero"); }
                byteOffset = value;
            }
        }

        /// <summary>
        /// The total byte length of the buffer view.
        /// </summary>
        [JsonProperty(PropertyName = "byteLength", Required = Required.Always)]
        public int ByteLength
        {
            get => byteLength;
            set
            {
                if (value < 1) { throw new Exception($"{nameof(ByteLength)} cannot be less than one"); }
                byteLength = value;
            }
        }

        /// <summary>
        /// The stride, in bytes, between vertex attributes.
        /// When this is not defined, data is tightly packed.
        /// When two or more accessors use the same bufferView, this field must be defined.
        /// </summary>
        [JsonProperty(PropertyName = "byteStride")]
        public int? ByteStride
        {
            get => byteStride;
            set
            {
                if (value < 4 || value > 252 || value % 4 != 0)
                {
                    throw new Exception($"{nameof(ByteStride)} must be in the range [4, 252] and divisible by 4");
                }
                byteStride = value;
            }
        }

        /// <summary>
        /// The target that the GPU buffer should be bound to.
        /// </summary>
        [JsonProperty(PropertyName = "target")]
        public BufferTarget? Target { get; set; }


        private int byteOffset;
        private int byteLength;
        private int? byteStride;

        public override bool Validate()
        {
            return Buffer != null && ByteLength >= 1;
        }
    }

    public enum BufferTarget : int
    {
        ARRAY_BUFFER = 34962,
        ELEMENT_ARRAY_BUFFER = 34963
    }
}