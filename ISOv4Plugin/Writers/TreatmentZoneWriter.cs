using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class TreatmentZoneWriter
    {
        public static string Write(XmlWriter writer, string zoneId, TreatmentZone treatmentZone)
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

            var variableDdi = DetermineVariableDdi(dataVariable);
            writer.WriteAttributeString("A", string.Format(CultureInfo.InvariantCulture, "{0:X4}", variableDdi));
            writer.WriteAttributeString("B", dataVariable.IsoUnit.ConvertToIsoUnit(dataVariable.Value).ToString("F0", CultureInfo.InvariantCulture));
            writer.WriteXmlAttribute("C", dataVariable.ProductId);

            writer.WriteEndElement();
        }

        private static long DetermineVariableDdi(DataVariable dataVariable)
        {
            if (dataVariable.IsoUnit == null)
                return 6;

            var adaptUnit = dataVariable.IsoUnit.ToAdaptUnit();
            if (UnitFactory.DimensionToDdi.ContainsKey(adaptUnit.Dimension))
                return UnitFactory.DimensionToDdi[adaptUnit.Dimension];

            return 6;
        }
    }
}
