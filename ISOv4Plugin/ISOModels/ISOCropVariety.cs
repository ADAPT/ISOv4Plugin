/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOCropVariety : ISOElement
    {
        //Attributes
        public string CropVarietyId { get; set; }
        public string CropVarietyDesignator { get; set; }
        public string ProductIdRef { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CVT");
            xmlBuilder.WriteXmlAttribute("A", CropVarietyId);
            xmlBuilder.WriteXmlAttribute("B", CropVarietyDesignator);
            xmlBuilder.WriteXmlAttribute("C", ProductIdRef);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOCropVariety ReadXML(XmlNode node)
        {
            ISOCropVariety item = new ISOCropVariety();
            item.CropVarietyId = node.GetXmlNodeValue("@A");
            item.CropVarietyDesignator = node.GetXmlNodeValue("@B");
            item.ProductIdRef = node.GetXmlNodeValue("@C");

            return item;
        }

        public static IEnumerable<ISOCropVariety> ReadXML(XmlNodeList nodes)
        {
            List<ISOCropVariety> items = new List<ISOCropVariety>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOCropVariety.ReadXML(node));
            }
            return items;
        }
    }
}