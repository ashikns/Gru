using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/glTFChildOfRootProperty.schema.json"/>
    /// </summary>
    public class GLTFChildOfRootProperty : GLTFProperty
    {
        [Preserve]
        public GLTFChildOfRootProperty() { }

        /// <summary>
        /// The user-defined name of this object.
        /// This is not necessarily unique, e.g., an accessor and a buffer could have the same name,
        /// or two accessors could even have the same name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}