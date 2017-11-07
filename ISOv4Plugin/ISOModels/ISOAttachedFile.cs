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
    public class ISOAttachedFile : ISOElement
    {
        //Attributes
        public string FilenamewithExtension { get; set; }
        public ISOAttachedFilePreserve Preserve { get; set; }
        public string ManufacturerGLN  { get; set; }
        public byte FileType { get; set; }
        public string FileVersion { get; set; }
        public long? FileLength { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("AFE");
            xmlBuilder.WriteXmlAttribute("A", FilenamewithExtension);
            xmlBuilder.WriteXmlAttribute("B", ((int)Preserve).ToString());
            xmlBuilder.WriteAttributeString("C", ManufacturerGLN); //Override suppression of empty strings
            xmlBuilder.WriteXmlAttribute<byte>("D", FileType);
            xmlBuilder.WriteXmlAttribute("E", FileVersion);
            xmlBuilder.WriteXmlAttribute<long>("F", FileLength);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOAttachedFile ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISOAttachedFile item = new ISOAttachedFile();
            item.FilenamewithExtension = node.GetXmlNodeValue("@A");
            item.Preserve = (ISOAttachedFilePreserve)(node.GetXmlNodeValueAsInt("@B"));
            item.ManufacturerGLN  = node.GetXmlNodeValue("@C");
            item.FileType = node.GetXmlNodeValueAsByte("@D");
            item.FileVersion = node.GetXmlNodeValue("@E");
            item.FileLength = node.GetXmlNodeValueAsNullableLong("@F");
            return item;
        }

        public static List<ISOAttachedFile> ReadXML(XmlNodeList nodes)
        {
            List<ISOAttachedFile> items = new List<ISOAttachedFile>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOAttachedFile.ReadXML(node));
            }
            return items;
        }
    }
}