using Gru.GLTF.Schema;
using Newtonsoft.Json;
using System;

namespace Gru.GLTF.Converters
{
    public class GLTFIdConverter : JsonConverter<GLTFId>
    {
        public override GLTFId ReadJson(JsonReader reader, Type objectType, GLTFId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                if ((reader.Value as long?).HasValue)
                {
                    var val = reader.Value as long?;
                    return new GLTFId((int)val.Value);
                }
                else if ((reader.Value as int?).HasValue)
                {
                    var val = reader.Value as int?;
                    return new GLTFId(val.Value);
                }
                else
                {
                    throw new JsonException($"There's a property named {nameof(GLTFId)} but failed to parse its value");
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, GLTFId value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.Key);
            }
        }
    }
}