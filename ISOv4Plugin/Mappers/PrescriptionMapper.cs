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
using AgGateway.ADAPT.ApplicationDataModel.Equipment;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IPrecriptionMapper
    {
        ISOTask ExportPrescription(WorkItem workItem, int gridType, Prescription prescription);
        Prescription ImportPrescription(ISOTask task);
    }

    public class PrescriptionMapper : BaseMapper
    {
        private ADAPT.Representation.UnitSystem.UnitOfMeasureConverter _unitConverter;
        private GridMapper _gridMapper;
        private ConnectionMapper _connectionMapper;
        public PrescriptionMapper(TaskDataMapper taskDataMapper, ConnectionMapper connectionMapper)
            :base (taskDataMapper, "TSK")
        {
            _unitConverter = new ADAPT.Representation.UnitSystem.UnitOfMeasureConverter();
            _gridMapper = new GridMapper(taskDataMapper, this);
            _connectionMapper = connectionMapper;
        }

        #region Export
        public ISOTask ExportPrescription(WorkItem workItem, int gridType, Prescription prescription)
        {
            ISOTask task = new ISOTask();

            //Task ID
            string taskID = workItem.Id.FindIsoId() ?? GenerateId();
            task.TaskID = taskID;
            ExportIDs(workItem.Id, taskID);

            //Designator
            task.TaskDesignator = prescription.Description;

            //Customer Ref
            if (workItem.GrowerId.HasValue)
            {
                task.CustomerIdRef = TaskDataMapper.InstanceIDMap.GetISOID(workItem.GrowerId.Value);
            }

            //Farm Ref
            if (workItem.FarmId.HasValue)
            {
                task.FarmIdRef = TaskDataMapper.InstanceIDMap.GetISOID(workItem.FarmId.Value);
            }

            //Partfield Ref
            if (workItem.CropZoneId.HasValue)
            {
                task.PartFieldIdRef = TaskDataMapper.InstanceIDMap.GetISOID(workItem.CropZoneId.Value);
            }
            else if (workItem.FieldId.HasValue)
            {
                task.PartFieldIdRef = TaskDataMapper.InstanceIDMap.GetISOID(workItem.FieldId.Value);
            }

            //Comments
            if (workItem.Notes.Any())
            {
                CommentAllocationMapper canMapper = new CommentAllocationMapper(TaskDataMapper);
                task.CommentAllocations = canMapper.ExportCommentAllocations(workItem.Notes).ToList();
            }

            //Worker Allocations
            if (workItem.PeopleRoleIds.Any())
            {
                WorkerAllocationMapper workerAllocationMapper = new WorkerAllocationMapper(TaskDataMapper);
                List<PersonRole> personRoles = new List<PersonRole>();
                foreach (int id in workItem.PeopleRoleIds)
                {
                    PersonRole personRole = DataModel.Catalog.PersonRoles.FirstOrDefault(p => p.Id.ReferenceId == id);
                    if (personRole != null)
                    {
                        personRoles.Add(personRole);
                    }
                }
                task.WorkerAllocations = workerAllocationMapper.ExportWorkerAllocations(personRoles).ToList();
            }

            //Guidance Allocations
            if (workItem.GuidanceAllocationIds.Any())
            {
                GuidanceAllocationMapper guidanceAllocationMapper = new GuidanceAllocationMapper(TaskDataMapper);
                List<GuidanceAllocation> allocations = new List<GuidanceAllocation>();
                foreach (int id in workItem.GuidanceAllocationIds)
                {
                    GuidanceAllocation allocation = DataModel.Documents.GuidanceAllocations.FirstOrDefault(p => p.Id.ReferenceId == id);
                    if (allocation != null)
                    {
                        allocations.Add(allocation);
                    }
                }
                task.GuidanceAllocations = guidanceAllocationMapper.ExportGuidanceAllocations(allocations).ToList();
            }

            //Connections
            if (workItem.EquipmentConfigurationGroup != null)
            {
                task.Connections = _connectionMapper.ExportConnections(task, workItem.EquipmentConfigurationGroup.EquipmentConfigurations).ToList();
            }

            //Status
            if (workItem.StatusUpdates == null || workItem.StatusUpdates.Count == 0)
            {
                task.TaskStatus = ISOEnumerations.ISOTaskStatus.Planned;
            }
            else if (workItem.StatusUpdates.Count == 1)
            {
                StatusUpdate lastStatus = workItem.StatusUpdates.OrderByDescending(su => su.TimeStamp).Last();
                task.TaskStatus = ExportStatus(lastStatus.Status);
            }

            //Prescription
            if (prescription is RasterGridPrescription)
            {
                ExportRasterPrescription(task, prescription as RasterGridPrescription, gridType);
            }
            else if (prescription is VectorPrescription)
            {
                ExportVectorPrescription(task, prescription as VectorPrescription);
            }
            else if (prescription is ManualPrescription)
            {
                ExportManualPresciption(task, prescription as ManualPrescription);
            }

            return task;
        }

        private void ExportRasterPrescription(ISOTask task, RasterGridPrescription rx, int gridType)
        {
            if (gridType == 1)
            {
                List<byte> treatmentZoneCodes = ExportTreatmentZonesForType1(task, rx);
                task.Grid = _gridMapper.Export(rx, null, treatmentZoneCodes);
            }
            else
            {
                ISOTreatmentZone defaultTreatmentZone = ExportTreatmentZonesForType2(task, rx);
                task.Grid = _gridMapper.Export(rx, defaultTreatmentZone);
            }
        }

        private void ExportVectorPrescription(ISOTask task, VectorPrescription rx)
        {
            byte i = 0;
            foreach (RxShapeLookup shapeLookup in rx.RxShapeLookups)
            {
                ISOTreatmentZone tzn = new ISOTreatmentZone();
                tzn.TreatmentZoneCode = i++;
                foreach (RxRate rxRate in shapeLookup.Rates)
                {
                    tzn.ProcessDataVariables.Add(ExportProcessDataVariable(rxRate, rx));
                }

                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                tzn.Polygons = polygonMapper.ExportPolygons(shapeLookup.Shape.Polygons, ISOEnumerations.ISOPolygonType.TreatmentZone).ToList();
                task.TreatmentZones.Add(tzn);
            }         
        }

        private void ExportManualPresciption(ISOTask task, ManualPrescription rx)
        {
            ISOTreatmentZone tzn = new ISOTreatmentZone();
            tzn.TreatmentZoneDesignator = "Default Treatment Zone";
            tzn.TreatmentZoneCode = 1;
            task.DefaultTreatmentZoneCode = tzn.TreatmentZoneCode;

            foreach (ProductUse productUse in rx.ProductUses)
            {
                var isoUnit = DetermineIsoUnit(rx.RxProductLookups.First(p => p.ProductId == productUse.ProductId).UnitOfMeasure);
                string productIDRef = TaskDataMapper.InstanceIDMap.GetISOID(productUse.ProductId);
                ISOProcessDataVariable pdv = ExportProcessDataVariable(productUse.Rate, productIDRef, isoUnit);
                tzn.ProcessDataVariables.Add(pdv);
            }

            task.TreatmentZones.Add(tzn);
        }

        private List<byte> ExportTreatmentZonesForType1(ISOTask task, RasterGridPrescription prescription)
        {
            Dictionary<string, ISOTreatmentZone> treatmentZones = new Dictionary<string, ISOTreatmentZone>();
            List<byte> rateCodePerCell = new List<byte>();

            byte tznCounter = 1;
            foreach (RxCellLookup cellRates in prescription.Rates)
            {
                string key = GetRxRatesKey(cellRates);
                if (!treatmentZones.ContainsKey(key))
                {
                    ISOTreatmentZone tzn = GetNewType1TreatmentZone(cellRates, tznCounter, prescription);
                    treatmentZones.Add(key, tzn);
                    task.TreatmentZones.Add(tzn);
                    tznCounter++;
                }
                rateCodePerCell.Add(treatmentZones[key].TreatmentZoneCode);
            }
            return rateCodePerCell;

        }

        //Determines a unique key that describes the products and rates assigned to each cell.
        private string GetRxRatesKey(RxCellLookup cellLookup)
        {
            string key = string.Empty;
            cellLookup.RxRates.ForEach(r => key += $"{r.RxProductLookupId}:{r.Rate}|");
            return key;
        }

        //Adds a treatment zone for a new rate combination
        private ISOTreatmentZone GetNewType1TreatmentZone(RxCellLookup cellLookup, byte counter, Prescription rx)
        {
            ISOTreatmentZone treatmentZone = new ISOTreatmentZone() { TreatmentZoneCode = counter, TreatmentZoneDesignator = $"TreatmentZone {counter.ToString()}" };

            foreach (RxRate rate in cellLookup.RxRates)
            {
                treatmentZone.ProcessDataVariables.Add(ExportProcessDataVariable(rate, rx));
            }
            return treatmentZone;
        }

        private ISOTreatmentZone ExportTreatmentZonesForType2(ISOTask task, RasterGridPrescription prescription)
        {
            if (prescription.ProductIds == null)
            {
                TaskDataMapper.AddError($"No Products are present for Grid Type 2 Prescription export: {prescription.Description}", prescription.Id.ReferenceId.ToString());
                return null;
            }

            var lossOfSignalTreatmentZone = new ISOTreatmentZone { TreatmentZoneDesignator = "Loss of GPS", ProcessDataVariables = new List<ISOProcessDataVariable>() };
            var outOfFieldTreatmentZone = new ISOTreatmentZone { TreatmentZoneDesignator = "Out of Field", ProcessDataVariables = new List<ISOProcessDataVariable>() };
            var defaultTreatmentZone = new ISOTreatmentZone { TreatmentZoneDesignator = "Default", ProcessDataVariables = new List<ISOProcessDataVariable>() };

            foreach (var productId in prescription.ProductIds)
            {
                var isoUnit = DetermineIsoUnit(prescription.RxProductLookups.First(p => p.ProductId == productId).UnitOfMeasure);

                string isoProductId = TaskDataMapper.InstanceIDMap.GetISOID(productId) ?? string.Empty;
                RxProductLookup productLookup = prescription.RxProductLookups.FirstOrDefault(p => p.ProductId == productId);
                ISOProcessDataVariable lossPDV = ExportProcessDataVariable(productLookup?.LossOfGpsRate ?? prescription.LossOfGpsRate, isoProductId, isoUnit);
                if (lossPDV != null)
                {
                    lossOfSignalTreatmentZone.ProcessDataVariables.Add(lossPDV);
                }
                ISOProcessDataVariable oofPDV = ExportProcessDataVariable(productLookup?.OutOfFieldRate ?? prescription.OutOfFieldRate, isoProductId, isoUnit);
                if (oofPDV != null)
                {
                    outOfFieldTreatmentZone.ProcessDataVariables.Add(oofPDV);
                }
                ISOProcessDataVariable defaultPDV = ExportProcessDataVariable(productLookup?.LossOfGpsRate ?? prescription.LossOfGpsRate, isoProductId, isoUnit);  //ADAPT doesn't have a separate Default Rate.  Using Loss of GPS Rate as a logical equivalent for a default rate.
                if (defaultPDV == null)
                {
                    //Add 0 as the default rate so that we have at least one PDV to reference
                    var defaultRate = new NumericRepresentationValue(null, new NumericValue(prescription.RxProductLookups.First().UnitOfMeasure, 0));
                    defaultPDV = ExportProcessDataVariable(defaultRate, isoProductId, isoUnit);
                }
                defaultTreatmentZone.ProcessDataVariables.Add(defaultPDV);
            }

            if (lossOfSignalTreatmentZone.ProcessDataVariables.Count > 0)
            {
                lossOfSignalTreatmentZone.TreatmentZoneCode = 253;
                task.TreatmentZones.Add(lossOfSignalTreatmentZone);
                task.PositionLostTreatmentZoneCode = lossOfSignalTreatmentZone.TreatmentZoneCode;
            }

            if (outOfFieldTreatmentZone.ProcessDataVariables.Count > 0)
            {
                outOfFieldTreatmentZone.TreatmentZoneCode = 254;
                task.TreatmentZones.Add(outOfFieldTreatmentZone);
                task.OutOfFieldTreatmentZoneCode = outOfFieldTreatmentZone.TreatmentZoneCode;
            }

            defaultTreatmentZone.TreatmentZoneCode = 1;
            task.TreatmentZones.Add(defaultTreatmentZone);
            task.DefaultTreatmentZoneCode = defaultTreatmentZone.TreatmentZoneCode;

            return defaultTreatmentZone;
        }

        private static ISOUnit DetermineIsoUnit(UnitOfMeasure rateUnit)
        {
            if (rateUnit == null)
                return null;

            return UnitFactory.Instance.GetUnitByDimension(rateUnit.Dimension);
        }

        private ISOProcessDataVariable ExportProcessDataVariable(RxRate rxRate, Prescription rx)
        {
            ISOProcessDataVariable processDataVariable = new ISOProcessDataVariable();
            RxProductLookup lookup = rx.RxProductLookups.FirstOrDefault(l => l.Id.ReferenceId == rxRate.RxProductLookupId);
            if (lookup != null)
            {
                processDataVariable.ProductIdRef = TaskDataMapper.InstanceIDMap.GetISOID(lookup.ProductId.Value);
                processDataVariable.ProcessDataDDI = DetermineVariableDDI(lookup.Representation, lookup.UnitOfMeasure).AsHexDDI();
                ISOUnit unit = UnitFactory.Instance.GetUnitByDDI(processDataVariable.ProcessDataDDI.AsInt32DDI());
                if (unit != null)
                {
                    processDataVariable.ProcessDataValue = (int)unit.ConvertToIsoUnit(rxRate.Rate);
                }
                else
                {
                    throw new ApplicationException("Missing unit on rate calculation from PDV.");
                }
            }
            return processDataVariable;
        }

        private ISOProcessDataVariable ExportProcessDataVariable(NumericRepresentationValue value, string isoProductIdRef, ISOUnit unit)
        {
            if (value != null && value.Value != null)
            {
                UnitOfMeasure adaptUnit = unit.ToAdaptUnit();
                var dataVariable = new ISOProcessDataVariable
                {
                    ProductIdRef = isoProductIdRef,
                    ProcessDataValue = value.AsIntViaMappedDDI(RepresentationMapper),
                    ProcessDataDDI = DetermineVariableDDI(value.Representation, adaptUnit).AsHexDDI()
                };

                return dataVariable;
            }
            return null;
        }

        /// <summary>
        /// If an implementer wants to export to a custom DDI or otherwise one that doesn't map,
        /// the appropriate DDI may be set in the Prescription prior to exporting.
        /// A ISO11783_DDI representation is as such the first mapping attempted.
        /// </summary>
        /// <param name="representation"></param>
        /// <param name="adaptUnit"></param>
        /// <returns></returns>
        private int DetermineVariableDDI(NumericRepresentation representation, UnitOfMeasure adaptUnit)
        {
            if (representation != null)
            {
                if (representation.CodeSource == RepresentationCodeSourceEnum.ISO11783_DDI)
                {
                    return Int32.Parse(representation.Code);
                }

                int? mappedDDI = RepresentationMapper.Map(representation);
                if (mappedDDI.HasValue)
                {
                    return mappedDDI.Value;
                }
            }

            if (adaptUnit != null && UnitFactory.DimensionToDdi.ContainsKey(adaptUnit.Dimension))
            {
                return UnitFactory.DimensionToDdi[adaptUnit.Dimension];
            }

            TaskDataMapper.AddError($"Unable to determine DDI for Prescription export {representation.Code}.", $"Representation ID : {representation.Id.ReferenceId}", "PrescriptionMapper.DetermineVariableDDI()");
            return 0; //Return an invalid DDI

        }

        private ISOEnumerations.ISOTaskStatus ExportStatus(WorkStatusEnum adaptStatus)
        {
            switch (adaptStatus)
            {
                case WorkStatusEnum.Cancelled:
                    return ISOEnumerations.ISOTaskStatus.Canceled;
                case WorkStatusEnum.Completed:
                    return ISOEnumerations.ISOTaskStatus.Completed;
                case WorkStatusEnum.InProgress:
                    return ISOEnumerations.ISOTaskStatus.Running;
                case WorkStatusEnum.PartiallyCompleted:
                case WorkStatusEnum.Paused:
                    return ISOEnumerations.ISOTaskStatus.Paused;
                case WorkStatusEnum.Scheduled:
                default:
                    return ISOEnumerations.ISOTaskStatus.Planned;
            }
        }

        #endregion Export

        #region Import
        public Prescription ImportPrescription(ISOTask task, WorkItem workItem)
        {
            Prescription prescription = null;

            if (task.HasRasterPrescription)
            {
                prescription = _gridMapper.Import(task, workItem);
            }
            else if (task.HasVectorPrescription)
            {
                prescription = ImportVectorPrescription(task, workItem);
            }
            else if (task.HasManualPrescription)
            {
                prescription = ImportManualPrescription(task, workItem);
            }

            return prescription;
         }

        public VectorPrescription ImportVectorPrescription(ISOTask task, WorkItem workItem)
        {
            VectorPrescription vectorRx = new VectorPrescription();
            ImportSharedPrescriptionProperties(task, workItem, vectorRx);
            vectorRx.RxShapeLookups = new List<RxShapeLookup>();
            foreach (ISOTreatmentZone treatmentZone in task.TreatmentZones)
            {
                RxShapeLookup shapeLookup = new RxShapeLookup();

                //Rates
                shapeLookup.Rates = new List<RxRate>();
                foreach (ISOProcessDataVariable pdv in treatmentZone.ProcessDataVariables)
                {
                    int? productID = TaskDataMapper.InstanceIDMap.GetADAPTID(pdv.ProductIdRef);
                    if (productID.HasValue)
                    {
                        shapeLookup.Rates.Add(PrescriptionMapper.ImportAndConvertRate(productID.Value, pdv, vectorRx));
                    }
                }

                //Shapes
                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                shapeLookup.Shape = new MultiPolygon();
                shapeLookup.Shape.Polygons = polygonMapper.ImportBoundaryPolygons(treatmentZone.Polygons).ToList();

                //Add to the collection
                vectorRx.RxShapeLookups.Add(shapeLookup);
            }

            return vectorRx;
        }

        public ManualPrescription ImportManualPrescription(ISOTask task, WorkItem workItem)
        {
            ManualPrescription manualRx = null;
            if (task.DefaultTreatmentZone != null)
            {
                foreach (ISOProcessDataVariable pdv in task.DefaultTreatmentZone.ProcessDataVariables)
                {
                    if (manualRx == null)
                    {
                        manualRx = new ManualPrescription();
                        manualRx.ProductUses = new List<ProductUse>();
                        manualRx.ProductIds = new List<int>();
                        ImportSharedPrescriptionProperties(task, workItem, manualRx);
                    }

                    if (pdv.ProductIdRef != null) //Products on ISO Rxs are optional, but without a place to store a product-agnostic prescription, those will not be imported here.
                    {
                        ProductUse productUse = new ProductUse();
                        int? productID = TaskDataMapper.InstanceIDMap.GetADAPTID(pdv.ProductIdRef);
                        if (productID.HasValue)
                        {
                            productUse.ProductId = productID.Value;
                            manualRx.ProductIds.Add(productID.Value);
                        }
                        productUse.Rate = pdv.ProcessDataValue.AsNumericRepresentationValue(pdv.ProcessDataDDI, RepresentationMapper);
                        manualRx.ProductUses.Add(productUse);
                    }
                }
            }
            return manualRx;
        }

        internal void ImportSharedPrescriptionProperties(ISOTask task, WorkItem workItem, Prescription prescription)
        {
            //Description
            prescription.Description = task.TaskDesignator;

            //CropZone/Field
            if (workItem.CropZoneId.HasValue)
            {
                prescription.CropZoneId = workItem.CropZoneId.Value;
            }
            else if (workItem.FieldId.HasValue)
            {
                prescription.FieldId = workItem.FieldId.Value;
            }

            //Products
            prescription.ProductIds = new List<int>();
            foreach (ISOTreatmentZone treatmentZone in task.TreatmentZones)
            {
                foreach (var dataVariable in treatmentZone.ProcessDataVariables)
                {
                    if (!string.IsNullOrEmpty(dataVariable.ProductIdRef))
                    {
                        int? productID = TaskDataMapper.InstanceIDMap.GetADAPTID(dataVariable.ProductIdRef);
                        if (productID.HasValue)
                        {
                            //ProductIDs
                            if (!prescription.ProductIds.Contains(productID.Value))
                            {
                                prescription.ProductIds.Add(productID.Value);
                            }

                            //Product Lookups
                            int ddi = dataVariable.ProcessDataDDI.AsInt32DDI();
                            var rxProductLookup = new RxProductLookup
                            {
                                ProductId = productID,
                                UnitOfMeasure = UnitFactory.Instance.GetUnitByDDI(ddi).ToAdaptUnit(),
                                Representation = (NumericRepresentation)RepresentationMapper.Map(ddi)
                            };

                            if (!prescription.RxProductLookups.Any(r => r.ProductId == rxProductLookup.ProductId))
                            {
                                prescription.RxProductLookups.Add(rxProductLookup);
                            }
                        }
                    }
                }
            }

            //Connections
            if (task.Connections.Any())
            {
                IEnumerable<EquipmentConfiguration> equipConfigs = _connectionMapper.ImportConnections(task);

                workItem.EquipmentConfigurationGroup = new EquipmentConfigurationGroup();
                workItem.EquipmentConfigurationGroup.EquipmentConfigurations = equipConfigs.ToList();

                DataModel.Catalog.EquipmentConfigurations.AddRange(equipConfigs);
            }
        }

        internal static RxRate ImportAndConvertRate(int productId, ISOProcessDataVariable pdv, Prescription prescription)
        {
            ISOUnit isoUnit = UnitFactory.Instance.GetUnitByDDI(pdv.ProcessDataDDI.AsInt32DDI());
            if (isoUnit != null)
            {
                double rate = isoUnit.ConvertFromIsoUnit(pdv.ProcessDataValue);
                return ImportRate(productId, rate, prescription);
            }
            else
            {
                return null;
            }
        }

        internal static RxRate ImportRate(int productId, double productRate, Prescription prescription)
        {
            RxProductLookup rxProductLookup = prescription.RxProductLookups.SingleOrDefault(x => x.ProductId == productId);
            return new RxRate()
            {
                Rate = productRate,
                RxProductLookupId = rxProductLookup?.Id?.ReferenceId ?? 0
            };
        }

        #endregion Import
    }
}
