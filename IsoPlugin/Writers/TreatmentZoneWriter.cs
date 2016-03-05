using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.IsoPlugin.Writers
{
    internal class TreatmentZoneWriter
    {
        private static readonly Dictionary<UnitOfMeasureDimensionEnum, long> _dimensionToDdi =
            new Dictionary<UnitOfMeasureDimensionEnum, long>()
            {
                { UnitOfMeasureDimensionEnum.VolumePerArea, 1 },
                { UnitOfMeasureDimensionEnum.MassPerArea, 6 },
                { UnitOfMeasureDimensionEnum.CountPerArea, 11 },
                { UnitOfMeasureDimensionEnum.VolumePerVolume, 21 },
                { UnitOfMeasureDimensionEnum.MassPerMass, 26 },
                { UnitOfMeasureDimensionEnum.VolumePerMass, 31 },
                { UnitOfMeasureDimensionEnum.VolumePerTime, 36 },
                { UnitOfMeasureDimensionEnum.MassPerTime, 41 },
                { UnitOfMeasureDimensionEnum.PerTime, 46 },
                { UnitOfMeasureDimensionEnum.Mass, 74 },
                { UnitOfMeasureDimensionEnum.Count, 77 }
            };

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
            if (_dimensionToDdi.ContainsKey(adaptUnit.Dimension))
                return _dimensionToDdi[adaptUnit.Dimension];

            return 6;
        }
    }
}
