using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Notes;
using AgGateway.ADAPT.ApplicationDataModel.Representations;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class CommentWriter : BaseWriter
    {
        private CommentListWriter _listWriter;

        private CommentWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "CCT")
        {
            _listWriter = new CommentListWriter();
        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Documents ==null ||
                taskWriter.DataModel.Documents.WorkOrders ==null)
                return;

            var writer = new CommentWriter(taskWriter);
            writer.WriteComments(taskWriter.RootWriter);
        }

        private void WriteComments(XmlWriter writer)
        {
            foreach (var workOrder in TaskWriter.DataModel.Documents.WorkOrders)
            {
                if(workOrder.Notes == null)
                    continue;
                foreach (var note in workOrder.Notes)
                {
                    WriteComments(writer, note);
                }
            }
        }

        private void WriteComments(XmlWriter writer, Note note)
        {
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", GenerateId());
            writer.WriteAttributeString("B", note.Description);
            writer.WriteAttributeString("C", "2");

            WriteListValues(writer, note.Value);

            writer.WriteEndElement();
        }

        private void WriteListValues(XmlWriter writer, EnumeratedValue value)
        {
            if (value == null)
                return;

            if (value.Representation != null &&
                value.Representation.EnumeratedMembers != null)
            {
                _listWriter.Write(writer, value.Representation.EnumeratedMembers);
            }
            else if (value.Value != null)
            {
                _listWriter.Write(writer, new List<EnumerationMember> { value.Value });
            }
        }
    }
}