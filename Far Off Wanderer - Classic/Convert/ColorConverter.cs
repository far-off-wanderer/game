using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Far_Off_Wanderer.Convert
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            if (value.A != 255)
            {
                writer.WriteValue($"#{value.R:X2}{value.G:X2}{value.B:X2}{value.A:X2}");
            }
            else
            {
                writer.WriteValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
            }
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var color = (string)reader.Value;

            if (color.StartsWith("#"))
            {
                //remove the # at the front
                color = color.Replace("#", "");

                byte a = 255;
                byte r = 255;
                byte g = 255;
                byte b = 255;

                int start = 0;

                //handle RGBA strings (8 characters long)
                if (color.Length == 8)
                {
                    a = byte.Parse(color.Substring(0, 2), NumberStyles.HexNumber);
                    start = 2;
                }

                //convert RGB characters to bytes
                r = byte.Parse(color.Substring(start, 2), NumberStyles.HexNumber);
                g = byte.Parse(color.Substring(start + 2, 2), NumberStyles.HexNumber);
                b = byte.Parse(color.Substring(start + 4, 2), NumberStyles.HexNumber);

                return new Color(r, g, b, a);
            }
            else
            {
                return (Color)(typeof(Color).GetProperties().FirstOrDefault(p => string.Equals(p.Name, color, StringComparison.OrdinalIgnoreCase)).GetValue(null));
            }
        }
    }
}
