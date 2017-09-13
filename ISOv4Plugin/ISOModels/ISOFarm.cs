/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOFarm : ISOElement
    {
        //Attributes
        public string FarmId { get; set; }
        public string FarmDesignator { get; set; }
        public string FarmStreet { get; set; }
        public string FarmPOBox { get; set; }
        public string FarmPostalCode { get; set; }
        public string FarmCity { get; set; }
        public string FarmState { get; set; }
        public string FarmCountry { get; set; }
        public string CustomerIdRef { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("FRM");
            xmlBuilder.WriteXmlAttribute("A", FarmId);
            xmlBuilder.WriteXmlAttribute("B", FarmDesignator);
            xmlBuilder.WriteXmlAttribute("C", FarmStreet);
            xmlBuilder.WriteXmlAttribute("D", FarmPOBox);
            xmlBuilder.WriteXmlAttribute("E", FarmPostalCode);
            xmlBuilder.WriteXmlAttribute("F", FarmCity);
            xmlBuilder.WriteXmlAttribute("G", FarmState);
            xmlBuilder.WriteXmlAttribute("H", FarmCountry);
            xmlBuilder.WriteXmlAttribute("I", CustomerIdRef);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOFarm ReadXML(XmlNode node)
        {
            ISOFarm farm = new ISOFarm();
            farm.FarmId = node.GetXmlNodeValue("@A");
            farm.FarmDesignator = node.GetXmlNodeValue("@B");
            farm.FarmStreet = node.GetXmlNodeValue("@C");
            farm.FarmPOBox = node.GetXmlNodeValue("@D");
            farm.FarmPostalCode = node.GetXmlNodeValue("@E");
            farm.FarmCity = node.GetXmlNodeValue("@F");
            farm.FarmState = node.GetXmlNodeValue("@G");
            farm.FarmCountry = node.GetXmlNodeValue("@H");
            farm.CustomerIdRef = node.GetXmlNodeValue("@I");
            return farm;
        }

        public static List<ISOFarm> ReadXML(XmlNodeList farmNodes)
        {
            List<ISOFarm> farms = new List<ISOFarm>();
            foreach (XmlNode farmNode in farmNodes)
            {
                farms.Add(ISOFarm.ReadXML(farmNode));
            }
            return farms;
        }
    }
}
