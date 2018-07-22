using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Far_Off_Wanderer.Convert
{
    public static class Json
    {
        public static T Convert<T>(string data) => JsonConvert.DeserializeObject<T>(data, converters);
        public static object Convert(string data, Type type) => JsonConvert.DeserializeObject(data, type, converters);

        public static string Convert<T>(T data) => JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            Converters = converters,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        });

        static readonly JsonConverter[] converters = new JsonConverter[]
        {
            new Vector3Converter(),
            new ColorConverter()
        };
    }
}
