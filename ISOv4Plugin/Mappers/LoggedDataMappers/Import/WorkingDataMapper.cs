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
        List<WorkingData> Map(ISOTime time, IEnumerable<ISOSpatialRow> isoRecords, DeviceElementUse deviceElementUse, DeviceHierarchyElement isoDeviceElementHierarchy, List<DeviceElementUse> pendingDeviceElementUses, Dictionary<string, List<ISOProductAllocation>> isoProductAllocations);
        WorkingData ConvertToBaseType(WorkingData meter);
        Dictionary<int, ISODataLogValue> DataLogValuesByWorkingDataID { get; set; }
        Dictionary<int, string> ISODeviceElementIDsByWorkingDataID { get; set; }
    }

    public class WorkingDataMapper : BaseMapper, IWorkingDataMapper
    {
        private readonly List<string> _implementGeometryDDIsToOmit = new List<string> { "0044", "0046", "0086", "0087", "0088" };
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
                                     DeviceHierarchyElement isoDeviceElementHierarchy,
                                     List<DeviceElementUse> pendingDeviceElementUses,
                                     Dictionary<string, List<ISOProductAllocation>> isoProductAllocations)
        {
            var workingDatas = new List<WorkingData>();

            //Create vrProductIndex on relevant device elements if more than one product on this OperationData
            if (TimeLogMapper.GetDistinctProductIDs(TaskDataMapper, isoProductAllocations).Count > 1 &&
                isoProductAllocations.Keys.Contains(isoDeviceElementHierarchy.DeviceElement.DeviceElementId))
            {
                WorkingData workingData = CreateProductIndexWorkingData(deviceElementUse.Id.ReferenceId);
                ISODeviceElementIDsByWorkingDataID.Add(workingData.Id.ReferenceId, isoDeviceElementHierarchy.DeviceElement.DeviceElementId);
                workingDatas.Add(workingData);
            }

            //Add the Working Datas for this DeviceElement
            IEnumerable<ISODataLogValue> deviceElementDLVs = time.DataLogValues.Where(dlv => dlv.DeviceElementIdRef == isoDeviceElementHierarchy.DeviceElement.DeviceElementId ||  //DLV DET reference matches the primary DET for the ADAPT element
                                                                                  isoDeviceElementHierarchy.MergedElements.Any(e => e.DeviceElementId == dlv.DeviceElementIdRef)); //DLV DET reference matches one of the merged DETs on the ADAPT element


            foreach (ISODataLogValue dlv in deviceElementDLVs.Where(d => !_implementGeometryDDIsToOmit.Contains(d.ProcessDataDDI))) //Omit implement geomtry data from the spatial records (with the exception of 0043 working width which is commonly dynamic).
            {
                IEnumerable<WorkingData> newWorkingDatas = Map(dlv, 
                                                               isoSpatialRows,
                                                               deviceElementUse,
                                                               dlv.Index,
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

        /// <summary>
        /// This method returns multiple WorkingData objects for a single DLV in the ISO model
        /// to handle the Condensed DDI case where a single DLV can contain multiple logical
        /// data points.  For non-condensed DDIs, the method will return a single item in output enumerable.
        /// </summary>
        /// <param name="dlv"></param>
        /// <param name="isoSpatialRows"></param>
        /// <param name="deviceElementUse"></param>
        /// <param name="order"></param>
        /// <param name="pendingDeviceElementUses"></param>
        /// <param name="isoDeviceElementHierarchy"></param>
        /// <returns></returns>
        private IEnumerable<WorkingData> Map(ISODataLogValue dlv, 
                                             IEnumerable<ISOSpatialRow> isoSpatialRows, 
                                             DeviceElementUse deviceElementUse, 
                                             byte order, 
                                             List<DeviceElementUse> pendingDeviceElementUses,
                                             DeviceHierarchyElement isoDeviceElementHierarchy)
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
                var isoEnumeratedMeters = meterCreator.CreateMeters(isoSpatialRows, dlv);
                foreach (ISOEnumeratedMeter enumeratedMeter in isoEnumeratedMeters)
                {
                    DataLogValuesByWorkingDataID.Add(enumeratedMeter.Id.ReferenceId, dlv);
                    ISODeviceElementIDsByWorkingDataID.Add(enumeratedMeter.Id.ReferenceId, dlv.DeviceElementIdRef);
                    enumeratedMeter.DeviceElementUseId = deviceElementUse.Id.ReferenceId;
                }
                workingDatas.AddRange(isoEnumeratedMeters);

                if (meterCreator is CondensedStateMeterCreator)
                {
                    UpdateCondensedWorkingDatas(workingDatas.Cast<ISOEnumeratedMeter>().ToList(), dlv, deviceElementUse, pendingDeviceElementUses, isoDeviceElementHierarchy);
                }
            }
            else
            {
                //Proprietary DDIs - report out as numeric value
                NumericWorkingData proprietaryWorkingData = new NumericWorkingData();
                proprietaryWorkingData.Representation = new ApplicationDataModel.Representations.NumericRepresentation { Code = dlv.ProcessDataDDI, CodeSource = RepresentationCodeSourceEnum.ISO11783_DDI };
                proprietaryWorkingData.DeviceElementUseId = deviceElementUse.Id.ReferenceId;

                //Always set unit as count.   In SpatialRecordMapper, we will place the DVP unit on the NumericRepresentationValue.UserProvidedUnitOfMeasure
                //so that consumers can apply any offset/scaling to get to the desired display unit.
                proprietaryWorkingData.UnitOfMeasure =  UnitSystemManager.GetUnitOfMeasure("count");

                //Take any information from DPD
                ISODeviceElement det = isoDeviceElementHierarchy.DeviceElement ??
                                       isoDeviceElementHierarchy.MergedElements.FirstOrDefault(me => me.DeviceElementId == dlv.DeviceElementIdRef);
                if (det != null)
                {
                    ISODeviceProcessData dpd = det.DeviceProcessDatas.FirstOrDefault(d => d.DDI == dlv.ProcessDataDDI);
                    if (dpd != null)
                    {
                        proprietaryWorkingData.Representation.Description = dpd.Designator; //Update the representation with a name since we have one here.  
                    }
                }

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

        private void UpdateCondensedWorkingDatas(List<ISOEnumeratedMeter> condensedWorkingDatas, ISODataLogValue dlv, DeviceElementUse deviceElementUse, List<DeviceElementUse> pendingDeviceElementUses, DeviceHierarchyElement isoDeviceElementHierarchy)
        {
            ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(dlv.DeviceElementIdRef);
            List<ISODeviceElement> isoSectionElements = isoDeviceElement.ChildDeviceElements.Where(d => d.DeviceElementType == ISOEnumerations.ISODeviceElementType.Section).ToList();
            //We have some sections in the DDOP
            if (isoSectionElements.Count > 0)
            {
                //Update the DeviceElementReference on the Condensed WorkingDatas
                foreach (var workingData in condensedWorkingDatas)
                {
                    if (workingData.SectionIndex - 1 >= isoSectionElements.Count)
                    {
                        break;
                    }
                    ISODeviceElement targetSection = isoSectionElements[workingData.SectionIndex - 1];

                    DeviceElementUse condensedDeviceElementUse = FindExistingDeviceElementUseForCondensedData(targetSection, pendingDeviceElementUses);
                    if (condensedDeviceElementUse == null)
                    {
                        //Make a new DeviceElementUse
                        condensedDeviceElementUse = new DeviceElementUse();
                        condensedDeviceElementUse.OperationDataId = deviceElementUse.OperationDataId;

                        int? deviceElementID = TaskDataMapper.InstanceIDMap.GetADAPTID(targetSection.DeviceElementId);
                        if (deviceElementID.HasValue)
                        {
                            DeviceElement deviceElement = DataModel.Catalog.DeviceElements.SingleOrDefault(d => d.Id.ReferenceId == deviceElementID.Value);
                            if (deviceElement != null)
                            {
                                //Reference the device element in its hierarchy so that we can get the depth & order
                                DeviceHierarchyElement deviceElementInHierarchy = isoDeviceElementHierarchy.FromDeviceElementID(targetSection.DeviceElementId);

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
                    else
                    {
                        //Use the existing DeviceElementUse
                        List<WorkingData> data = new List<WorkingData>();
                        IEnumerable<WorkingData> existingWorkingDatas = condensedDeviceElementUse.GetWorkingDatas();
                        if (existingWorkingDatas != null)
                        {
                            data.AddRange(existingWorkingDatas.ToList());  //Add the preexisting 
                        }
                        data.Add(workingData);
                        condensedDeviceElementUse.GetWorkingDatas = () => data;
                    }
                }
            }
        }

        private DeviceElementUse FindExistingDeviceElementUseForCondensedData(ISODeviceElement targetSection, List<DeviceElementUse> pendingDeviceElementUses)
        {
            DeviceElementUse existingDeviceElementUse = null;
            int? deviceElementID =  TaskDataMapper.InstanceIDMap.GetADAPTID(targetSection.DeviceElementId);
            if (deviceElementID.HasValue)
            {
                DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(d => d.DeviceElementId == deviceElementID.Value);
                if (config != null)
                {
                    existingDeviceElementUse = pendingDeviceElementUses.FirstOrDefault(p => p.DeviceConfigurationId == config.Id.ReferenceId);
                }
            }
            return existingDeviceElementUse;
        }

        #endregion Import
    }
}
