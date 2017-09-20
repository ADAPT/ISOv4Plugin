/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

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
    }
}