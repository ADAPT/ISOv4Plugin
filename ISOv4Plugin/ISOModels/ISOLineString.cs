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
    public class ISOLineString : ISOElement
    {
        public ISOLineString()
            :this(4)
        {
        }
        public ISOLineString(int version)
            :base(version)
        {
            Points = new List<ISOPoint>();
        }

        //Attributes
        public ISOLineStringType LineStringType { get { return (ISOLineStringType)LineStringTypeInt; } set { LineStringTypeInt = (int)value; } }
        private int LineStringTypeInt { get; set; }
        public string LineStringDesignator { get; set; }
        public uint? LineStringWidth { get; set; }
        public uint? LineStringLength { get; set; }
        public byte? LineStringColour { get; set; }
        public string LineStringId { get; set; }

        //Child Elements
        public List<ISOPoint> Points { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("LSG");
            xmlBuilder.WriteXmlAttribute("A", ((int)LineStringType).ToString());
            xmlBuilder.WriteXmlAttribute("B", LineStringDesignator);
            xmlBuilder.WriteXmlAttribute("C", LineStringWidth);
            xmlBuilder.WriteXmlAttribute("D", LineStringLength);
            xmlBuilder.WriteXmlAttribute<byte>("E", LineStringColour);
            if (Version > 3)
            {
                xmlBuilder.WriteXmlAttribute("F", LineStringId);
            }

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
            lsg.LineStringTypeInt = node.GetXmlNodeValueAsInt("@A");
            lsg.LineStringDesignator = node.GetXmlNodeValue("@B");
            lsg.LineStringWidth = node.GetXmlNodeValueAsNullableUInt("@C");
            lsg.LineStringLength = node.GetXmlNodeValueAsNullableUInt("@D");
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

        public override List<IError> Validate(List<IError> errors)
        {
            ValidateEnumerationValue(typeof(ISOLineStringType), LineStringTypeInt, errors);
            ValidateString(this, x => x.LineStringDesignator, 32, errors, "B");
            if (LineStringWidth.HasValue) ValidateRange<ISOLineString, uint>(this, x => x.LineStringWidth.Value, 0, uint.MaxValue - 2, errors, "C");
            if (LineStringLength.HasValue) ValidateRange<ISOLineString, uint>(this, x => x.LineStringLength.Value, 0, uint.MaxValue - 2, errors, "D");
            if (LineStringColour.HasValue) ValidateRange<ISOLineString, byte>(this, x => x.LineStringColour.Value, 0, 254, errors, "E");
            ValidateString(this, x => x.LineStringId, 14, errors, "F"); 
            if (RequireNonZeroCount(Points, "PNT", errors)) Points.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}
