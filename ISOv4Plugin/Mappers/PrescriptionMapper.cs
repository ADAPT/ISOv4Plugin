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
        private CodedCommentMapper _commentMapper;
        private GridMapper _gridMapper;
        public PrescriptionMapper(TaskDataMapper taskDataMapper, GridMapper gridMapper, CodedCommentMapper commentMapper)
            :base (taskDataMapper, "TSK")
        {
            _unitConverter = new ADAPT.Representation.UnitSystem.UnitOfMeasureConverter();
            _gridMapper = gridMapper;
            _commentMapper = commentMapper;
        }

        #region Export
        public ISOTask ExportPrescription(WorkItem workItem, int gridType, Prescription prescription)
        {
            ISOTask task = new ISOTask();

            //Task ID
            string taskID = workItem.Id.FindIsoId() ?? GenerateId();
            task.TaskID = taskID;
            ExportUniqueIDs(workItem.Id, taskID);
            TaskDataMapper.ISOIdMap.Add(workItem.Id.ReferenceId, taskID);

            //Designator
            task.TaskDesignator = prescription.Description;

            //Customer Ref
            if (workItem.GrowerId.HasValue)
            {
                task.CustomerIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(workItem.GrowerId.Value);
            }

            //Farm Ref
            if (workItem.FarmId.HasValue)
            {
                task.FarmIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(workItem.FarmId.Value);
            }

            //Partfield Ref
            if (workItem.CropZoneId.HasValue)
            {
                task.PartFieldIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(workItem.CropZoneId.Value);
            }
            else if (workItem.FieldId.HasValue)
            {
                task.PartFieldIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(workItem.FieldId.Value);
            }

            //Comments
            if (workItem.Notes.Any())
            {
                CommentAllocationMapper canMapper = new CommentAllocationMapper(TaskDataMapper, _commentMapper);
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
            throw new NotImplementedException();
        }

        private void ExportManualPresciption(ISOTask task, ManualPrescription rx)
        {
            throw new NotImplementedException();
        }

        private List<byte> ExportTreatmentZonesForType1(ISOTask task, RasterGridPrescription prescription)
        {
            Dictionary<string, ISOTreatmentZone> treatmentZones = new Dictionary<string, ISOTreatmentZone>();
            List<byte> rateCodePerCell = new List<byte>();

            byte tznCounter = 1;
            foreach (RxRates cellRates in prescription.Rates)
            {
                string key = GetRxRatesKey(cellRates);
                if (!treatmentZones.ContainsKey(key))
                {
                    ISOTreatmentZone tzn = GetNewType1TreatmentZone(cellRates, tznCounter);
                    treatmentZones.Add(key, tzn);
                    task.TreatmentZones.Add(tzn);
                    tznCounter++;
                }
                rateCodePerCell.Add(treatmentZones[key].TreatmentZoneCode);
            }
            return rateCodePerCell;

            //Determines a unique key that describes the products and rates assigned to each cell.
            string GetRxRatesKey(RxRates rates)
            {
                string key = string.Empty;
                rates.RxRate.ForEach(r => key += $"{r.RxProductLookupId}:{r.Rate}|");
                return key;
            }

            //Adds a treatment zone for a new rate combination
            ISOTreatmentZone GetNewType1TreatmentZone(RxRates rates, byte counter)
            {
                ISOTreatmentZone treatmentZone = new ISOTreatmentZone() { TreatmentZoneCode = counter, TreatmentZoneDesignator = $"TreatmentZone {counter.ToString()}" };

                foreach (RxRate rate in rates.RxRate)
                {
                    ISOProcessDataVariable processDataVariable = new ISOProcessDataVariable();
                    RxProductLookup lookup = prescription.RxProductLookups.FirstOrDefault(l => l.Id.ReferenceId == rate.RxProductLookupId);
                    if (lookup != null)
                    {
                        processDataVariable.ProductIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(lookup.ProductId.Value);
                        processDataVariable.ProcessDataDDI = DetermineVariableDDI(lookup.UnitOfMeasure).AsHexDDI();
                        processDataVariable.ProcessDataValue = (long)rate.Rate; 
                    }
                    treatmentZone.ProcessDataVariables.Add(processDataVariable);
                }
                return treatmentZone;
            }
        }

        private ISOTreatmentZone ExportTreatmentZonesForType2(ISOTask task, RasterGridPrescription prescription)
        {
            if (prescription.ProductIds == null)
            {
                return null;
            }

            var lossOfSignalTreatmentZone = new ISOTreatmentZone { TreatmentZoneDesignator = "Loss of GPS", ProcessDataVariables = new List<ISOProcessDataVariable>() };
            var outOfFieldTreatmentZone = new ISOTreatmentZone { TreatmentZoneDesignator = "Out of Field", ProcessDataVariables = new List<ISOProcessDataVariable>() };
            var defaultTreatmentZone = new ISOTreatmentZone { TreatmentZoneDesignator = "Default", ProcessDataVariables = new List<ISOProcessDataVariable>() };

            var defaultRate = new NumericRepresentationValue(null, new NumericValue(prescription.RxProductLookups.First().UnitOfMeasure, 0)); //We are always setting a 0 default rate

            foreach (var productId in prescription.ProductIds)
            {
                var isoUnit = DetermineIsoUnit(prescription.RxProductLookups.First(p => p.ProductId == productId).UnitOfMeasure);  

                string isoProductId = string.Empty;
                if (TaskDataMapper.ISOIdMap.ContainsKey(productId))
                {
                    isoProductId = TaskDataMapper.ISOIdMap[productId];
                }

                ISOProcessDataVariable lossPDV = ExportProcessDataVariable(prescription.LossOfGpsRate, isoProductId, isoUnit);
                if (lossPDV != null)
                {
                    lossOfSignalTreatmentZone.ProcessDataVariables.Add(lossPDV);
                }
                ISOProcessDataVariable oofPDV = ExportProcessDataVariable(prescription.OutOfFieldRate, isoProductId, isoUnit);
                if (oofPDV != null)
                {
                    outOfFieldTreatmentZone.ProcessDataVariables.Add(oofPDV);
                }
                ISOProcessDataVariable defaultPDV = ExportProcessDataVariable(defaultRate, isoProductId, isoUnit);
                if (defaultPDV != null)
                {
                    defaultTreatmentZone.ProcessDataVariables.Add(defaultPDV);
                }
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

        private ISOProcessDataVariable ExportProcessDataVariable(NumericRepresentationValue value, string productId, ISOUnit unit)
        {
            if (value != null && value.Value != null)
            {
                var targetValue = value.Value.Value;

                // Convert input value to Iso unit
                UnitOfMeasure adaptUnit = unit.ToAdaptUnit();
                UnitOfMeasure srcUnit = null;
                if (adaptUnit != null && value.Value.UnitOfMeasure != null &&
                    adaptUnit.Dimension == value.Value.UnitOfMeasure.Dimension)
                {
                    srcUnit = value.Value.UnitOfMeasure;
                    targetValue = _unitConverter.Convert(srcUnit.ToInternalUom(), adaptUnit.ToInternalUom(), targetValue);
                }

                var dataVariable = new ISOProcessDataVariable
                {
                    ProductIdRef = productId,
                    ProcessDataValue = (long)targetValue,
                    ProcessDataDDI = DetermineVariableDDI(adaptUnit).AsHexDDI() //TODO This needs work
                    //TODO VPN
                };

                return dataVariable;
            }
            return null;
        }

        private static int DetermineVariableDDI(UnitOfMeasure adaptUnit)
        {
            if (adaptUnit != null && UnitFactory.DimensionToDdi.ContainsKey(adaptUnit.Dimension))
                return UnitFactory.DimensionToDdi[adaptUnit.Dimension];

            //TODO this currently defaults to a dry application if nothing else can be determined
            return 6;
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
                //TODO Vector prescription
            }
            else if (task.HasManualPrescription)
            {
                //TODO Task has PDVs but no grid or polygons.   Manual prescription.
            }

            if (task.CommentAllocations.Any())
            {
                CommentAllocationMapper canMapper = new CommentAllocationMapper(TaskDataMapper, _commentMapper);
                workItem.Notes = canMapper.ImportCommentAllocations(task.CommentAllocations).ToList();
            }

            return prescription;
         }

        #endregion Import
    }
}
