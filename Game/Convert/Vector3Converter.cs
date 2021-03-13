using Microsoft.Xna.Framework;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Far_Off_Wanderer.Convert
{
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Vector3 result;
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Parsing Error in Vector3Converter: Not an Array");
            }
            reader.Read();
            result.X = reader.GetSingle();
            reader.Read();
            result.Y = reader.GetSingle();
            reader.Read();
            result.Z = reader.GetSingle();
            reader.Read();

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteEndArray();
        }
    }
}
