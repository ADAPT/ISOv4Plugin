using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Representations;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class CommentListWriter : BaseWriter
    {
        public CommentListWriter()
            : base(null, "CCL")
        {
        }

        public void Write(XmlWriter writer, List<EnumerationMember> listValues)
        {
            if (listValues == null || listValues.Count == 0)
                return;

            foreach (var listValue in listValues)
            {
                WriteListValue(writer, listValue);
            }
        }

        private void WriteListValue(XmlWriter writer, EnumerationMember listValue)
        {
            var elementId = GenerateId();

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", elementId);
            writer.WriteAttributeString("B", listValue.Value);
            writer.WriteEndElement();
        }
    }
}