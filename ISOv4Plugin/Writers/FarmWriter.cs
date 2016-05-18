using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class FarmWriter : BaseWriter
    {
        private FarmWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "FRM")
        {
        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Farms == null ||
                taskWriter.DataModel.Catalog.Farms.Count == 0)
                return;

            var writer = new FarmWriter(taskWriter);
            writer.WriteFarms(taskWriter.RootWriter);
        }

        private void WriteFarms(XmlWriter writer)
        {
            foreach (var farm in TaskWriter.DataModel.Catalog.Farms)
            {
                var farmId = WriteFarm(writer, farm);
                TaskWriter.Farms[farm.Id.ReferenceId] = farmId;
            }
        }

        private string WriteFarm(XmlWriter writer, Farm farm)
        {
            var farmId = GenerateId();
            TaskWriter.Ids.Add(farmId, farm.Id);

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", farmId);
            writer.WriteAttributeString("B", farm.Description);

            WriteContactInfo(writer, farm.ContactInfo);
            WriteCustomerReference(writer, farm.GrowerId);

            writer.WriteEndElement();

            return farmId;
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
        }

        private void WriteCustomerReference(XmlWriter writer, int? growerId)
        {
            if (!growerId.HasValue)
                return;

            writer.WriteXmlAttribute("I", TaskWriter.Customers.FindById(growerId.Value));
        }
    }
}