using System;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class WorkerWriter : BaseWriter
    {
        private WorkerWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "WKR")
        {
        }

        internal static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Persons == null ||
                taskWriter.DataModel.Catalog.Persons.Count == 0)
                return;

            var writer = new WorkerWriter(taskWriter);
            writer.Write();
        }

        private void Write()
        {
            WriteToExternalFile(WriteWorkers);
        }

        private void WriteWorkers(XmlWriter writer)
        {
            foreach (var person in TaskWriter.DataModel.Catalog.Persons)
            {
                var workerId = WriteWorker(writer, person);
                TaskWriter.Workers[person.Id.ReferenceId] = workerId;
            }
        }

        private string WriteWorker(XmlWriter writer, Person person)
        {
            var workerId = GenerateId();
            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", workerId);
            writer.WriteAttributeString("B", person.LastName);
            writer.WriteXmlAttribute("C", person.FirstName);

            writer.WriteEndElement();

            return workerId;
        }
    }
}