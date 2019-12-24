using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// Texture sampler properties for filtering and wrapping modes.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/sampler.schema.json"/>
    /// </summary>
    public class Sampler : GLTFChildOfRootProperty
    {
        [Preserve]
        public Sampler()
        {
            WrapS = WrapMode.REPEAT;
            WrapT = WrapMode.REPEAT;
        }

        /// <summary>
        /// Magnification filter. Valid values correspond to WebGL enums: `9728` (NEAREST) and `9729` (LINEAR).
        /// </summary>
        [JsonProperty(PropertyName = "magFilter")]
        public FilterMode? MagFilter { get; set; }

        /// <summary>
        /// Minification filter.
        /// </summary>
        [JsonProperty(PropertyName = "minFilter")]
        public FilterMode? MinFilter { get; set; }

        /// <summary>
        /// S (U) wrapping mode.
        /// </summary>
        [JsonProperty(PropertyName = "wrapS")]
        public WrapMode WrapS { get; set; }

        /// <summary>
        /// T (V) wrapping mode.
        /// </summary>
        [JsonProperty(PropertyName = "wrapT")]
        public WrapMode WrapT { get; set; }
    }

    public enum FilterMode : int
    {
        NEAREST = 9728,
        LINEAR = 9729,
        NEAREST_MIPMAP_NEAREST = 9984,
        LINEAR_MIPMAP_NEAREST = 9985,
        NEAREST_MIPMAP_LINEAR = 9986,
        LINEAR_MIPMAP_LINEAR = 9987
    }

    public enum WrapMode : int
    {
        CLAMP_TO_EDGE = 33071,
        MIRRORED_REPEAT = 33648,
        REPEAT = 10497
    }
}