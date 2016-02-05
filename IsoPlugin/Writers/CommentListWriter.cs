using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class CommentListWriter : BaseWriter
    {
        internal CommentListWriter()
            : base(null, "CCL")
        {
        }

        internal void Write(XmlWriter writer, List<EnumerationMember> listValues)
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