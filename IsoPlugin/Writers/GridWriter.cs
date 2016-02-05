using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal class GridWriter : BaseWriter
    {
        internal GridWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "GRD", 0)
        {
        }

        internal void Write(XmlWriter writer, RasterGridPrescription prescription)
        {
            writer.WriteStartElement(XmlPrefix);

            WriteGridDefinition(writer, prescription);

            var gridFileName = WriteGridFile(prescription);
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

        private string WriteGridFile(RasterGridPrescription prescription)
        {
            var gridFileName = GenerateId(5);
            using (var binaryWriter = CreateWriter(Path.ChangeExtension(gridFileName, ".BIN")))
            {
                foreach (var rate in prescription.Rates)
                {
                    foreach (var productRate in rate.RxRate)
                    {
                        var bytes = BitConverter.GetBytes((int)productRate.Rate);
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