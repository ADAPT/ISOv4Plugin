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
    public class ISOLineString : ISOElement
    {
        public ISOLineString()
        {
            Points = new List<ISOPoint>();
        }

        //Attributes
        public ISOLineStringType LineStringType { get; set; }
        public string LineStringDesignator { get; set; }
        public long? LineStringWidth { get; set; }
        public long? LineStringLength { get; set; }
        public byte? LineStringColour { get; set; }
        public string LineStringId { get; set; }

        //Child Elements
        public List<ISOPoint> Points { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("LSG");
            xmlBuilder.WriteXmlAttribute("A", ((int)LineStringType).ToString());
            xmlBuilder.WriteXmlAttribute("B", LineStringDesignator);
            xmlBuilder.WriteXmlAttribute<long>("C", LineStringWidth);
            xmlBuilder.WriteXmlAttribute<long>("D", LineStringLength);
            xmlBuilder.WriteXmlAttribute<byte>("E", LineStringColour);
            xmlBuilder.WriteXmlAttribute("F", LineStringId);

            foreach (var item in Points)
            {
                item.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOLineString ReadXML(XmlNode node)
        {
            ISOLineString lsg = new ISOLineString();
            lsg.LineStringType = (ISOLineStringType)(node.GetXmlNodeValueAsInt("@A"));
            lsg.LineStringDesignator = node.GetXmlNodeValue("@B");
            lsg.LineStringWidth = node.GetXmlNodeValueAsNullableLong("@C");
            lsg.LineStringLength = node.GetXmlNodeValueAsNullableLong("@D");
            lsg.LineStringColour = node.GetXmlNodeValueAsNullableByte("@E");
            lsg.LineStringId = node.GetXmlNodeValue("@F");

            XmlNodeList pntNodes = node.SelectNodes("PNT");
            if (pntNodes != null)
            {
                lsg.Points.AddRange(ISOPoint.ReadXML(pntNodes));
            }

            return lsg;
        }

        public static List<ISOLineString> ReadXML(XmlNodeList nodes)
        {
            List<ISOLineString> items = new List<ISOLineString>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOLineString.ReadXML(node));
            }
            return items;
        }
    }
}