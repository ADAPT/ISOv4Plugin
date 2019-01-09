using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.UnitSystem;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    #region Import

    public interface IWorkingDataMapper
    {
        List<WorkingData> Map(ISOTime time, IEnumerable<ISOSpatialRow> isoRecords, DeviceElementUse deviceElementUse, DeviceElementHierarchy isoDeviceElementHierarchy, List<DeviceElementUse> pendingDeviceElementUses, Dictionary<string, List<ISOProductAllocation>> isoProductAllocations);
        WorkingData ConvertToBaseType(WorkingData meter);
        Dictionary<int, ISODataLogValue> DataLogValuesByWorkingDataID { get; set; }
        Dictionary<int, string> ISODeviceElementIDsByWorkingDataID { get; set; }
    }

    public class WorkingDataMapper : BaseMapper, IWorkingDataMapper
    {
        private readonly IEnumeratedMeterFactory _enumeratedMeterCreatorFactory;
        private readonly Dictionary<int, DdiDefinition> _ddis;
        public Dictionary<int, ISODataLogValue> DataLogValuesByWorkingDataID { get; set;}
        public Dictionary<int, string> ISODeviceElementIDsByWorkingDataID { get; set; }



        public WorkingDataMapper(IEnumeratedMeterFactory enumeratedMeterCreatorFactory, TaskDataMapper taskDataMapper)
            : base(taskDataMapper, null)
        {
            _enumeratedMeterCreatorFactory = enumeratedMeterCreatorFactory;
            _ddis = DdiLoader.Ddis;
            DataLogValuesByWorkingDataID = new Dictionary<int, ISODataLogValue>();
            ISODeviceElementIDsByWorkingDataID = new Dictionary<int, string>();
        }

        public List<WorkingData> Map(ISOTime time,
                                     IEnumerable<ISOSpatialRow> isoSpatialRows,
                                     DeviceElementUse deviceElementUse,
                                     DeviceElementHierarchy isoDeviceElementHierarchy,
                                     List<DeviceElementUse> pendingDeviceElementUses,
                                     Dictionary<string, List<ISOProductAllocation>> isoProductAllocations)
        {
            var workingDatas = new List<WorkingData>();

           //Set orders on the collection of DLVs
            var allDLVs = time.DataLogValues;
            for (int order = 0; order < allDLVs.Count(); order++)
            {
                var dlv = allDLVs.ElementAt(order);
                dlv.Order = order;
            }


            //Create vrProductIndex on relevant device elements if more than one product on this OperationData
            if (TimeLogMapper.GetDistinctProductIDs(TaskDataMapper, isoProductAllocations).Count > 1 &&
                isoProductAllocations.Keys.Contains(isoDeviceElementHierarchy.DeviceElement.DeviceElementId))
            {
                WorkingData workingData = CreateProductIndexWorkingData(deviceElementUse.Id.ReferenceId);
                ISODeviceElementIDsByWorkingDataID.Add(workingData.Id.ReferenceId, isoDeviceElementHierarchy.DeviceElement.DeviceElementId);
                workingDatas.Add(workingData);
            }

            //Add the Working Datas for this DeviceElement
            IEnumerable<ISODataLogValue> deviceElementDLVs = allDLVs.Where(dlv => dlv.DeviceElementIdRef == isoDeviceElementHierarchy.DeviceElement.DeviceElementId);
            foreach (ISODataLogValue dlv in deviceElementDLVs)
            {
                IEnumerable<WorkingData> newWorkingDatas = Map(dlv, 
                                                               isoSpatialRows,
                                                               deviceElementUse,
                                                               dlv.Order,
                                                               pendingDeviceElementUses,
                                                               isoDeviceElementHierarchy);
                if (newWorkingDatas.Count() > 0)
                {
                    int ddi = dlv.ProcessDataDDI.AsInt32DDI();
                    if (!EnumeratedMeterFactory.IsCondensedMeter(ddi))
                    {
                        //We skip adding Condensed WorkingDatas to this DeviceElementUse since they were added separately below to their specific DeviceElementUse
                        workingDatas.AddRange(newWorkingDatas);
                    }
                }
            }

            return workingDatas;
        }

        public WorkingData ConvertToBaseType(WorkingData meter)
        {
            if (meter is ISOEnumeratedMeter)
            {
                var enumMeter = (ISOEnumeratedMeter)meter;
                var newMeter = new EnumeratedWorkingData
                {
                    AppliedLatency = enumMeter.AppliedLatency,
                    DeviceElementUseId = enumMeter.DeviceElementUseId,
                    ReportedLatency = enumMeter.ReportedLatency,
                    Representation = enumMeter.Representation,
                    ValueCodes = enumMeter.ValueCodes,
                };
                newMeter.Id.ReferenceId = meter.Id.ReferenceId;
                newMeter.Id.UniqueIds = meter.Id.UniqueIds;

                return newMeter;
            }
            return meter;
        }

        private IEnumerable<WorkingData> Map(ISODataLogValue dlv, 
                                             IEnumerable<ISOSpatialRow> isoSpatialRows, 
                                             DeviceElementUse deviceElementUse, 
                                             int order, 
                                             List<DeviceElementUse> pendingDeviceElementUses,
                                             DeviceElementHierarchy isoDeviceElementHierarchy)
        {
            var workingDatas = new List<WorkingData>();
            if (_ddis.ContainsKey(dlv.ProcessDataDDI.AsInt32DDI()))
            {
                //Numeric Representations
                NumericWorkingData numericMeter = MapNumericMeter(dlv, deviceElementUse.Id.ReferenceId);
                DataLogValuesByWorkingDataID.Add(numericMeter.Id.ReferenceId, dlv);
                ISODeviceElementIDsByWorkingDataID.Add(numericMeter.Id.ReferenceId, dlv.DeviceElementIdRef);
                workingDatas.Add(numericMeter);
                return workingDatas;
            }
            var meterCreator = _enumeratedMeterCreatorFactory.GetMeterCreator(dlv.ProcessDataDDI.AsInt32DDI());
            if (meterCreator != null)
            {
                //Enumerated Representations
                var isoEnumeratedMeters = meterCreator.CreateMeters(isoSpatialRows);
                foreach (ISOEnumeratedMeter enumeratedMeter in isoEnumeratedMeters)
                {
                    DataLogValuesByWorkingDataID.Add(enumeratedMeter.Id.ReferenceId, dlv);
                    ISODeviceElementIDsByWorkingDataID.Add(enumeratedMeter.Id.ReferenceId, dlv.DeviceElementIdRef);
                    enumeratedMeter.DeviceElementUseId = deviceElementUse.Id.ReferenceId;
                }
                workingDatas.AddRange(isoEnumeratedMeters);

                if (meterCreator is CondensedStateMeterCreator)
                {
                    UpdateCondensedWorkingDatas(workingDatas, dlv, deviceElementUse, pendingDeviceElementUses, isoDeviceElementHierarchy);
                }
            }
            else
            {
                //Proprietary DDIs - report out as numeric value
                NumericWorkingData proprietaryWorkingData = new NumericWorkingData();
                proprietaryWorkingData.Representation = new ApplicationDataModel.Representations.NumericRepresentation { Code = dlv.ProcessDataDDI, CodeSource = RepresentationCodeSourceEnum.ISO11783_DDI };
                proprietaryWorkingData.DeviceElementUseId = deviceElementUse.Id.ReferenceId;

                //Take any information from DPDs/DVPs
                ApplicationDataModel.Common.UnitOfMeasure uom = null;
                ISODeviceElement det = isoDeviceElementHierarchy.DeviceElement;
                if (det != null)
                {
                    ISODeviceProcessData dpd = det.DeviceProcessDatas.FirstOrDefault(d => d.DDI == dlv.ProcessDataDDI);
                    if (dpd != null)
                    {
                        proprietaryWorkingData.Representation.Description = dpd.Designator; //Update the representation with a name since we have one here.
                        ISODeviceValuePresentation dvp = det.Device.DeviceValuePresentations.FirstOrDefault(d => d.ObjectID == dpd.DeviceValuePresentationObjectId);
                        if (dvp != null && dvp.UnitDesignator != null)
                        {
                            if (AgGateway.ADAPT.Representation.UnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures.Contains(dvp.UnitDesignator))
                            {
                                //The unit designator used by the OEM will need to match ADAPT for this to work, otherwise we'll need to default to 'count' below
                                //It will likely work for many simple units and will not for work compound units
                                uom = UnitSystemManager.GetUnitOfMeasure(dvp.UnitDesignator);
                            }
                        }   
                    }
                }

                proprietaryWorkingData.UnitOfMeasure = uom ?? UnitSystemManager.GetUnitOfMeasure("count"); //Best we can do

                DataLogValuesByWorkingDataID.Add(proprietaryWorkingData.Id.ReferenceId, dlv);
                ISODeviceElementIDsByWorkingDataID.Add(proprietaryWorkingData.Id.ReferenceId, dlv.DeviceElementIdRef);
                workingDatas.Add(proprietaryWorkingData);
            }
            return workingDatas;
        }

        private NumericWorkingData MapNumericMeter(ISODataLogValue dlv, int deviceElementUseId)
        {
            var meter = new NumericWorkingData
            {
                UnitOfMeasure = RepresentationMapper.GetUnitForDdi(dlv.ProcessDataDDI.AsInt32DDI()),
                DeviceElementUseId = deviceElementUseId,
                Representation = RepresentationMapper.Map(dlv.ProcessDataDDI.AsInt32DDI())
            };
            return meter;
        }

        private NumericWorkingData CreateProductIndexWorkingData(int deviceElementUseId)
        {
            var meter = new NumericWorkingData
            {
                UnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("count"),
                DeviceElementUseId = deviceElementUseId,
                Representation = RepresentationMapper.GetRepresentation("vrProductIndex")
            };
            return meter;
        }

        private void UpdateCondensedWorkingDatas(List<WorkingData> condensedWorkingDatas, ISODataLogValue dlv, DeviceElementUse deviceElementUse, List<DeviceElementUse> pendingDeviceElementUses, DeviceElementHierarchy isoDeviceElementHierarchy)
        {
            ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(dlv.DeviceElementIdRef);
            IEnumerable<ISODeviceElement> isoSectionElements = isoDeviceElement.ChildDeviceElements.Where(d => d.DeviceElementType == ISOEnumerations.ISODeviceElementType.Section);
            if (isoSectionElements.Count() > 0 && isoSectionElements.Count() <= condensedWorkingDatas.Count)
            {
                //We have found the expected number of sections in the DDOP
                List<ISODeviceElement> targetSections = isoSectionElements.ToList();

                //Update the DeviceElementReference on the Condensed WorkingDatas
                for (int i = 0; i < isoSectionElements.Count(); i++)
                {
                    WorkingData workingData = condensedWorkingDatas[i];

                    DeviceElementUse condensedDeviceElementUse = new DeviceElementUse();
                    condensedDeviceElementUse.OperationDataId = deviceElementUse.OperationDataId;

                    ISODeviceElement targetSection = targetSections[i];
                    int? deviceElementID = TaskDataMapper.InstanceIDMap.GetADAPTID(targetSection.DeviceElementId);
                    if (deviceElementID.HasValue)
                    {
                        DeviceElement deviceElement = DataModel.Catalog.DeviceElements.SingleOrDefault(d => d.Id.ReferenceId == deviceElementID.Value);
                        if (deviceElement != null)
                        {
                            //Reference the device element in its hierarchy so that we can get the depth & order
                            DeviceElementHierarchy deviceElementInHierarchy = isoDeviceElementHierarchy.FromDeviceElementID(targetSection.DeviceElementId);

                            //Get the config id
                            DeviceElementConfiguration deviceElementConfig = DeviceElementMapper.GetDeviceElementConfiguration(deviceElement, deviceElementInHierarchy, DataModel.Catalog);
                            condensedDeviceElementUse.DeviceConfigurationId = deviceElementConfig.Id.ReferenceId;

                            //Set the depth & order
                            condensedDeviceElementUse.Depth = deviceElementInHierarchy.Depth;
                            condensedDeviceElementUse.Order = deviceElementInHierarchy.Order;
                        }
                    }

                    condensedDeviceElementUse.GetWorkingDatas = () => new List<WorkingData> { workingData };

                    workingData.DeviceElementUseId = condensedDeviceElementUse.Id.ReferenceId;

                    pendingDeviceElementUses.Add(condensedDeviceElementUse);
                }
            }
        }

        #endregion Import
    }
}
