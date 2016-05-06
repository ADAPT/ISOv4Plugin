using System.IO;
using System.Text;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public interface IXpathFileWriter
    {
        void WriteToFile(ISO11783_TaskData iso11783TaskData, string filePath);
    }

    public class XpathFileWriter : IXpathFileWriter
    {
        private readonly StringBuilder _finalXml = new StringBuilder();
        public void WriteToFile(ISO11783_TaskData iso11783TaskData, string filePath)
        {
            WriteHeader();
            _finalXml.Append(iso11783TaskData.WriteXML());
            File.WriteAllText(filePath, _finalXml.ToString());
        }

        private void WriteHeader()
        {
            _finalXml.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
        }
    }
}
