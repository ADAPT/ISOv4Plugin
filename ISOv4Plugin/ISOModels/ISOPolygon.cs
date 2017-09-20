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
    public class ISOPolygon : ISOElement
    {
        public ISOPolygon()
        {
            LineStrings = new List<ISOLineString>();
        }

        //Attributes
        public ISOPolygonType PolygonType { get; set; }
        public string PolygonDesignator { get; set; }
        public long? PolygonArea { get; set; }
        public byte? PolygonColour { get; set; }
        public string PolygonId { get; set; }

        //Child Elements
        public List<ISOLineString> LineStrings { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PLN");
            xmlBuilder.WriteXmlAttribute("A", ((int)PolygonType).ToString());
            xmlBuilder.WriteXmlAttribute("B", PolygonDesignator);
            xmlBuilder.WriteXmlAttribute<long>("C", PolygonArea);
            xmlBuilder.WriteXmlAttribute<byte>("D", PolygonColour);
            xmlBuilder.WriteXmlAttribute("E", PolygonId);

            foreach (var item in LineStrings)
            {
                item.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOPolygon ReadXML(XmlNode node)
        {
            ISOPolygon pln = new ISOPolygon();
            pln.PolygonType = (ISOPolygonType)(Int32.Parse(node.GetXmlNodeValue("@A")));
            pln.PolygonDesignator = node.GetXmlNodeValue("@B");
            pln.PolygonArea = node.GetXmlNodeValueAsNullableLong("@C");
            pln.PolygonColour = node.GetXmlNodeValueAsNullableByte("@D");
            pln.PolygonId = node.GetXmlNodeValue("@E");

            XmlNodeList lsgNodes = node.SelectNodes("LSG");
            if (lsgNodes != null)
            {
                pln.LineStrings.AddRange(ISOLineString.ReadXML(lsgNodes));
            }

            return pln;
        }

        public static List<ISOPolygon> ReadXML(XmlNodeList nodes)
        {
            List<ISOPolygon> items = new List<ISOPolygon>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOPolygon.ReadXML(node));
            }
            return items;
        }
    }
}