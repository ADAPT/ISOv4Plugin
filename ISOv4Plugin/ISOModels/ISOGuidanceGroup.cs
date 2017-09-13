/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOGuidanceGroup : ISOElement
    {
        public ISOGuidanceGroup()
        {
            GuidancePatterns = new List<ISOGuidancePattern>();
            BoundaryPolygons = new List<ISOPolygon>();
        }

        //Attributes
        public string GuidanceGroupId  { get; set; }
        public string GuidanceGroupDesignator { get; set; }

        //Child Elements
        public List<ISOGuidancePattern> GuidancePatterns { get; set; }
        public List<ISOPolygon> BoundaryPolygons { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GGP");
            xmlBuilder.WriteXmlAttribute("A", GuidanceGroupId );
            xmlBuilder.WriteXmlAttribute("B", GuidanceGroupDesignator );
            foreach (ISOGuidancePattern item in GuidancePatterns) { item.WriteXML(xmlBuilder); }
            foreach (ISOPolygon item in BoundaryPolygons) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOGuidanceGroup ReadXML(XmlNode node)
        {
            ISOGuidanceGroup item = new ISOGuidanceGroup();
            item.GuidanceGroupId  = node.GetXmlNodeValue("@A");
            item.GuidanceGroupDesignator  = node.GetXmlNodeValue("@B");

            XmlNodeList gpnNodes = node.SelectNodes("GPN");
            if (gpnNodes != null)
            {
                item.GuidancePatterns.AddRange(ISOGuidancePattern.ReadXML(gpnNodes));
            }

            XmlNodeList plnNodes = node.SelectNodes("PLN");
            if (plnNodes != null)
            {
                item.BoundaryPolygons.AddRange(ISOPolygon.ReadXML(plnNodes));
            }
            return item;
        }

        public static IEnumerable<ISOGuidanceGroup> ReadXML(XmlNodeList nodes)
        {
            List<ISOGuidanceGroup> items = new List<ISOGuidanceGroup>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOGuidanceGroup.ReadXML(node));
            }
            return items;
        }
    }
}