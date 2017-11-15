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
    public class ISOGuidancePattern : ISOElement
    {
        public ISOGuidancePattern()
        {
            BoundaryPolygons = new List<ISOPolygon>();
        }

        //Attributes
        public string GuidancePatternId { get; set; }
        public string GuidancePatternDesignator { get; set; }
        public ISOGuidancePatternType GuidancePatternType { get; set; }
        public ISOGuidancePatternOption? GuidancePatternOptions { get; set; }
        public ISOGuidancePatternPropagationDirection? PropagationDirection { get; set; }
        public ISOGuidancePatternExtension? Extension { get; set; }
        public decimal? Heading { get; set; }
        public long? Radius { get; set; }
        public ISOGuidancePatternGNSSMethod? GNSSMethod { get; set; }
        public decimal? HorizontalAccuracy { get; set; }
        public decimal? VerticalAccuracy { get; set; }
        public string BaseStationRef { get; set; }
        public string OriginalSRID { get; set; }
        public long? NumberOfSwathsLeft { get; set; }
        public long? NumberOfSwathsRight { get; set; }

        //Child Elements
        public ISOLineString LineString { get; set; }
        public List<ISOPolygon> BoundaryPolygons { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GPN");
            xmlBuilder.WriteXmlAttribute("A", GuidancePatternId);
            xmlBuilder.WriteXmlAttribute("B", GuidancePatternDesignator);
            xmlBuilder.WriteXmlAttribute("C", ((int)GuidancePatternType).ToString());
            xmlBuilder.WriteXmlAttribute<ISOGuidancePatternOption>("D", GuidancePatternOptions);
            xmlBuilder.WriteXmlAttribute<ISOGuidancePatternPropagationDirection>("E", PropagationDirection);
            xmlBuilder.WriteXmlAttribute<ISOGuidancePatternExtension>("F", Extension);
            xmlBuilder.WriteXmlAttribute("G", Heading);
            xmlBuilder.WriteXmlAttribute("H", Radius);
            xmlBuilder.WriteXmlAttribute<ISOGuidancePatternGNSSMethod>("I", GNSSMethod);
            xmlBuilder.WriteXmlAttribute("J", HorizontalAccuracy);
            xmlBuilder.WriteXmlAttribute("K", VerticalAccuracy);
            xmlBuilder.WriteXmlAttribute("L", BaseStationRef);
            xmlBuilder.WriteXmlAttribute("M", OriginalSRID);
            xmlBuilder.WriteXmlAttribute("N", NumberOfSwathsLeft);
            xmlBuilder.WriteXmlAttribute("O", NumberOfSwathsRight);

            if (LineString != null)
            {
                LineString.WriteXML(xmlBuilder);
            }
            foreach (ISOPolygon item in BoundaryPolygons) { item.WriteXML(xmlBuilder); }


            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOGuidancePattern ReadXML(XmlNode node)
        {
            ISOGuidancePattern item = new ISOGuidancePattern();
            item.GuidancePatternId = node.GetXmlNodeValue("@A");
            item.GuidancePatternDesignator = node.GetXmlNodeValue("@B");
            item.GuidancePatternType = (ISOGuidancePatternType)(node.GetXmlNodeValueAsInt("@C"));
            item.GuidancePatternOptions = (ISOGuidancePatternOption?)(node.GetXmlNodeValueAsNullableInt("@D"));
            item.PropagationDirection = (ISOGuidancePatternPropagationDirection?)(node.GetXmlNodeValueAsNullableInt("@E"));
            item.Extension = (ISOGuidancePatternExtension?)(node.GetXmlNodeValueAsNullableInt("@F"));
            item.Heading = node.GetXmlNodeValueAsNullableDecimal("@G");
            item.Radius = node.GetXmlNodeValueAsNullableLong("@H");
            item.GNSSMethod = (ISOGuidancePatternGNSSMethod?)(node.GetXmlNodeValueAsNullableInt("@I"));
            item.HorizontalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@J");
            item.VerticalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@K");
            item.BaseStationRef = node.GetXmlNodeValue("@L");
            item.OriginalSRID = node.GetXmlNodeValue("@M");
            item.NumberOfSwathsLeft = node.GetXmlNodeValueAsNullableLong("@N");
            item.NumberOfSwathsRight = node.GetXmlNodeValueAsNullableLong("@O");

            XmlNode lsgNode = node.SelectSingleNode("LSG");
            if (lsgNode != null)
            {
                item.LineString = ISOLineString.ReadXML(lsgNode);
            }

            XmlNodeList plnNodes = node.SelectNodes("PLN");
            if (plnNodes != null)
            {
                item.BoundaryPolygons.AddRange(ISOPolygon.ReadXML(plnNodes));
            }

            return item;
        }

        public static IEnumerable<ISOGuidancePattern> ReadXML(XmlNodeList nodes)
        {
            List<ISOGuidancePattern> items = new List<ISOGuidancePattern>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOGuidancePattern.ReadXML(node));
            }
            return items;
        }
    }
}