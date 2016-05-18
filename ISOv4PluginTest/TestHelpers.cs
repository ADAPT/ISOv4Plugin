using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
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

        public static T LoadFromJson<T>(byte[] workersWithAllData)
        {
            using (var stream = new MemoryStream(workersWithAllData))
            using (var reader = new StreamReader(stream))
            {
                return (T)_jsonSerializer.Deserialize(reader, typeof (T));
            }
        }

        public static string Export(TaskDocumentWriter taskWriter, ApplicationDataModel adaptDocument, string datacardPath)
        {
            taskWriter.Write(datacardPath, adaptDocument);
            new Exporter().Export(adaptDocument, datacardPath, taskWriter.RootWriter, taskWriter);
            taskWriter.RootWriter.Flush();
            var actual = Encoding.UTF8.GetString(taskWriter.XmlStream.ToArray());
            return actual;
        }
    }
}
