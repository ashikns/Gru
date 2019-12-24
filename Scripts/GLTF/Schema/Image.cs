using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// Image data used to create a texture.
    /// Image can be referenced by URI or `bufferView` index. `mimeType` is required in the latter case.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/image.schema.json"/>
    /// </summary>
    public class Image : GLTFChildOfRootProperty
    {
        [Preserve]
        public Image() { }

        /// <summary>
        /// The uri of the image. Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// The image format must be jpg or png.
        /// </summary>
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        /// <summary>
        /// The image's MIME type. Required if `bufferView` is defined.
        /// </summary>
        [JsonProperty(PropertyName = "mimeType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MimeType? MimeType { get; set; }

        /// <summary>
        /// The index of the bufferView that contains the image. Use this instead of the image's uri property.
        /// </summary>
        [JsonProperty(PropertyName = "bufferView")]
        public GLTFId BufferView { get; set; }


        public override bool Validate()
        {
            // BufferView -> MimeType
            return (BufferView == null || MimeType != null)
                // Required oneOf [Uri, BufferView]
                && (!string.IsNullOrEmpty(Uri) || BufferView != null);
        }
    }

    public enum MimeType
    {
        [EnumMember(Value = "image/jpeg")]
        jpeg,
        [EnumMember(Value = "image/png")]
        png
    }
}