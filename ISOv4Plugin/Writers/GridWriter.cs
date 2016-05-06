using System;
using System.Globalization;
using System.IO;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class GridWriter : BaseWriter
    {
        private ADAPT.Representation.UnitSystem.UnitOfMeasureConverter _unitConverter;

        public GridWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "GRD", 0)
        {
            _unitConverter = new ADAPT.Representation.UnitSystem.UnitOfMeasureConverter();
        }

        public void Write(XmlWriter writer, RasterGridPrescription prescription, TreatmentZone treatmentZone)
        {
            writer.WriteStartElement(XmlPrefix);

            WriteGridDefinition(writer, prescription);

            var gridFileName = WriteGridFile(prescription, treatmentZone);
            writer.WriteAttributeString("G", gridFileName);
            writer.WriteAttributeString("I", "2");
            writer.WriteAttributeString("J", "1");

            writer.WriteEndElement();
        }

        private static void WriteGridDefinition(XmlWriter writer, RasterGridPrescription prescription)
        {
            writer.WriteAttributeString("A", prescription.Origin.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("B", prescription.Origin.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("C", prescription.CellHeight.Value.Value.ToString("F14", CultureInfo.InvariantCulture));
            writer.WriteAttributeString("D", prescription.CellWidth.Value.Value.ToString("F14", CultureInfo.InvariantCulture));
            writer.WriteAttributeString("E", prescription.ColumnCount.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("F", prescription.RowCount.ToString(CultureInfo.InvariantCulture));
        }

        private string WriteGridFile(RasterGridPrescription prescription, TreatmentZone treatmentZone)
        {
            var gridFileName = GenerateId(5);
            using (var binaryWriter = CreateWriter(Path.ChangeExtension(gridFileName, ".BIN")))
            {
                foreach (var rxRate in prescription.Rates)
                {
                    for (int index = 0; index < rxRate.RxRate.Count; index++)
                    {
                        var dataVariable = treatmentZone.Variables[index];
                        var rate = rxRate.RxRate[index].Rate;
                        if (dataVariable.UserUnit != null)
                            rate = _unitConverter.Convert(dataVariable.UserUnit.ToInternalUom(), dataVariable.IsoUnit.ToAdaptUnit().ToInternalUom(), rate);

                        var bytes = BitConverter.GetBytes((int)Math.Round(dataVariable.IsoUnit.ConvertToIsoUnit(rate), 0));
                        binaryWriter.Write(bytes, 0, bytes.Length);
                    }
                }
            }

            return gridFileName;
        }

        private FileStream CreateWriter(string fileName)
        {
            return File.OpenWrite(Path.Combine(TaskWriter.BaseFolder, fileName));
        }
    }
}