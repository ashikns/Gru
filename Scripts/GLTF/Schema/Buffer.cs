using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A buffer points to binary geometry, animation, or skins.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/buffer.schema.json"/>
    /// </summary>
    public class Buffer : GLTFChildOfRootProperty
    {
        [Preserve]
        public Buffer() { }

        /// <summary>
        /// The uri of the buffer. Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// </summary>
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        /// <summary>
        /// The length of the buffer in bytes.
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


        private int byteLength;

        public override bool Validate()
        {
            return byteLength >= 1;
        }
    }
}