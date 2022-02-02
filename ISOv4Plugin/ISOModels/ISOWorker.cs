/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOWorker : ISOElement
    {
        //Attributes
        public string WorkerId { get; set; }
        public string WorkerLastName { get; set; }
        public string WorkerFirstName { get; set; }
        public string WorkerStreet { get; set; }
        public string WorkerPOBox { get; set; }
        public string WorkerPostalCode { get; set; }
        public string WorkerCity { get; set; }
        public string WorkerState { get; set; }
        public string WorkerCountry { get; set; }
        public string WorkerPhone { get; set; }
        public string WorkerMobile { get; set; }
        public string WorkerLicenseNumber { get; set; }
        public string WorkerEmail { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("WKR");
            xmlBuilder.WriteXmlAttribute("A", WorkerId);
            xmlBuilder.WriteXmlAttribute("B", WorkerLastName);
            xmlBuilder.WriteXmlAttribute("C", WorkerFirstName);
            xmlBuilder.WriteXmlAttribute("D", WorkerStreet);
            xmlBuilder.WriteXmlAttribute("E", WorkerPOBox);
            xmlBuilder.WriteXmlAttribute("F", WorkerPostalCode);
            xmlBuilder.WriteXmlAttribute("G", WorkerCity);
            xmlBuilder.WriteXmlAttribute("H", WorkerState);
            xmlBuilder.WriteXmlAttribute("I", WorkerCountry);
            xmlBuilder.WriteXmlAttribute("J", WorkerPhone);
            xmlBuilder.WriteXmlAttribute("K", WorkerMobile);
            xmlBuilder.WriteXmlAttribute("L", WorkerLicenseNumber);
            xmlBuilder.WriteXmlAttribute("M", WorkerEmail);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOWorker ReadXML(XmlNode node)
        {
            ISOWorker worker = new ISOWorker();
            worker.WorkerId = node.GetXmlNodeValue("@A");
            worker.WorkerLastName = node.GetXmlNodeValue("@B");
            worker.WorkerFirstName = node.GetXmlNodeValue("@C");
            worker.WorkerStreet = node.GetXmlNodeValue("@D");
            worker.WorkerPOBox = node.GetXmlNodeValue("@E");
            worker.WorkerPostalCode = node.GetXmlNodeValue("@F");
            worker.WorkerCity = node.GetXmlNodeValue("@G");
            worker.WorkerState = node.GetXmlNodeValue("@H");
            worker.WorkerCountry = node.GetXmlNodeValue("@I");
            worker.WorkerPhone = node.GetXmlNodeValue("@J");
            worker.WorkerMobile = node.GetXmlNodeValue("@K");
            worker.WorkerLicenseNumber = node.GetXmlNodeValue("@L");
            worker.WorkerEmail = node.GetXmlNodeValue("@M");
            worker.ProprietarySchemaExtensions = ReadProperietarySchemaExtensions(node);
            return worker;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOWorker> workers = new List<ISOWorker>();
            foreach (XmlNode node in nodes)
            {
                workers.Add(ISOWorker.ReadXML(node));
            }
            return workers;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.WorkerId, 14, errors, "A");
            RequireString(this, x => x.WorkerLastName, 32, errors, "B");
            ValidateString(this, x => x.WorkerFirstName, 32, errors, "C");
            ValidateString(this, x => x.WorkerStreet, 32, errors, "D");
            ValidateString(this, x => x.WorkerPOBox, 32, errors, "E");
            ValidateString(this, x => x.WorkerPostalCode, 10, errors, "F");
            ValidateString(this, x => x.WorkerCity, 32, errors, "G");
            ValidateString(this, x => x.WorkerState, 32, errors, "H");
            ValidateString(this, x => x.WorkerCountry, 32, errors, "I");
            ValidateString(this, x => x.WorkerPhone, 20, errors, "J");
            ValidateString(this, x => x.WorkerMobile, 20, errors, "K");
            ValidateString(this, x => x.WorkerLicenseNumber, 32, errors, "L");
            ValidateString(this, x => x.WorkerEmail, 64, errors, "M");
            return errors;
        }
    }
}
