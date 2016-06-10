using System;
using System.IO;
using System.IO.Compression;

namespace TestUtilities
{
    public class DataCardUtility
    {
        public static string WriteDataCard(string resource)
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            WriteDataCard(resource, directory);
            return directory;
        }

        public static void WriteDataCard(string resource, string directory)
        {
            var bytes = GetResource(resource);
            Directory.CreateDirectory(directory);

            var zipFilePath = Path.Combine(directory, "DataCard.zip");
            File.WriteAllBytes(zipFilePath, bytes);

            ZipFile.ExtractToDirectory(zipFilePath, directory);
        }

        private static string GetResourceName(string resourceName)
        {
            return resourceName.Replace(" ", "_");
        }

        private static byte[] GetResource(string resource)
        {
            var resourceName = GetResourceName(resource);
            var bytes = (byte[])Datacards.ResourceManager.GetObject(resourceName);
            if (bytes == null)
                throw new ArgumentException(string.Format("Data card {0} is not part of data cards resources.", resource));

            return bytes;
        }
    }
}
