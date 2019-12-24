using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A texture and its sampler.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/texture.schema.json"/>
    /// </summary>
    public class Texture : GLTFChildOfRootProperty
    {
        [Preserve]
        public Texture() { }

        /// <summary>
        /// The index of the sampler used by this texture.
        /// When undefined, a sampler with repeat wrapping and auto filtering should be used.
        /// </summary>
        [JsonProperty(PropertyName = "sampler")]
        public GLTFId Sampler { get; set; }

        /// <summary>
        /// The index of the image used by this texture.
        /// When undefined, it is expected that an extension or other mechanism will supply an alternate texture source,
        /// otherwise behavior is undefined.
        /// </summary>
        [JsonProperty(PropertyName = "source")]
        public GLTFId Source { get; set; }
    }
}