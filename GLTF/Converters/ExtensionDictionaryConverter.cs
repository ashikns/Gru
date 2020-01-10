using Gru.GLTF.Extensions;
using Gru.GLTF.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Gru.GLTF.Converters
{
    public class ExtensionDictionaryConverter : JsonConverter<Dictionary<GLTFExtension, object>>
    {
        [Preserve]
        public ExtensionDictionaryConverter() { }

        public override Dictionary<GLTFExtension, object> ReadJson(JsonReader reader, Type objectType, Dictionary<GLTFExtension, object> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Unexpected token!");
            }

            var extensions = new Dictionary<GLTFExtension, object>();
            reader.Read();

            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    throw new JsonSerializationException("Unexpected token!");
                }

                var key = (string)reader.Value;
                reader.Read();

                if (Enum.TryParse<GLTFExtension>(key, out var enumKey))
                {
                    if (reader.TokenType != JsonToken.StartObject)
                    {
                        throw new JsonSerializationException("Unexpected token!");
                    }

                    switch (enumKey)
                    {
                        case GLTFExtension.KHR_materials_pbrSpecularGlossiness:
                            var extensionObj = serializer.Deserialize<KHR_materials_pbrSpecularGlossiness>(reader);
                            extensions.Add(enumKey, extensionObj);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(enumKey));
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"Extension {key} is not recognized.");

                    // Advance reader position past extension object
                    _ = serializer.Deserialize(reader);
                }

                reader.Read();
            }
            return extensions;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<GLTFExtension, object> value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            foreach (var extension in value)
            {
                writer.WritePropertyName(extension.Key.ToString());

                switch (extension.Key)
                {
                    case GLTFExtension.KHR_materials_pbrSpecularGlossiness:
                        serializer.Serialize(writer, extension.Value, typeof(KHR_materials_pbrSpecularGlossiness));
                        break;
                    default:
                        throw new Exception($"Extension {extension.Key} is not supported.");
                }
            }

            writer.WriteEndObject();
        }
    }
}