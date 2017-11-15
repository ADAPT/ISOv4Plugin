/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOAllocationStamp : ISOElement
    {
        public ISOAllocationStamp()
        {
            Positions = new List<ISOPosition>();
        }

        //Attributes
        public DateTime? Start { get; set; }  
        public DateTime? Stop { get; set; }
        public long? Duration { get; set; }
        public ISOAllocationStampType Type { get; set; }

        //Child Elements
        public List<ISOPosition> Positions { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("ASP");
            xmlBuilder.WriteXmlAttribute("A", Start.HasValue ? Start.Value.ToString("yyyy-MM-ddThh:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute("B", Stop.HasValue ? Stop.Value.ToString("yyyy-MM-ddThh:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute<long>("C", Duration);
            xmlBuilder.WriteXmlAttribute("D", ((int)Type).ToString());
            foreach (ISOPosition item in Positions) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOAllocationStamp ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISOAllocationStamp item = new ISOAllocationStamp();
            item.Start = node.GetXmlNodeValueAsNullableDateTime("@A");
            item.Stop = node.GetXmlNodeValueAsNullableDateTime("@B");
            item.Duration = node.GetXmlNodeValueAsNullableLong("@C");
            item.Type = (ISOAllocationStampType)(node.GetXmlNodeValueAsInt("@D"));

            XmlNodeList ptnNodes = node.SelectNodes("PTN");
            if (ptnNodes != null)
            {
                item.Positions.AddRange(ISOPosition.ReadXML(ptnNodes));
            }

            return item;
        }

        public static List<ISOAllocationStamp> ReadXML(XmlNodeList nodes)
        {
            List<ISOAllocationStamp> items = new List<ISOAllocationStamp>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOAllocationStamp.ReadXML(node));
            }
            return items;
        }
    }
}