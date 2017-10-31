/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOOperTechPractice : ISOElement
    {
        //Attributes
        public string CulturalPracticeIdRef { get; set; }
        public string OperationTechniqueIdRef { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("OTP");
            xmlBuilder.WriteXmlAttribute("A", CulturalPracticeIdRef);
            xmlBuilder.WriteXmlAttribute("B", OperationTechniqueIdRef);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOOperTechPractice ReadXML(XmlNode node)
        {
            ISOOperTechPractice item = new ISOOperTechPractice();
            item.CulturalPracticeIdRef = node.GetXmlNodeValue("@A");
            item.OperationTechniqueIdRef = node.GetXmlNodeValue("@B");
            return item;
        }

        public static IEnumerable<ISOOperTechPractice> ReadXML(XmlNodeList nodes)
        {
            List<ISOOperTechPractice> items = new List<ISOOperTechPractice>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOOperTechPractice.ReadXML(node));
            }
            return items;
        }
    }
}