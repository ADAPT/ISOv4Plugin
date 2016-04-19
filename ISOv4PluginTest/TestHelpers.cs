using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace ISOv4PluginTest
{
    internal static class TestHelpers
    {
        private static JsonSerializer _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                NullValueHandling = NullValueHandling.Ignore
            });

        internal static string LoadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        internal static string LoadFromFileAsHexString(string filePath)
        {
            return BitConverter.ToString(File.ReadAllBytes(filePath)).Replace("-", "");
        }

        internal static T LoadFromJson<T>(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _jsonSerializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}
