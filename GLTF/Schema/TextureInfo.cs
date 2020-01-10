using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// Reference to a texture.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/textureInfo.schema.json"/>
    /// </summary>
    public class TextureInfo : GLTFProperty
    {
        private int texCoord;

        [Preserve]
        public TextureInfo()
        {
            TexCoord = 0;
        }

        /// <summary>
        /// The index of the texture.
        /// </summary>
        [JsonProperty(PropertyName = "index", Required = Required.Always)]
        public GLTFId Index { get; set; }

        /// <summary>
        /// This integer value is used to construct a string in the format `TEXCOORD_<set index>`
        /// which is a reference to a key in mesh.primitives.attributes (e.g. A value of `0` corresponds to `TEXCOORD_0`).
        /// Mesh must have corresponding texture coordinate attributes for the material to be applicable to it.
        /// </summary>
        [JsonProperty(PropertyName = "texCoord")]
        public int TexCoord
        {
            get => texCoord;
            set
            {
                if (value < 0) { throw new Exception($"{nameof(TexCoord)} cannot be less than zero"); }
                texCoord = value;
            }
        }
    }
}