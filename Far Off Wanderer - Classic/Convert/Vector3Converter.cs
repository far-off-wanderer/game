using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Far_Off_Wanderer.Convert
{
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.X} {value.Y} {value.Z}");
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var s = ((string)reader.Value).Split(' ').Select(v => float.Parse(v)).Take(3).ToArray();

            return new Vector3(s[0], s[1], s[2]);
        }
    }
}
