using AgGateway.ADAPT.ApplicationDataModel.Notes;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class CommentWriter : BaseWriter
    {
        private CommentListWriter _listWriter;

        private CommentWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "CCT")
        {
            _listWriter = new CommentListWriter();
        }

        internal static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Notes == null ||
                taskWriter.DataModel.Catalog.Notes.Count == 0)
                return;

            var writer = new CommentWriter(taskWriter);
            writer.Write();
        }

        private void Write()
        {
            WriteToExternalFile(WriteComments);
        }

        private void WriteComments(XmlWriter writer)
        {
            foreach (var note in TaskWriter.DataModel.Catalog.Notes)
            {
                WriteComments(writer, note);
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