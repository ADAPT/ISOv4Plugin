/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOAttachedFile : ISOElement
    {
        //Attributes
        public string FilenamewithExtension { get; set; }
        public ISOAttachedFilePreserve Preserve { get { return (ISOAttachedFilePreserve)PreserveInt; } set { PreserveInt = (int)value; } }
        private int PreserveInt { get; set; }
        public string ManufacturerGLN  { get; set; }
        public byte FileType { get; set; }
        public string FileVersion { get; set; }
        public uint? FileLength { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("AFE");
            xmlBuilder.WriteXmlAttribute("A", FilenamewithExtension);
            xmlBuilder.WriteXmlAttribute("B", ((int)Preserve).ToString());
            xmlBuilder.WriteAttributeString("C", ManufacturerGLN); //Override suppression of empty strings
            xmlBuilder.WriteXmlAttribute<byte>("D", FileType);
            xmlBuilder.WriteXmlAttribute("E", FileVersion);
            xmlBuilder.WriteXmlAttribute<uint>("F", FileLength);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOAttachedFile ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISOAttachedFile item = new ISOAttachedFile();
            item.FilenamewithExtension = node.GetXmlNodeValue("@A");
            item.PreserveInt = node.GetXmlNodeValueAsInt("@B");
            item.ManufacturerGLN  = node.GetXmlNodeValue("@C");
            item.FileType = node.GetXmlNodeValueAsByte("@D");
            item.FileVersion = node.GetXmlNodeValue("@E");
            item.FileLength = node.GetXmlNodeValueAsNullableUInt("@F");
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

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.FilenamewithExtension, 12, errors, "A");
            ValidateEnumerationValue(typeof(ISOAttachedFilePreserve), PreserveInt, errors);
            RequireString(this, x => x.ManufacturerGLN, 32, errors, "C");
            RequireRange<ISOAttachedFile, byte>(this, x => x.FileType, 1, 254, errors, "D");
            ValidateString(this, x => x.FileVersion, 32, errors, "E");
            if (FileLength.HasValue) ValidateRange<ISOAttachedFile, uint>(this, x => x.FileLength.Value, 0, UInt32.MaxValue - 2, errors, "F");
            return errors;
        }
    }
}