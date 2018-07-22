using Newtonsoft.Json;
using System;

namespace Far_Off_Wanderer.Convert
{
    public static class Json
    {
        public static T Convert<T>(string data) => JsonConvert.DeserializeObject<T>(data, converters);
        public static object Convert(string data, Type type) => JsonConvert.DeserializeObject(data, type, converters);

        public static string Convert<T>(T data) =>  JsonConvert.SerializeObject(data, Formatting.Indented, converters);

        static readonly JsonConverter[] converters = new JsonConverter[]
        {
            new Vector3Converter(),
            new ColorConverter()
        };
    }
}
