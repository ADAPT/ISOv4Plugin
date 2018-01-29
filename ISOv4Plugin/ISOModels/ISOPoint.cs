/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOPoint : ISOElement
    {
        //Attributes
        public ISOPointType PointType { get { return (ISOPointType)PointTypeInt; } set { PointTypeInt = (int)value; } }
        private int PointTypeInt { get; set; }
        public string PointDesignator { get; set; }
        public decimal PointNorth { get; set; }
        public decimal PointEast { get; set; }
        public int? PointUp { get; set; }
        public byte? PointColour { get; set; }
        public string PointId { get; set; }
        public decimal? PointHorizontalAccuracy { get; set; }
        public decimal? PointVerticalAccuracy { get; set; }
        public string Filename { get; set; }
        public uint? Filelength { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PNT");
            xmlBuilder.WriteXmlAttribute("A", ((int)PointType).ToString());
            xmlBuilder.WriteXmlAttribute("B", PointDesignator);
            xmlBuilder.WriteXmlAttribute<decimal>("C", PointNorth);
            xmlBuilder.WriteXmlAttribute<decimal>("D", PointEast);
            xmlBuilder.WriteXmlAttribute<int>("E", PointUp);
            xmlBuilder.WriteXmlAttribute<byte>("F", PointColour);
            xmlBuilder.WriteXmlAttribute("G", PointId);
            xmlBuilder.WriteXmlAttribute<decimal>("H", PointHorizontalAccuracy);
            xmlBuilder.WriteXmlAttribute<decimal>("I", PointVerticalAccuracy);
            xmlBuilder.WriteXmlAttribute("J", Filename);
            xmlBuilder.WriteXmlAttribute<uint>("K", Filelength);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOPoint ReadXML(XmlNode node)
        {
            ISOPoint point = new ISOPoint();
            point.PointTypeInt = node.GetXmlNodeValueAsInt("@A");
            point.PointDesignator = node.GetXmlNodeValue("@B");
            point.PointNorth = node.GetXmlNodeValueAsDecimal("@C");
            point.PointEast = node.GetXmlNodeValueAsDecimal("@D");
            point.PointUp = node.GetXmlNodeValueAsNullableInt("@E");
            point.PointColour = node.GetXmlNodeValueAsNullableByte("@F");
            point.PointId = node.GetXmlNodeValue("@G");
            point.PointHorizontalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@H");
            point.PointVerticalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@I");
            point.Filename = node.GetXmlNodeValue("@J");
            point.Filelength = node.GetXmlNodeValueAsNullableUInt("@K");

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

        public override List<Error> Validate(List<Error> errors)
        {
            ValidateEnumerationValue(typeof(ISOPointType), PointTypeInt, errors);
            ValidateString(this, x => x.PointDesignator, 32, errors, "B");
            RequireRange(this, x => x.PointNorth, -90m, 90m, errors, "C");
            RequireRange(this, x => x.PointEast, -180m, 180m, errors, "D");
            if (PointUp.HasValue) ValidateRange(this, x => x.PointUp.Value, Int32.MinValue +1, Int32.MaxValue - 1, errors, "E");
            if (PointColour.HasValue) ValidateRange<ISOPoint, byte>(this, x => x.PointColour.Value, 0, 254, errors, "F");
            ValidateString(this, x => x.PointId, 14, errors, "G");
            if (PointHorizontalAccuracy.HasValue) ValidateRange(this, x => x.PointHorizontalAccuracy.Value, 0m, 65m, errors, "H");
            if (PointVerticalAccuracy.HasValue) ValidateRange(this, x => x.PointVerticalAccuracy.Value, 0m, 65m, errors, "I");
            ValidateString(this, x => x.Filename, 8, errors, "J");
            if (Filelength.HasValue) ValidateRange<ISOPoint, uint>(this, x => x.Filelength.Value, 0, uint.MaxValue - 2, errors, "K");
            return errors;
        }
    }
}