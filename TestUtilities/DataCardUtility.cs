using System;
using System.IO;
using System.IO.Compression;

namespace AgGateway.ADAPT.TestUtilities
{
    public class DataCardUtility
    {
        public static string WriteDatacard(string name)
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            WriteDatacard(name, directory);
            return directory;
        }

        public static void WriteDatacard(string name, string directory)
        {
            var bytes = GetDatacard(name);
            Directory.CreateDirectory(directory);

            var zipFilePath = Path.Combine(directory, "DataCard.zip");
            File.WriteAllBytes(zipFilePath, bytes);

            ZipFile.ExtractToDirectory(zipFilePath, directory);
        }

        public static byte[] GetDatacard(string name)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataCards", Path.ChangeExtension(name.Replace(" ", "_"), ".zip"));
            return File.ReadAllBytes(path);
        }        
    }
}
