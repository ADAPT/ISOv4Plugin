using AgGateway.ADAPT.ApplicationDataModel;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;
using System.Globalization;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class CustomerWriter : BaseWriter
    {
        private CustomerWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "CTR")
        {
        }

        internal static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Growers == null ||
                taskWriter.DataModel.Catalog.Growers.Count == 0)
                return;

            var writer = new CustomerWriter(taskWriter);
            writer.WriteCustomers();
        }

        private void WriteCustomers()
        {
            WriteToExternalFile(WriteCustomers);
        }

        private void WriteCustomers(XmlWriter writer)
        {
            foreach (var grower in TaskWriter.DataModel.Catalog.Growers)
            {
                var customerId = WriteCustomer(writer, grower);
                TaskWriter.Customers[grower.Id.ReferenceId] = customerId;
            }
        }

        private string WriteCustomer(XmlWriter writer, Grower grower)
        {
            var customerId = GenerateId();

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", customerId);
            writer.WriteAttributeString("B", grower.Name);

            WriteContactInfo(writer, grower.ContactInfo);

            writer.WriteEndElement();

            return customerId;
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