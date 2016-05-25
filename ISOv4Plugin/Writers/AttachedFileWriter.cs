using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class AttachedFileWriter : BaseWriter
    {
        protected AttachedFileWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "AFE")
        {

        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Crops == null ||
                taskWriter.DataModel.Catalog.Crops.Count == 0)
                return;

            var writer = new AttachedFileWriter(taskWriter);
            writer.WriteAttachedFile(taskWriter.RootWriter);
        }


        private void WriteAttachedFile(XmlWriter writer)
        {
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", "LINKLIST.XML");
            writer.WriteAttributeString("B", "1");
            writer.WriteAttributeString("C", "");
            writer.WriteAttributeString("D", "1");
            writer.WriteEndElement();
        }
    }
}
