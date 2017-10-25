using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ISectionMapper
    {
        List<DeviceElementUse> Map(ISOTime time, IEnumerable<ISOSpatialRow> isoRecords, int operationDataId);
        List<DeviceElementUse> ConvertToBaseTypes(List<DeviceElementUse> meters);
    }

    public class SectionMapper : BaseMapper, ISectionMapper
    {
        private readonly IWorkingDataMapper _workingDataMapper;

        public SectionMapper(IWorkingDataMapper meterMapper, TaskDataMapper taskDataMapper)
            : base (taskDataMapper, null)
        {
            _workingDataMapper = meterMapper;
        }

        public List<DeviceElementUse> Map(ISOTime time, IEnumerable<ISOSpatialRow> isoRecords, int operationDataId)
        {
            var sections = new List<DeviceElementUse>();

            IEnumerable<string> distinctDeviceElementIDs = time.DataLogValues.Select(d => d.DeviceElementIdRef).Distinct();
            foreach (string isoDeviceElementID in distinctDeviceElementIDs)
            {
                //For now we are treating multiple top level Device Elements all as Depth 0.   We are not rationalizing multiple devices into one hierarchy.
                DeviceElementHierarchy hierarchy = TaskDataMapper.DeviceElementHierarchies.GetRelevantHierarchy(isoDeviceElementID);
                if (hierarchy != null)
                {
                    DeviceElementUse deviceElementUse = null;
                    List<WorkingData> workingDatas = new List<WorkingData>();


                    if (hierarchy.DeviceElement.DeviceElementType == ISOEnumerations.ISODeviceElementType.Bin
                       && (hierarchy.Parent.DeviceElement.DeviceElementType == ISOEnumerations.ISODeviceElementType.Function ||
                           hierarchy.Parent.DeviceElement.DeviceElementType == ISOEnumerations.ISODeviceElementType.Device))
                    {
                        //-------------------------------------------------------------------------------------------------------------------------------------
                        //Bin children of functions or devices carry data that effectively belong to the parent device element in ISO.  See TC-GEO examples 5-8.
                        //Find the parent DeviceElementUse and add the data to that object.
                        int? parentDeviceElementID = TaskDataMapper.ADAPTIdMap.FindByISOId(hierarchy.Parent.DeviceElement.DeviceElementId);
                        if (parentDeviceElementID.HasValue)
                        {
                            DeviceElementConfiguration parentConfig = DataModel.Catalog.DeviceElementConfigurations.SingleOrDefault(c => c.DeviceElementId == parentDeviceElementID);
                            if (parentConfig == null)
                            {
                                DeviceElement adaptDeviceElement = DataModel.Catalog.DeviceElements.Single(d => d.Id.ReferenceId == parentDeviceElementID);
                                parentConfig = DeviceElementMapper.AddDeviceElementConfiguration(adaptDeviceElement, hierarchy, DataModel.Catalog);
                            }
                            deviceElementUse = sections.SingleOrDefault(d => d.DeviceConfigurationId == parentConfig.Id.ReferenceId);
                            if (deviceElementUse == null)
                            {
                                deviceElementUse = new DeviceElementUse();
                                deviceElementUse.Depth = hierarchy.Depth - 1;
                                deviceElementUse.Order = hierarchy.Order; //TODO this may have repeated order ids.
                                deviceElementUse.OperationDataId = operationDataId;
                                deviceElementUse.DeviceConfigurationId = parentConfig.Id.ReferenceId;
                            }
                            List<WorkingData> binData = _workingDataMapper.Map(time, isoRecords, deviceElementUse, hierarchy.Parent.DeviceElement.DeviceElementId, sections);
                            if (binData.Any())
                            {
                                workingDatas.AddRange(binData);
                            }
                        }
                        //---------------------
                    }
                    else
                    {
                        //Find DeviceConfigurationID
                        int adaptDeviceElementId = TaskDataMapper.ADAPTIdMap[isoDeviceElementID].Value;
                        DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.SingleOrDefault(c => c.DeviceElementId == adaptDeviceElementId);
                        if (config == null)
                        {
                            DeviceElement adaptDeviceElement = DataModel.Catalog.DeviceElements.Single(d => d.Id.ReferenceId == adaptDeviceElementId);
                            config = DeviceElementMapper.AddDeviceElementConfiguration(adaptDeviceElement, hierarchy, DataModel.Catalog);
                        }

                        //Read any spatially-listed widths/offsets on this data onto the DeviceElementConfiguration objects
                        hierarchy.SetWidthsAndOffsetsFromSpatialData(isoRecords, config, RepresentationMapper);

                        //Create the DeviceElementUse
                        deviceElementUse = new DeviceElementUse();
                        deviceElementUse.Depth = hierarchy.Depth;
                        deviceElementUse.Order = hierarchy.Order; //TODO this may have repeated order ids.
                        deviceElementUse.OperationDataId = operationDataId;
                        deviceElementUse.DeviceConfigurationId = config.Id.ReferenceId;

                        //Add Working Data for any data on this device element
                        List<WorkingData> standardData = _workingDataMapper.Map(time, isoRecords, deviceElementUse, isoDeviceElementID, sections);
                        if (standardData.Any())
                        {
                            workingDatas.AddRange(standardData);
                        }
                    }

                    deviceElementUse.GetWorkingDatas = () => workingDatas;

                    if (!sections.Contains(deviceElementUse))
                    {
                        sections.Add(deviceElementUse);
                    }
                }
            }

            return sections;
        }

        /// <summary>
        /// This call exists to translate any enumerated workingDatas (managed as a derived type within the plugin) back to an ADAPT-framework native type.   
        /// All other workingDatas pass through unchanged.  The containing DeviceElementUses are cloned, except for referencing the translated workingDatas.
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        public List<DeviceElementUse> ConvertToBaseTypes(List<DeviceElementUse> sections)
        {
            return sections.Select(x => {
                var section = new DeviceElementUse();
                var meters = x.GetWorkingDatas().Select(y => _workingDataMapper.ConvertToBaseType(y)).ToList();
                section.GetWorkingDatas = () => meters;
                section.Depth = x.Depth;
                section.Order = x.Order;
                section.OperationDataId = x.OperationDataId;
                section.TotalDistanceTravelled = x.TotalDistanceTravelled;
                section.TotalElapsedTime = x.TotalElapsedTime;
                section.DeviceConfigurationId = x.DeviceConfigurationId;
                return section;
                }).ToList();
        }

    }
}
