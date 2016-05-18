using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public static class LinkListWriter
    {
        public static void Write(string exportPath, Dictionary<string, CompoundIdentifier> ids)
        {
            var linkFile = Path.Combine(exportPath, "TASKDATA", "LINKLIST.xml");

            using(var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("ISO11783_LinkList");
                writer.WriteAttributeString("VersionMajor", "4");
                writer.WriteAttributeString("VersionMinor", "0");
                writer.WriteAttributeString("ManagementSoftwareManufacturer", "AgGateway");
                writer.WriteAttributeString("ManagementSoftwareVersion", "1.0");
                writer.WriteAttributeString("DataTransferOrigin", ((int)ISO11783_TaskDataDataTransferOrigin.Item1).ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("TaskControllerManufacturer", "");
                writer.WriteAttributeString("TaskControllerVersion", "");

                writer.WriteStartElement("LGP");
                writer.WriteAttributeString("A", "LGP1");
                writer.WriteAttributeString("B", "1");

                foreach (var kvp in ids)
                {
                    foreach (var uniqueId in kvp.Value.UniqueIds)
                    {
                        writer.WriteStartElement("LST");
                        writer.WriteAttributeString("A", kvp.Key);
                        writer.WriteAttributeString("B", uniqueId.Id);
                        writer.WriteAttributeString("C", uniqueId.Source);
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.Flush();

                var xml = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(linkFile, xml);
            }
        }
    }



    public class LinkGroup
    {
        public string Id { get; set; }
        public List<Link> Links { get; set; }
    }

    public class Link
    {
        public string ReferenceId { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
