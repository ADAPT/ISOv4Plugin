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
            base.WriteXML(xmlBuilder);
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
            customer.ProprietarySchemaExtensions = ReadProperietarySchemaExtensions(customerNode);
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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.CustomerId, 14, errors, "A");
            RequireString(this, x => x.CustomerLastName, 32, errors, "B");
            ValidateString(this, x => x.CustomerFirstName, 32, errors, "C");
            ValidateString(this, x => x.CustomerStreet, 32, errors, "D");
            ValidateString(this, x => x.CustomerPOBox, 32, errors, "E");
            ValidateString(this, x => x.CustomerPostalCode, 10, errors, "F");
            ValidateString(this, x => x.CustomerCity, 32, errors, "G");
            ValidateString(this, x => x.CustomerState, 32, errors, "H");
            ValidateString(this, x => x.CustomerCountry, 32, errors, "I");
            ValidateString(this, x => x.CustomerPhone, 20, errors, "J");
            ValidateString(this, x => x.CustomerMobile, 20, errors, "K");
            ValidateString(this, x => x.CustomerFax, 20, errors, "L");
            ValidateString(this, x => x.CustomerEmail, 64, errors, "M");
            return errors;
        }
    }
}
