using System.Text.Json;
using System;

namespace Far_Off_Wanderer.Convert
{
    public static class Json
    {
        public static T Convert<T>(string data) => JsonSerializer.Deserialize<T>(data, GetOptions());
        public static object Convert(string data, Type type) => JsonSerializer.Deserialize(data, type, GetOptions());

        public static string Convert<T>(T data) => JsonSerializer.Serialize<T>(data, GetOptions());

        static JsonSerializerOptions GetOptions()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new Vector3Converter());
            options.Converters.Add(new ColorConverter());
            return options;
        }
    }
}
