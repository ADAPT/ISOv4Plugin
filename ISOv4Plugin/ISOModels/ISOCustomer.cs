/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOCustomer : ISOElement
    {
        //Attributes
        public string CustomerId { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerStreet { get; set; }
        public string CustomerPOBox { get; set; }
        public string CustomerPostalCode { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerFax { get; set; }
        public string CustomerEmail { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CTR");
            xmlBuilder.WriteXmlAttribute("A", CustomerId);
            xmlBuilder.WriteXmlAttribute("B", CustomerLastName);
            xmlBuilder.WriteXmlAttribute("C", CustomerFirstName);
            xmlBuilder.WriteXmlAttribute("D", CustomerStreet);
            xmlBuilder.WriteXmlAttribute("E", CustomerPOBox);
            xmlBuilder.WriteXmlAttribute("F", CustomerPostalCode);
            xmlBuilder.WriteXmlAttribute("G", CustomerCity);
            xmlBuilder.WriteXmlAttribute("H", CustomerState);
            xmlBuilder.WriteXmlAttribute("I", CustomerCountry);
            xmlBuilder.WriteXmlAttribute("J", CustomerPhone);
            xmlBuilder.WriteXmlAttribute("K", CustomerMobile);
            xmlBuilder.WriteXmlAttribute("L", CustomerFax);
            xmlBuilder.WriteXmlAttribute("M", CustomerEmail);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOCustomer ReadXML(XmlNode customerNode)
        {
            ISOCustomer customer = new ISOCustomer();
            customer.CustomerId = customerNode.GetXmlNodeValue("@A");
            customer.CustomerLastName = customerNode.GetXmlNodeValue("@B");
            customer.CustomerFirstName = customerNode.GetXmlNodeValue("@C");
            customer.CustomerStreet = customerNode.GetXmlNodeValue("@D");
            customer.CustomerPOBox = customerNode.GetXmlNodeValue("@E");
            customer.CustomerPostalCode = customerNode.GetXmlNodeValue("@F");
            customer.CustomerCity = customerNode.GetXmlNodeValue("@G");
            customer.CustomerState = customerNode.GetXmlNodeValue("@H");
            customer.CustomerCountry = customerNode.GetXmlNodeValue("@I");
            customer.CustomerPhone = customerNode.GetXmlNodeValue("@J");
            customer.CustomerMobile = customerNode.GetXmlNodeValue("@K");
            customer.CustomerFax = customerNode.GetXmlNodeValue("@L");
            customer.CustomerEmail = customerNode.GetXmlNodeValue("@M");
            return customer;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList customerNodes)
        {
            List<ISOCustomer> customers = new List<ISOCustomer>();
            foreach (XmlNode customerNode in customerNodes)
            {
                customers.Add(ISOCustomer.ReadXML(customerNode));
            }
            return customers;
        }
    }
}