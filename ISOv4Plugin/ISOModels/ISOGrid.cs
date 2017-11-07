/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOGrid : ISOElement
    {
        public const string GridTypeProperty = "GridType";

        //Attributes
        public decimal GridMinimumNorthPosition { get; set; }
        public decimal GridMinimumEastPosition { get; set; }
        public double GridCellNorthSize { get; set; }
        public double GridCellEastSize { get; set; }
        public long GridMaximumColumn { get; set; }
        public long GridMaximumRow { get; set; }
        public string Filename { get; set; }
        public long? Filelength { get; set; }
        public byte GridType { get; set; }
        public byte? TreatmentZoneCode { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GRD");
            xmlBuilder.WriteXmlAttribute<decimal>("A", GridMinimumNorthPosition);
            xmlBuilder.WriteXmlAttribute<decimal>("B", GridMinimumEastPosition);
            xmlBuilder.WriteXmlAttribute("C", GridCellNorthSize.ToString("F14"));
            xmlBuilder.WriteXmlAttribute("D", GridCellEastSize.ToString("F14"));
            xmlBuilder.WriteXmlAttribute<long>("E", GridMaximumColumn);
            xmlBuilder.WriteXmlAttribute<long>("F", GridMaximumRow);
            xmlBuilder.WriteXmlAttribute("G", Filename);
            xmlBuilder.WriteXmlAttribute("H", Filelength);
            xmlBuilder.WriteXmlAttribute<byte>("I", GridType);
            xmlBuilder.WriteXmlAttribute("J", TreatmentZoneCode);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOGrid ReadXML(XmlNode node)
        {
            ISOGrid grid = new ISOGrid();
            grid.GridMinimumNorthPosition = node.GetXmlNodeValueAsDecimal("@A");
            grid.GridMinimumEastPosition = node.GetXmlNodeValueAsDecimal("@B");
            grid.GridCellNorthSize = node.GetXmlNodeValueAsDouble("@C");
            grid.GridCellEastSize = node.GetXmlNodeValueAsDouble("@D");
            grid.GridMaximumColumn = node.GetXmlNodeValueAsLong("@E");
            grid.GridMaximumRow = node.GetXmlNodeValueAsLong("@F");
            grid.Filename = node.GetXmlNodeValue("@G");
            grid.Filelength = node.GetXmlNodeValueAsNullableLong("@H");
            grid.GridType = node.GetXmlNodeValueAsByte("@I");
            grid.TreatmentZoneCode = node.GetXmlNodeValueAsNullableByte("@J");
            return grid;
        }

        public static IEnumerable<ISOGrid> ReadXML(XmlNodeList nodes)
        {
            List<ISOGrid> items = new List<ISOGrid>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOGrid.ReadXML(node));
            }
            return items;
        }

        public List<int> GetRatesForGridType1(string dataPath)
        {
            List<int> values = new List<int>();
            string filePath = Path.ChangeExtension(Path.Combine(dataPath, Filename), ".bin");
            using (var fileStream = File.OpenRead(filePath))
            {
                int treatmentZoneId;
                while (true)
                {
                    treatmentZoneId = fileStream.ReadByte();
                    if (treatmentZoneId == -1)
                        break;

                    values.Add(treatmentZoneId);
                }
            }
            return values;

        }

        public List<List<double>> GetRatesForGridType2(string dataPath, ISOTreatmentZone treatmentZone)
        {
            if (treatmentZone == null || treatmentZone.ProcessDataVariables.Count <= 0)
            { 
                return null;
            }

            List<List<double>> productRates = new List<List<double>>();
            Dictionary<string, ISOUnit> unitsByDDI = new Dictionary<string, ISOUnit>();
            string filePath = Path.ChangeExtension(Path.Combine(dataPath, Filename), ".bin");
            using (var fileStream = File.OpenRead(filePath))
            {
                var bytes = new byte[4];
                var rates = new List<double>();
                var rateCount = 0;

                while (true) 
                {
                    var result = fileStream.Read(bytes, 0, bytes.Length);
                    if (result == 0)
                        break;

                    var rate = BitConverter.ToInt32(bytes, 0);

                    ISOProcessDataVariable pdv = treatmentZone.ProcessDataVariables[rateCount];
                    ISOUnit unit = null;
                    if (!unitsByDDI.ContainsKey(pdv.ProcessDataDDI))
                    {
                        unit = UnitFactory.Instance.GetUnitByDDI(pdv.ProcessDataDDI.AsInt32DDI());
                        unitsByDDI.Add(pdv.ProcessDataDDI, unit);
                    }
                    unit = unitsByDDI[pdv.ProcessDataDDI];

                    if (unit != null)
                    {
                        rates.Add(unit.ConvertFromIsoUnit(rate));
                    }
                    else
                    {
                        throw new ApplicationException("Missing unit on rate calculation from PDV.");
                    }
                    rateCount++;

                    if (rateCount == treatmentZone.ProcessDataVariables.Count)
                    {
                        productRates.Add(rates);
                        rateCount = 0;
                        rates = new List<double>();
                    }
                }
            }

            return productRates;
        }
    }
}