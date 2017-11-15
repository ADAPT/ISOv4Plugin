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
    public class ISODeviceValuePresentation : ISOElement
    {
        //Attributes
        public int ObjectID { get; set; }
        public long Offset  { get; set; }
        public double Scale  { get; set; }
        public byte NumberOfDecimals { get; set; }
        public string UnitDesignator { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DVP");
            xmlBuilder.WriteXmlAttribute<int>("A", ObjectID);
            xmlBuilder.WriteXmlAttribute<long>("B", Offset );
            xmlBuilder.WriteXmlAttribute<double>("C", Scale);
            xmlBuilder.WriteXmlAttribute<byte>("D", NumberOfDecimals);
            xmlBuilder.WriteXmlAttribute("E", UnitDesignator);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceValuePresentation ReadXML(XmlNode node)
        {
            ISODeviceValuePresentation item = new ISODeviceValuePresentation();
            item.ObjectID = node.GetXmlNodeValueAsInt("@A");
            item.Offset = node.GetXmlNodeValueAsLong("@B");
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
    }
}