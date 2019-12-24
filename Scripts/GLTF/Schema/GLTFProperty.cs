using Gru.GLTF.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/glTFProperty.schema.json"/>
    /// </summary>
    public class GLTFProperty
    {
        [Preserve]
        public GLTFProperty() { }

        /// <summary>
        /// Dictionary object with extension-specific objects.
        /// </summary>
        [JsonProperty(PropertyName = "extensions")]
        [JsonConverter(typeof(ExtensionDictionaryConverter))]
        public Dictionary<GLTFExtension, object> Extensions { get; set; }

        /// <summary>
        /// Application-specific data.
        /// </summary>
        [JsonProperty(PropertyName = "extras")]
        public object Extras { get; set; }

        public virtual bool Validate() => true;
    }

    public enum GLTFExtension
    {
        KHR_materials_pbrSpecularGlossiness
    }
}