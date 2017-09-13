/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOPartfield : ISOElement
    {
        public ISOPartfield()
        {
            Polygons = new List<ISOPolygon>();
            LineStrings = new List<ISOLineString>();
            Points = new List<ISOPoint>();
            GuidanceGroups = new List<ISOGuidanceGroup>();
        }

        //Attributes
        public string PartfieldID { get; set; }
        public string PartfieldCode { get; set; }
        public string PartfieldDesignator { get; set; }
        public long? PartfieldArea { get; set; }
        public string CustomerIdRef { get; set; }
        public string FarmIdRef { get; set; }
        public string CropTypeIdRef { get; set; }
        public string CropVarietyIdRef { get; set; }
        public string FieldIdRef { get; set; }

        //Child Elements
        public List<ISOPolygon> Polygons { get; set; }
        public List<ISOLineString> LineStrings { get; set;}
        public List<ISOPoint> Points { get; set; }
        public List<ISOGuidanceGroup> GuidanceGroups { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PFD");
            xmlBuilder.WriteXmlAttribute("A", PartfieldID);
            xmlBuilder.WriteXmlAttribute("B", PartfieldCode);
            xmlBuilder.WriteXmlAttribute("C", PartfieldDesignator);
            xmlBuilder.WriteXmlAttribute<long>("D", PartfieldArea);
            xmlBuilder.WriteXmlAttribute("E", CustomerIdRef);
            xmlBuilder.WriteXmlAttribute("F", FarmIdRef);
            xmlBuilder.WriteXmlAttribute("G", CropTypeIdRef);
            xmlBuilder.WriteXmlAttribute("H", CropVarietyIdRef);
            xmlBuilder.WriteXmlAttribute("I", FieldIdRef);
            foreach (ISOPolygon item in Polygons) { item.WriteXML(xmlBuilder); }
            foreach (ISOLineString item in LineStrings) { item.WriteXML(xmlBuilder); }
            foreach (ISOPoint item in Points) { item.WriteXML(xmlBuilder); }
            foreach (ISOGuidanceGroup item in GuidanceGroups) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOPartfield ReadXML(XmlNode node)
        {
            ISOPartfield field = new ISOPartfield();
            field.PartfieldID = node.GetXmlNodeValue("@A");
            field.PartfieldCode = node.GetXmlNodeValue("@B");
            field.PartfieldDesignator = node.GetXmlNodeValue("@C");
            field.PartfieldArea = node.GetXmlNodeValueAsNullableLong("@D");
            field.CustomerIdRef = node.GetXmlNodeValue("@E");
            field.FarmIdRef = node.GetXmlNodeValue("@F");
            field.CropTypeIdRef = node.GetXmlNodeValue("@G");
            field.CropVarietyIdRef = node.GetXmlNodeValue("@H");
            field.FieldIdRef = node.GetXmlNodeValue("@I");

            XmlNodeList plnNodes = node.SelectNodes("PLN");
            if (plnNodes != null)
            {
                field.Polygons.AddRange(ISOPolygon.ReadXML(plnNodes));
            }

            XmlNodeList lsgNodes = node.SelectNodes("LSG");
            if (lsgNodes != null)
            {
                field.LineStrings.AddRange(ISOLineString.ReadXML(lsgNodes));
            }

            XmlNodeList pntNodes = node.SelectNodes("PNT");
            if (pntNodes != null)
            {
                field.Points.AddRange(ISOPoint.ReadXML(pntNodes));
            }

            XmlNodeList ggpNodes = node.SelectNodes("GGP");
            if (ggpNodes != null)
            {
                field.GuidanceGroups.AddRange(ISOGuidanceGroup.ReadXML(ggpNodes));
            }

            return field;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOPartfield> fields = new List<ISOPartfield>();
            foreach (XmlNode fieldNode in nodes)
            {
                fields.Add(ISOPartfield.ReadXML(fieldNode));
            }
            return fields;
        }
    }
}