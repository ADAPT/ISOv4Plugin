using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public static class LinkListWriter
    {
        public static void Write(string exportPath, Dictionary<string, CompoundIdentifier> ids)
        {
            var linkFile = Path.Combine(exportPath, "LINKLIST.xml");

            using(var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream))
            {
                WriteXml(ids, writer);
                writer.Flush();

                var xml = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(linkFile, xml);
            }
        }

        private static void WriteXml(Dictionary<string, CompoundIdentifier> ids, XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("ISO11783_LinkList");
            writer.WriteAttributeString("VersionMajor", "4");
            writer.WriteAttributeString("VersionMinor", "0");
            writer.WriteAttributeString("ManagementSoftwareManufacturer", "AgGateway");
            writer.WriteAttributeString("ManagementSoftwareVersion", "1.0");
            writer.WriteAttributeString("DataTransferOrigin", ((int) ISO11783_TaskDataDataTransferOrigin.Item1).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("TaskControllerManufacturer", "");
            writer.WriteAttributeString("TaskControllerVersion", "");

            WriteGroups(ids, writer);
            writer.WriteEndElement();
        }

        private static void WriteGroups(Dictionary<string, CompoundIdentifier> ids, XmlWriter writer)
        {
            var allUniqueSources = ids.SelectMany(x => x.Value.UniqueIds.Select(y => y.Source)).Distinct().ToList();

            var currentGroupId = 1;
            foreach (var gln in allUniqueSources)
            {
                if(gln == "http://dictionary.isobus.net/isobus/")
                    return;

                var uuidKvps = ids.Where(x => x.Value.UniqueIds.Any(ui => ui.CiTypeEnum == CompoundIdentifierTypeEnum.UUID));
                WriteGroup(currentGroupId, "1", gln, uuidKvps, writer, true);
                currentGroupId ++;

                var intStringKvps = ids.Where(x => x.Value.UniqueIds.Any(ui => ui.Source == gln && (ui.CiTypeEnum == CompoundIdentifierTypeEnum.LongInt || ui.CiTypeEnum == CompoundIdentifierTypeEnum.String)));
                WriteGroup(currentGroupId, "2", gln, intStringKvps, writer, false);
                currentGroupId++;
            }
        }

        private static void WriteGroup(int groupId, string groupType, string gln, IEnumerable<KeyValuePair<string, CompoundIdentifier>> ids, XmlWriter writer, bool writeUuidIds)
        {
            writer.WriteStartElement("LGP");
            writer.WriteAttributeString("A", "LGP" + groupId);
            writer.WriteAttributeString("B", groupType);
            writer.WriteAttributeString("C", gln);

            WriteLsts(ids, gln, writer, writeUuidIds);
            writer.WriteEndElement();
        }

        private static void WriteLsts(IEnumerable<KeyValuePair<string, CompoundIdentifier>> ids, string gln, XmlWriter writer, bool writeUuidIds)
        {
            foreach (var kvp in ids)
            {
                foreach (var uniqueId in kvp.Value.UniqueIds.Where(i => writeUuidIds == (i.CiTypeEnum == CompoundIdentifierTypeEnum.UUID) && i.Source == gln))
                {
                    writer.WriteStartElement("LNK");
                    writer.WriteAttributeString("A", kvp.Key);
                    writer.WriteAttributeString("B", uniqueId.Id);
                    writer.WriteAttributeString("C", uniqueId.Source);
                    writer.WriteEndElement();
                }
            }
        }
    }
}
