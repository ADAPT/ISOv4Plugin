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
    public class ISOGuidancePattern : ISOElement
    {
        public ISOGuidancePattern()
        {
            BoundaryPolygons = new List<ISOPolygon>();
        }

        //Attributes
        public string GuidancePatternId { get; set; }
        public string GuidancePatternDesignator { get; set; }
        public ISOGuidancePatternType GuidancePatternType { get { return (ISOGuidancePatternType)GuidancePatternTypeInt; } set { GuidancePatternTypeInt = (int)value; } }
        private int GuidancePatternTypeInt { get; set; }
        public ISOGuidancePatternOption? GuidancePatternOptions { get { return (ISOGuidancePatternOption?)GuidancePatternOptionsInt; } set { GuidancePatternOptionsInt = (int?)value; } }
        private int? GuidancePatternOptionsInt { get; set; }
        public ISOGuidancePatternPropagationDirection? PropagationDirection { get { return (ISOGuidancePatternPropagationDirection?)PropagationDirectionInt; } set { PropagationDirectionInt = (int?)value; } }
        private int? PropagationDirectionInt { get; set; }
        public ISOGuidancePatternExtension? Extension { get { return (ISOGuidancePatternExtension?)ExtensionInt; } set { ExtensionInt = (int?)value; } }
        private int? ExtensionInt { get; set; }
        public decimal? Heading { get; set; }
        public uint? Radius { get; set; }
        public ISOGuidancePatternGNSSMethod? GNSSMethod { get { return (ISOGuidancePatternGNSSMethod?)GNSSMethodInt; } set { GNSSMethodInt = (int?)value; } }
        private int? GNSSMethodInt { get; set; }
        public decimal? HorizontalAccuracy { get; set; }
        public decimal? VerticalAccuracy { get; set; }
        public string BaseStationRef { get; set; }
        public string OriginalSRID { get; set; }
        public uint? NumberOfSwathsLeft { get; set; }
        public uint? NumberOfSwathsRight { get; set; }

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
            item.GuidancePatternTypeInt = node.GetXmlNodeValueAsInt("@C");
            item.GuidancePatternOptionsInt = node.GetXmlNodeValueAsNullableInt("@D");
            item.PropagationDirectionInt = node.GetXmlNodeValueAsNullableInt("@E");
            item.ExtensionInt = node.GetXmlNodeValueAsNullableInt("@F");
            item.Heading = node.GetXmlNodeValueAsNullableDecimal("@G");
            item.Radius = node.GetXmlNodeValueAsNullableUInt("@H");
            item.GNSSMethodInt = node.GetXmlNodeValueAsNullableInt("@I");
            item.HorizontalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@J");
            item.VerticalAccuracy = node.GetXmlNodeValueAsNullableDecimal("@K");
            item.BaseStationRef = node.GetXmlNodeValue("@L");
            item.OriginalSRID = node.GetXmlNodeValue("@M");
            item.NumberOfSwathsLeft = node.GetXmlNodeValueAsNullableUInt("@N");
            item.NumberOfSwathsRight = node.GetXmlNodeValueAsNullableUInt("@O");

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

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.GuidancePatternId, 14, errors, "A");
            ValidateString(this, x => x.GuidancePatternDesignator, 32, errors, "B");
            ValidateEnumerationValue(typeof(ISOGuidancePatternType), GuidancePatternTypeInt, errors);
            if (GuidancePatternOptionsInt.HasValue) ValidateEnumerationValue(typeof(ISOGuidancePatternOption), GuidancePatternOptionsInt.Value, errors);
            if (PropagationDirectionInt.HasValue) ValidateEnumerationValue(typeof(ISOGuidancePatternPropagationDirection), PropagationDirectionInt.Value, errors);
            if (ExtensionInt.HasValue) ValidateEnumerationValue(typeof(ISOGuidancePatternExtension), ExtensionInt.Value, errors);
            if (GNSSMethodInt.HasValue) ValidateEnumerationValue(typeof(ISOGuidancePatternGNSSMethod), GNSSMethodInt.Value, errors);
            if (HorizontalAccuracy.HasValue) ValidateRange(this, x => x.HorizontalAccuracy.Value, 0m, 65m, errors, "J");
            if (VerticalAccuracy.HasValue) ValidateRange(this, x => x.VerticalAccuracy.Value, 0m, 65m, errors, "K");
            ValidateString(this, x => x.BaseStationRef, 14, errors, "L");
            ValidateString(this, x => x.OriginalSRID, 32, errors, "M");
            if (NumberOfSwathsLeft.HasValue) ValidateRange<ISOGuidancePattern, uint>(this, x => x.NumberOfSwathsLeft.Value, 0, uint.MaxValue - 2, errors, "N");
            if (NumberOfSwathsRight.HasValue) ValidateRange<ISOGuidancePattern, uint>(this, x => x.NumberOfSwathsRight.Value, 0, uint.MaxValue - 2, errors, "O");
            if (RequireChildElement(LineString, "LSG", errors)) LineString.Validate(errors);
            if (BoundaryPolygons.Count > 0) BoundaryPolygons.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}