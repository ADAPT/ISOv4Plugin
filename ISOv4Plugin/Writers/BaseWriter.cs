using System.Globalization;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class BaseWriter
    {
        private int _itemId;
        protected string XmlPrefix { get; private set; }
        protected TaskDocumentWriter TaskWriter { get; private set; }

        protected BaseWriter(TaskDocumentWriter taskWriter, string xmlPrefix, int startId = 1)
        {
            TaskWriter = taskWriter;
            XmlPrefix = xmlPrefix;
            _itemId = startId;
        }

        protected string GenerateId(byte idLength = 0)
        {
            var formatString = string.Format(CultureInfo.InvariantCulture, "{{0}}{{1:D{0}}}", idLength == 0 ? 0 : idLength);
            return string.Format(CultureInfo.InvariantCulture, formatString, XmlPrefix, _itemId++);
        }

        protected void WriteExternalFileReference(string externalFileName)
        {
            TaskWriter.RootWriter.WriteStartElement("XFR");
            TaskWriter.RootWriter.WriteAttributeString("A", externalFileName);
            TaskWriter.RootWriter.WriteAttributeString("B", "1");
            TaskWriter.RootWriter.WriteEndElement();
        }
    }
}