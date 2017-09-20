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
    public class ISOLink : ISOElement
    {
        //Attributes
        public string ObjectIdRef { get; set; }
        public string LinkValue { get; set; }
        public string LinkDesignator  { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("LNK");
            xmlBuilder.WriteXmlAttribute("A", ObjectIdRef);
            xmlBuilder.WriteXmlAttribute("B", LinkValue);
            xmlBuilder.WriteXmlAttribute("C", LinkDesignator);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOLink ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISOLink item = new ISOLink();
            item.ObjectIdRef = node.GetXmlNodeValue("@A");
            item.LinkValue = node.GetXmlNodeValue("@B");
            item.LinkDesignator  = node.GetXmlNodeValue("@C");
            return item;
        }

        public static List<ISOLink> ReadXML(XmlNodeList nodes)
        {
            List<ISOLink> items = new List<ISOLink>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOLink.ReadXML(node));
            }
            return items;
        }
    }
}