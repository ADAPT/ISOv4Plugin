/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using NumericRepresentation = AgGateway.ADAPT.ApplicationDataModel.Representations.NumericRepresentation;
using UnitOfMeasure = AgGateway.ADAPT.ApplicationDataModel.Common.UnitOfMeasure;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using System.IO;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IGridMapper
    {
        ISOGrid Export(RasterGridPrescription adaptRx, ISOTreatmentZone type2DefaultTreatmentZone = null, List<byte> type1TreatmentZoneCodes = null);
        RasterGridPrescription Import(ISOTask task, WorkItem workItem);
    }

    public class GridMapper : BaseMapper, IGridMapper
    {
        private PrescriptionMapper _prescriptionMapper;
        public GridMapper(TaskDataMapper taskDataMapper, PrescriptionMapper prescriptionMapper) : base(taskDataMapper, "GRD")
        {
            _prescriptionMapper = prescriptionMapper;
        }

        #region Export

        public ISOGrid Export(RasterGridPrescription adaptRx, ISOTreatmentZone type2DefaultTreatmentZone = null, List<byte> type1TreatmentZoneCodes = null)
        {
            ISOGrid grid = new ISOGrid();

            if (type1TreatmentZoneCodes != null)
            {
                grid.Filename = WriteType1GridFile(type1TreatmentZoneCodes);
                grid.GridType = 1;
            }
            else if (type2DefaultTreatmentZone != null)
            {
                grid.Filename = WriteType2GridFile(adaptRx, type2DefaultTreatmentZone);
                grid.GridType = 2;
                grid.TreatmentZoneCode = type2DefaultTreatmentZone.TreatmentZoneCode;
            }

            FileInfo info = new FileInfo(Path.Combine(this.TaskDataPath, Path.ChangeExtension(grid.Filename,"bin")));
            grid.Filelength = info.Length;

            grid.GridMinimumNorthPosition = Convert.ToDecimal(GetOriginY(adaptRx));
            grid.GridMinimumEastPosition = Convert.ToDecimal(GetOriginX(adaptRx));
            grid.GridCellNorthSize = GetCellHeight(adaptRx);
            grid.GridCellEastSize = GetCellWidth(adaptRx);
            grid.GridMaximumColumn = adaptRx.ColumnCount;
            grid.GridMaximumRow = adaptRx.RowCount;
            return grid;
        }

        private static double GetOriginX(RasterGridPrescription prescription)
        {
            if (prescription.BoundingBox != null && prescription.BoundingBox.MinX != null)
                return prescription.BoundingBox.MinX.Value.Value;
            return prescription.Origin.X;
        }

        private static double GetOriginY(RasterGridPrescription prescription)
        {
            if (prescription.BoundingBox != null && prescription.BoundingBox.MinY != null)
                return prescription.BoundingBox.MinY.Value.Value;
            return prescription.Origin.Y;
        }

        private static double GetCellHeight(RasterGridPrescription prescription)
        {
            var cellHeight = 0.0;
            if (prescription.CellHeight != null)
                cellHeight = prescription.CellHeight.Value.Value;
            if (prescription.BoundingBox != null && prescription.BoundingBox.MinY != null && prescription.BoundingBox.MaxY != null)
            {
                cellHeight = Math.Abs(prescription.BoundingBox.MaxY.Value.Value - prescription.BoundingBox.MinY.Value.Value) / prescription.RowCount;
            }
            return cellHeight;
        }

        private static double GetCellWidth(RasterGridPrescription prescription)
        {
            var cellWidth = 0.0;
            if (prescription.CellWidth != null)
                cellWidth = prescription.CellWidth.Value.Value;
            if (prescription.BoundingBox != null && prescription.BoundingBox.MinX != null && prescription.BoundingBox.MaxX != null)
                cellWidth = Math.Abs(prescription.BoundingBox.MaxX.Value.Value - prescription.BoundingBox.MinX.Value.Value) / prescription.ColumnCount;
            return cellWidth;
        }

        private string WriteType2GridFile(RasterGridPrescription prescription, ISOTreatmentZone treatmentZone)
        {
            var gridFileName = GenerateId(5);
            Dictionary<string, ISOUnit> unitsByDDI = new Dictionary<string, ISOUnit>();
            using (var binaryWriter = CreateWriter(Path.ChangeExtension(gridFileName, ".BIN")))
            {
                byte[] previousBytes = BitConverter.GetBytes(0);
                foreach (var rxRate in prescription.Rates)
                {
                    if (rxRate.RxRate == null || !rxRate.RxRate.Any())
                    {
                        //If there is null or no rate, write the previous rate (or 0 if we have not yet entered a valid rate)
                        binaryWriter.Write(previousBytes, 0, previousBytes.Length);
                    }
                    else
                    {
                        for (int index = 0; index < rxRate.RxRate.Count; index++)
                        {
                            ISOProcessDataVariable pdv = treatmentZone.ProcessDataVariables[index];
                            var rate = rxRate.RxRate[index].Rate;
 
                            ISOUnit unit = null;
                            if (!unitsByDDI.ContainsKey(pdv.ProcessDataDDI))
                            {
                                unit = UnitFactory.Instance.GetUnitByDDI(pdv.ProcessDataDDI.AsInt32DDI());
                                unitsByDDI.Add(pdv.ProcessDataDDI, unit);
                            }
                            unit = unitsByDDI[pdv.ProcessDataDDI];

                            previousBytes = BitConverter.GetBytes((int)Math.Round(unit.ConvertToIsoUnit(rate), 0));
                            binaryWriter.Write(previousBytes, 0, previousBytes.Length);
                        }
                    }
                }
            }

            return gridFileName;
        }

        private string WriteType1GridFile(List<byte> treatmentZoneCodes)
        {
            var gridFileName = GenerateId(5);
            using (var binaryWriter = CreateWriter(Path.ChangeExtension(gridFileName, ".BIN")))
            {
                foreach (byte code in treatmentZoneCodes)
                {
                    binaryWriter.Write(BitConverter.GetBytes(code), 0, 1);
                }
            }
            return gridFileName;
        }


        private FileStream CreateWriter(string fileName)
        {
            Directory.CreateDirectory(base.TaskDataPath);
            return File.OpenWrite(Path.Combine(base.TaskDataPath, fileName));
        }
        #endregion Export 

        #region Import

        public RasterGridPrescription Import(ISOTask task, WorkItem workItem)
        {
            RasterGridPrescription rasterPrescription = new RasterGridPrescription();

            _prescriptionMapper.ImportSharedPrescriptionProperties(task, workItem, rasterPrescription);

            GridDescriptor gridDescriptor = LoadGridDescriptor(task, TaskDataPath);
            if (gridDescriptor != null)
            {
                rasterPrescription.BoundingBox = new BoundingBox();
                rasterPrescription.BoundingBox.MinY = new NumericRepresentationValue(RepresentationInstanceList.vrLatitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), gridDescriptor.Origin.Y));
                rasterPrescription.BoundingBox.MinX = new NumericRepresentationValue(RepresentationInstanceList.vrLongitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), gridDescriptor.Origin.X));
                var maxYValue = rasterPrescription.BoundingBox.MinY.Value.Value + gridDescriptor.CellHeight.Value.Value * gridDescriptor.RowCount;
                var maxXValue = rasterPrescription.BoundingBox.MinX.Value.Value + gridDescriptor.CellWidth.Value.Value * gridDescriptor.ColumnCount;
                rasterPrescription.BoundingBox.MaxY = new NumericRepresentationValue(RepresentationInstanceList.vrLatitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), maxYValue));
                rasterPrescription.BoundingBox.MaxX = new NumericRepresentationValue(RepresentationInstanceList.vrLongitude.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("arcdeg"), maxXValue));
                rasterPrescription.Origin = gridDescriptor.Origin;
                rasterPrescription.CellHeight = gridDescriptor.CellHeight;
                rasterPrescription.CellWidth = gridDescriptor.CellWidth;
                rasterPrescription.ColumnCount = gridDescriptor.ColumnCount;
                rasterPrescription.RowCount = gridDescriptor.RowCount;

                ImportRates(task, gridDescriptor, rasterPrescription);
            }
            return rasterPrescription;
        }

        private GridDescriptor LoadGridDescriptor(ISOTask task, string dataPath)
        {
            if (task.Grid == null)
            {
                return null;
            }

            GridDescriptor descriptor = new GridDescriptor();
            if (!descriptor.LoadGridDefinition(task.Grid))
            {
                return null;
            }

            ISOTreatmentZone treatmentZone = null;
            if (task.Grid.GridType == 2)
            {
                treatmentZone = task.TreatmentZones.SingleOrDefault(tz => tz.TreatmentZoneCode == task.Grid.TreatmentZoneCode);
                if (treatmentZone == null)
                {
                    return null;
                }
            }
            if (!descriptor.LoadRates(dataPath, task.Grid, treatmentZone)) 
            {
                return null;
            }

            return descriptor;
        }

        private void ImportRates(ISOTask task, GridDescriptor gridDescriptor, RasterGridPrescription prescription)
        {
            if (task.PositionLostTreatmentZone != null)
            {
                prescription.LossOfGpsRate = ImportTreatmentZoneAsNumericRepValue(task.PositionLostTreatmentZone);
            }
            if (task.OutOfFieldTreatmentZone != null)
            {
                prescription.OutOfFieldRate = ImportTreatmentZoneAsNumericRepValue(task.OutOfFieldTreatmentZone);
            }

            if (gridDescriptor.TreatmentZoneCodes != null)
            {
                //Grid Type 1
                ISOTreatmentZone treatmentZone = task.TreatmentZones.FirstOrDefault();
                if (treatmentZone == null)
                    return;

                prescription.Rates = ImportRatesFromTreatmentZones(gridDescriptor, task.TreatmentZones, prescription.ProductIds, prescription);
            }
            else if (gridDescriptor.ProductRates != null)
            {
                //Grid Type 2
                var treatmentZoneTemplate = task.DefaultTreatmentZone;
                if (treatmentZoneTemplate == null)
                    return;

                prescription.Rates = ImportRatesFromProducts(gridDescriptor, prescription.ProductIds, prescription);
            }
        }

        private List<RxRates> ImportRatesFromProducts(GridDescriptor gridDescriptor, List<int> productIds, RasterGridPrescription prescription)
        {
            var rates = new List<RxRates>();
            foreach (var productRates in gridDescriptor.ProductRates)
            {
                var rate = new RxRates { RxRate = new List<RxRate>() };
                for (int productIndex = 0; productIndex < productRates.Count; productIndex++)
                {
                    int adaptProductId = productIds[productIndex];
                    rate.RxRate.Add(PrescriptionMapper.ImportRate(adaptProductId, productRates[productIndex], prescription));
                }
                rates.Add(rate);
            }

            return rates;
        }

        private List<RxRates> ImportRatesFromTreatmentZones(GridDescriptor gridDescriptor, IEnumerable<ISOTreatmentZone> treatmentZones, List<int> productIds, RasterGridPrescription prescription)
        {
            var rates = new List<RxRates>();
            foreach (var treatmentZoneCode in gridDescriptor.TreatmentZoneCodes)
            {
                ISOTreatmentZone treatmentZone = treatmentZones.FirstOrDefault(t => t.TreatmentZoneCode == treatmentZoneCode);
                if (treatmentZone == null)
                {
                    return null;
                }

                var rate = new RxRates { RxRate = new List<RxRate>() };
                foreach (ISOProcessDataVariable pdv in treatmentZone.ProcessDataVariables)
                {
                    if (!string.IsNullOrEmpty(pdv.ProductIdRef))
                    {
                        int? productID = TaskDataMapper.ADAPTIdMap.FindByISOId(pdv.ProductIdRef);
                        if (productID.HasValue)
                        {
                            rate.RxRate.Add(PrescriptionMapper.ImportRate(productID.Value, pdv.ProcessDataValue, prescription));
                        }
                    }
                }

                rates.Add(rate);
            }

            return rates;
        }

        private NumericRepresentationValue ImportTreatmentZoneAsNumericRepValue(ISOTreatmentZone treatmentZone)
        {
            if (treatmentZone.ProcessDataVariables == null || treatmentZone.ProcessDataVariables.Count == 0)
            {
                return null;
            }

            return treatmentZone.ProcessDataVariables.First().AsNumericRepresentationValue(RepresentationMapper, ISOTaskData); //In this situation, there should be only one PDV
        }
        #endregion Import
    }
}
