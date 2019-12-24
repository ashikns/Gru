using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// Metadata about the glTF asset.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/asset.schema.json"/>
    /// </summary>
    public class Asset : GLTFProperty
    {
        [Preserve]
        public Asset() { }

        /// <summary>
        /// A copyright message suitable for display to credit the content creator.
        /// </summary>
        [JsonProperty(PropertyName = "copyright")]
        public string Copyright { get; set; }

        /// <summary>
        /// Tool that generated this glTF model. Useful for debugging.
        /// </summary>
        [JsonProperty(PropertyName = "generator")]
        public string Generator { get; set; }

        /// <summary>
        /// The glTF version that this asset targets.
        /// </summary>
        [JsonProperty(PropertyName = "version", Required = Required.Always)]
        public string Version
        {
            get => version;
            set
            {
                if (!Regex.IsMatch(value, Pattern))
                {
                    throw new Exception($"{nameof(Version)} must be of format {Pattern}");
                }
                version = value;
            }
        }

        /// <summary>
        /// The minimum glTF version that this asset targets.
        /// </summary>
        [JsonProperty(PropertyName = "minVersion")]
        public string MinVersion
        {
            get => minVersion;
            set
            {
                if (!Regex.IsMatch(value, Pattern))
                {
                    throw new Exception($"{nameof(MinVersion)} must be of format {Pattern}");
                }
                minVersion = value;
            }
        }


        private const string Pattern = "^[0-9]+\\.[0-9]+$";

        private string version;
        private string minVersion;

        public override bool Validate()
        {
            return !string.IsNullOrEmpty(Version);
        }
    }
}