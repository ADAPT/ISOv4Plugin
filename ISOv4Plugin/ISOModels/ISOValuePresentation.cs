/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOValuePresentation : ISOElement
    {
        //Attributes
        public string ValuePresentationID { get; set; }
        public long Offset { get; set; }
        public double Scale { get; set; }
        public byte NumberOfDecimals { get; set; }
        public string UnitDesignator { get; set; }
        public string ColourLegendIdRef { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("VPN");
            xmlBuilder.WriteXmlAttribute("A", ValuePresentationID);
            xmlBuilder.WriteXmlAttribute<long>("B", Offset);
            xmlBuilder.WriteXmlAttribute<double>("C", Scale);
            xmlBuilder.WriteXmlAttribute<byte>("D", NumberOfDecimals);
            xmlBuilder.WriteXmlAttribute("E", UnitDesignator);
            xmlBuilder.WriteXmlAttribute("F", ColourLegendIdRef);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOValuePresentation ReadXML(XmlNode node)
        {
            ISOValuePresentation presentation = new ISOValuePresentation();
            presentation.ValuePresentationID = node.GetXmlNodeValue("@A");
            presentation.Offset = node.GetXmlNodeValueAsNullableLong("@B").Value;
            presentation.Scale = node.GetXmlNodeValueAsNullableDouble("@C").Value;
            presentation.NumberOfDecimals = byte.Parse(node.GetXmlNodeValue("@D"));
            presentation.UnitDesignator = node.GetXmlNodeValue("@E");
            presentation.ColourLegendIdRef = node.GetXmlNodeValue("@F");
            return presentation;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOValuePresentation> items = new List<ISOValuePresentation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOValuePresentation.ReadXML(node));
            }
            return items;
        }
    }
}