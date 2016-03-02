using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.IsoPlugin.Writers
{
    internal class TreatmentZoneWriter
    {
        internal static string Write(XmlWriter writer, string zoneId, TreatmentZone treatmentZone)
        {
            if (treatmentZone == null)
                return null;

            writer.WriteStartElement("TZN");
            writer.WriteAttributeString("A", zoneId);
            writer.WriteXmlAttribute("B", treatmentZone.Name);

            if (treatmentZone.Variables != null)
            {
                foreach (var dataVariable in treatmentZone.Variables)
                {
                    WriteDataVariable(writer, dataVariable);
                }
            }

            writer.WriteEndElement();
            return zoneId;
        }

        private static void WriteDataVariable(XmlWriter writer, DataVariable dataVariable)
        {
            writer.WriteStartElement("PDV");
            writer.WriteAttributeString("A", string.Format(CultureInfo.InvariantCulture, "{0:X4}", 6));
            writer.WriteAttributeString("B", dataVariable.Value.Value.Value.ToString("F0", CultureInfo.InvariantCulture));
            writer.WriteXmlAttribute("C", dataVariable.ProductId);
            writer.WriteEndElement();
        }
    }
}
