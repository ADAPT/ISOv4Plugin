/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using System.IO;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using System.Xml.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOTimeLog : ISOElement
    {
        //Attributes
        public string Filename { get; set; }
        public uint? Filelength { get; set; }
        public byte TimeLogType { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TLG");
            xmlBuilder.WriteXmlAttribute("A", Filename);
            xmlBuilder.WriteXmlAttribute("B", Filelength);
            xmlBuilder.WriteXmlAttribute<byte>("C", TimeLogType);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOTimeLog ReadXML(XmlNode node)
        {
            ISOTimeLog item = new ISOTimeLog();
            item.Filename = node.GetXmlNodeValue("@A");
            item.Filelength = node.GetXmlNodeValueAsNullableUInt("@B");
            item.TimeLogType = node.GetXmlNodeValueAsByte("@C");
            return item;
        }

        public static IEnumerable<ISOTimeLog> ReadXML(XmlNodeList nodes)
        {
            List<ISOTimeLog> items = new List<ISOTimeLog>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOTimeLog.ReadXML(node));
            }
            return items;
        }

        public ISOTime GetTimeElement(string dataPath)
        {
            string filePath = Path.Combine(dataPath, string.Concat(Filename, ".xml"));
            if (File.Exists(filePath))
            {
                XmlDocument document = new XmlDocument();
                document.Load(filePath);

                XmlNode rootNode = document.SelectSingleNode("TIM");
                return ISOTime.ReadXML(rootNode);
            }
            else
            {
                return null;
            }
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.Filename, 8, errors, "A");
            if (Filelength.HasValue) ValidateRange<ISOTimeLog, uint>(this, x => x.Filelength.Value, 0, uint.MaxValue - 2, errors, "B");
            Require(this, x => x.TimeLogType, errors, "C");
            return errors;
        }
    }
}