using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class WorkerWriter : BaseWriter
    {
        private WorkerWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "WKR")
        {
        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Persons == null ||
                taskWriter.DataModel.Catalog.Persons.Count == 0)
                return;

            var writer = new WorkerWriter(taskWriter);
            writer.WriteWorkers(taskWriter.RootWriter);
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

            var contactInfo = FindContactInfo(person.ContactInfoId);
            WriteContactInfo(writer, contactInfo);

            writer.WriteEndElement();

            return workerId;
        }

        private ContactInfo FindContactInfo(int contactInfoId)
        {
            if (TaskWriter.DataModel.Catalog.ContactInfo == null ||
                TaskWriter.DataModel.Catalog.ContactInfo.Count == 0)
                return null;

            foreach (var contactInfo in TaskWriter.DataModel.Catalog.ContactInfo)
            {
                if (contactInfoId == contactInfo.Id.ReferenceId)
                    return contactInfo;
            }
            return null;
        }

        private static void WriteContactInfo(XmlWriter writer, ContactInfo contactInfo)
        {
            if (contactInfo == null)
                return;

            writer.WriteXmlAttribute("D", contactInfo.AddressLine1);
            writer.WriteXmlAttribute("E", contactInfo.PoBoxNumber);
            writer.WriteXmlAttribute("F", contactInfo.PostalCode);
            writer.WriteXmlAttribute("G", contactInfo.City);
            writer.WriteXmlAttribute("H", contactInfo.StateOrProvince);
            writer.WriteXmlAttribute("I", contactInfo.Country);

            WriteContacts(writer, contactInfo.Contacts);
        }

        private static void WriteContacts(XmlWriter writer, List<Contact> contacts)
        {
            if (contacts == null || contacts.Count == 0)
                return;

            writer.WriteXmlAttribute("J", GetContactForType(contacts, ContactTypeEnum.FixedPhone));
            writer.WriteXmlAttribute("K", GetContactForType(contacts, ContactTypeEnum.MobilePhone));
            writer.WriteXmlAttribute("L", GetContactForType(contacts, ContactTypeEnum.Fax));
            writer.WriteXmlAttribute("M", GetContactForType(contacts, ContactTypeEnum.Email));
        }

        private static string GetContactForType(List<Contact> contacts, ContactTypeEnum contactType)
        {
            foreach (var contact in contacts)
            {
                if (contact.Type == contactType)
                    return contact.Number;
            }
            return null;
        }
    }
}