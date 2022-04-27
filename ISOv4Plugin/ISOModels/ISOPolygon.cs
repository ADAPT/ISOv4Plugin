/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOPolygon : ISOElement
    {
        public ISOPolygon()
            :this(4)
        {
        }

        public ISOPolygon(int version)
            :base(version)
        {
            LineStrings = new List<ISOLineString>();
        }

        //Attributes
        public ISOPolygonType PolygonType { get { return (ISOPolygonType)PolygonTypeInt; } set { PolygonTypeInt = (int)value; } }
        private int PolygonTypeInt { get; set; }
        public string PolygonDesignator { get; set; }
        public uint? PolygonArea { get; set; }
        public byte? PolygonColour { get; set; }
        public string PolygonId { get; set; }

        //Child Elements
        public List<ISOLineString> LineStrings { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PLN");
            xmlBuilder.WriteXmlAttribute("A", ((int)PolygonType).ToString());
            xmlBuilder.WriteXmlAttribute("B", PolygonDesignator);
            xmlBuilder.WriteXmlAttribute<uint>("C", PolygonArea);
            xmlBuilder.WriteXmlAttribute<byte>("D", PolygonColour);
            if (Version > 3)
            {
                xmlBuilder.WriteXmlAttribute("E", PolygonId);
            }

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
            pln.PolygonTypeInt = node.GetXmlNodeValueAsInt("@A");
            pln.PolygonDesignator = node.GetXmlNodeValue("@B");
            pln.PolygonArea = node.GetXmlNodeValueAsNullableUInt("@C");
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

        public override List<IError> Validate(List<IError> errors)
        {
            ValidateEnumerationValue(typeof(ISOPolygonType), PolygonTypeInt, errors);
            ValidateString(this, x => x.PolygonDesignator, 32, errors, "B");
            if (PolygonArea.HasValue) ValidateRange<ISOPolygon, uint>(this, x => x.PolygonArea.Value, 0, uint.MaxValue - 2, errors, "C");
            if (PolygonColour.HasValue) ValidateRange<ISOPolygon, byte>(this, x => x.PolygonColour.Value, 0, 254, errors, "D");
            ValidateString(this, x => x.PolygonId, 14, errors, "E");
            if (RequireNonZeroCount(LineStrings, "LSG", errors)) LineStrings.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}
