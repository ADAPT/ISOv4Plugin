/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOValuePresentation : ISOElement
    {
        //Attributes
        public string ValuePresentationID { get; set; }
        public int Offset { get; set; }
        public double Scale { get; set; }
        public byte NumberOfDecimals { get; set; }
        public string UnitDesignator { get; set; }
        public string ColourLegendIdRef { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("VPN");
            xmlBuilder.WriteXmlAttribute("A", ValuePresentationID);
            xmlBuilder.WriteXmlAttribute<int>("B", Offset);
            xmlBuilder.WriteXmlAttribute<double>("C", Scale);
            xmlBuilder.WriteXmlAttribute<byte>("D", NumberOfDecimals);
            xmlBuilder.WriteXmlAttribute("E", UnitDesignator);
            xmlBuilder.WriteXmlAttribute("F", ColourLegendIdRef);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOValuePresentation ReadXML(XmlNode node)
        {
            ISOValuePresentation presentation = new ISOValuePresentation();
            presentation.ValuePresentationID = node.GetXmlNodeValue("@A");
            presentation.Offset = node.GetXmlNodeValueAsInt("@B");
            presentation.Scale = node.GetXmlNodeValueAsDouble("@C");
            presentation.NumberOfDecimals = byte.Parse(node.GetXmlNodeValue("@D"));
            presentation.UnitDesignator = node.GetXmlNodeValue("@E");
            presentation.ColourLegendIdRef = node.GetXmlNodeValue("@F");
            return presentation;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOValuePresentation> items = new List<ISOValuePresentation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOValuePresentation.ReadXML(node));
            }
            return items;
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.ValuePresentationID, 14, errors, "A");
            RequireRange(this, x => x.Offset, Int32.MinValue, Int32.MaxValue - 1, errors, "B");
            RequireRange(this, x => x.Scale, .000000001d, 100000000d, errors, "C");
            RequireRange<ISOValuePresentation, byte>(this, x => x.NumberOfDecimals, 0, 7, errors, "D");
            ValidateString(this, x => x.UnitDesignator, 32, errors, "E");
            ValidateString(this, x => x.ColourLegendIdRef, 14, errors, "F");
            return errors;
        }
    }
}