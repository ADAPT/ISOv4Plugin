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
    public class ISOLinkGroup : ISOElement
    {
        public ISOLinkGroup()
        {
            Links = new List<ISOLink>();
        }

        //Attributes
        public string LinkGroupId  { get; set; }
        public ISOLinkGroupType LinkGroupType { get; set; }
        public string ManufacturerGLN  { get; set; }
        public string LinkGroupNamespace  { get; set; }
        public string LinkGroupDesignator  { get; set; }

        //Child Elements
        public List<ISOLink> Links { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("LGP");
            xmlBuilder.WriteXmlAttribute("A", LinkGroupId );
            xmlBuilder.WriteXmlAttribute("B", ((int)LinkGroupType).ToString());
            xmlBuilder.WriteXmlAttribute("C", ManufacturerGLN);
            xmlBuilder.WriteXmlAttribute("D", LinkGroupNamespace);
            xmlBuilder.WriteXmlAttribute("E", LinkGroupDesignator);
            foreach (ISOLink item in Links) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOLinkGroup ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISOLinkGroup item = new ISOLinkGroup();
            item.LinkGroupId  = node.GetXmlNodeValue("@A");
            item.LinkGroupType = (ISOLinkGroupType)(Int32.Parse(node.GetXmlNodeValue("@B")));
            item.ManufacturerGLN  = node.GetXmlNodeValue("@C");
            item.LinkGroupNamespace  = node.GetXmlNodeValue("@D");
            item.LinkGroupDesignator  = node.GetXmlNodeValue("@E");

            XmlNodeList lnkNodes = node.SelectNodes("LNK");
            if (lnkNodes != null)
            {
                item.Links.AddRange(ISOLink.ReadXML(lnkNodes));
            }

            return item;
        }

        public static List<ISOLinkGroup> ReadXML(XmlNodeList nodes)
        {
            List<ISOLinkGroup> items = new List<ISOLinkGroup>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOLinkGroup.ReadXML(node));
            }
            return items;
        }
    }
}