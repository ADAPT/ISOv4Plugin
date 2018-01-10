/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceValuePresentation : ISOElement
    {
        //Attributes
        public uint ObjectID { get; set; }
        public int Offset  { get; set; }
        public double Scale  { get; set; }
        public byte NumberOfDecimals { get; set; }
        public string UnitDesignator { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DVP");
            xmlBuilder.WriteXmlAttribute<uint>("A", ObjectID);
            xmlBuilder.WriteXmlAttribute<int>("B", Offset );
            xmlBuilder.WriteXmlAttribute<double>("C", Scale);
            xmlBuilder.WriteXmlAttribute<byte>("D", NumberOfDecimals);
            xmlBuilder.WriteXmlAttribute("E", UnitDesignator);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceValuePresentation ReadXML(XmlNode node)
        {
            ISODeviceValuePresentation item = new ISODeviceValuePresentation();
            item.ObjectID = node.GetXmlNodeValueAsUInt("@A");
            item.Offset = node.GetXmlNodeValueAsInt("@B");
            item.Scale = node.GetXmlNodeValueAsDouble("@C");
            item.NumberOfDecimals = node.GetXmlNodeValueAsByte("@D");
            item.UnitDesignator = node.GetXmlNodeValue("@E");

            return item;
        }

        public static IEnumerable<ISODeviceValuePresentation> ReadXML(XmlNodeList nodes)
        {
            List<ISODeviceValuePresentation> items = new List<ISODeviceValuePresentation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceValuePresentation.ReadXML(node));
            }
            return items;
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireRange<ISODeviceValuePresentation, uint>(this, x => x.ObjectID, 1, 65534, errors, "A");
            RequireRange(this, x => x.Offset, Int32.MinValue, Int32.MaxValue-1, errors, "B");
            RequireRange(this, x => x.Scale, .000000001d, 100000000d, errors, "C");
            RequireRange<ISODeviceValuePresentation, byte>(this, x => x.NumberOfDecimals, 0, 7, errors, "D");
            ValidateString(this, x => x.UnitDesignator, 32, errors, "E");
            return errors;
        }
    }
}