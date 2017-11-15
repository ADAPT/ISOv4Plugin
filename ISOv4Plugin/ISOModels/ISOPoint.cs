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
    public class ISOPoint : ISOElement
    {
        //Attributes
        public ISOPointType PointType { get; set; }
        public string PointDesignator { get; set; }
        public decimal? PointNorth { get; set; }
        public decimal? PointEast { get; set; }
        public long? PointUp { get; set; }
        public byte? PointColour { get; set; }
        public string PointId { get; set; }
        public decimal? PointHorizontalAccuracy { get; set; }
        public decimal? PointVerticalAccuracy { get; set; }
        public string Filename { get; set; }
        public long? Filelength { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PNT");
            xmlBuilder.WriteXmlAttribute("A", ((int)PointType).ToString());
            xmlBuilder.WriteXmlAttribute("B", PointDesignator);
            xmlBuilder.WriteXmlAttribute<decimal>("C", PointNorth);
            xmlBuilder.WriteXmlAttribute<decimal>("D", PointEast);
            xmlBuilder.WriteXmlAttribute<long>("E", PointUp);
            xmlBuilder.WriteXmlAttribute<byte>("F", PointColour);
            xmlBuilder.WriteXmlAttribute("G", PointId);
            xmlBuilder.WriteXmlAttribute<decimal>("H", PointHorizontalAccuracy);
            xmlBuilder.WriteXmlAttribute<decimal>("I", PointVerticalAccuracy);
            xmlBuilder.WriteXmlAttribute("J", Filename);
            xmlBuilder.WriteXmlAttribute<long>("K", Filelength);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOPoint ReadXML(XmlNode node)
        {
            ISOPoint point = new ISOPoint();
            point.PointType = (ISOPointType)(node.GetXmlNodeValueAsInt("@A"));
            point.PointDesignator = node.GetXmlNodeValue("@B");
            point.PointNorth = node.GetXmlNodeValueAsNullableDecimal("@C");
            point.PointEast = node.GetXmlNodeValueAsNullableDecimal("@D");
            point.PointUp = node.GetXmlNodeValueAsNullableLong("@E");
            point.PointColour = node.GetXmlNodeValueAsNullableByte("@F");
            point.PointId = node.GetXmlNodeValue("@G");
            point.PointHorizontalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@H");
            point.PointVerticalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@I");
            point.Filename = node.GetXmlNodeValue("@J");
            point.Filelength = node.GetXmlNodeValueAsNullableLong("@K");

            return point;
        }

        public static List<ISOPoint> ReadXML(XmlNodeList nodes)
        {
            List<ISOPoint> items = new List<ISOPoint>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOPoint.ReadXML(node));
            }
            return items;
        }
    }
}