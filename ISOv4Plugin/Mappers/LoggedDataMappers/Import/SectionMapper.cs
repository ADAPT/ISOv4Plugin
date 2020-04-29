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
        List<DeviceElementUse> Map(ISOTime time, IEnumerable<ISOSpatialRow> isoRecords, int operationDataId, IEnumerable<string> isoDeviceElementIDs, Dictionary<string, List<ISOProductAllocation>> isoProductAllocations);
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

        public List<DeviceElementUse> Map(ISOTime time,
                                          IEnumerable<ISOSpatialRow> isoRecords,
                                          int operationDataId,
                                          IEnumerable<string> isoDeviceElementIDs,
                                          Dictionary<string, List<ISOProductAllocation>> isoProductAllocations)
        {
            var sections = new List<DeviceElementUse>();
            foreach (string isoDeviceElementID in isoDeviceElementIDs)
            {
                DeviceElementHierarchy hierarchy = TaskDataMapper.DeviceElementHierarchies.GetRelevantHierarchy(isoDeviceElementID);
                if (hierarchy != null)
                {
                    DeviceElementUse deviceElementUse = null;
                    List<WorkingData> workingDatas = new List<WorkingData>();

                    //Get the relevant DeviceElementConfiguration
                    int adaptDeviceElementId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDeviceElementID).Value;
                    DeviceElement adaptDeviceElement = DataModel.Catalog.DeviceElements.SingleOrDefault(d => d.Id.ReferenceId == adaptDeviceElementId);
                    if (adaptDeviceElement != null)
                    {
                        DeviceElementConfiguration config = DeviceElementMapper.GetDeviceElementConfiguration(adaptDeviceElement, hierarchy, DataModel.Catalog);

                        int depth = hierarchy.Depth;
                        int order = hierarchy.Order;
                        if (config.DeviceElementId == adaptDeviceElement.ParentDeviceId)
                        {
                            //The configuration references the parent ISO element
                            depth = hierarchy.Parent.Depth;
                            order = hierarchy.Parent.Order;
                        }

                        //Read any spatially-listed widths/offsets on this data onto the DeviceElementConfiguration objects
                        hierarchy.SetWidthsAndOffsetsFromSpatialData(isoRecords, config, RepresentationMapper);

                        deviceElementUse = sections.FirstOrDefault(d => d.DeviceConfigurationId == config.Id.ReferenceId);
                        if (deviceElementUse == null)
                        {
                            //Create the DeviceElementUse
                            deviceElementUse = new DeviceElementUse();
                            deviceElementUse.Depth = depth;
                            deviceElementUse.Order = order;
                            deviceElementUse.OperationDataId = operationDataId;
                            deviceElementUse.DeviceConfigurationId = config.Id.ReferenceId;

                            //Add Working Data for any data on this device element
                            List<WorkingData> data = _workingDataMapper.Map(time, isoRecords, deviceElementUse, hierarchy, sections, isoProductAllocations);
                            if (data.Any())
                            {
                                workingDatas.AddRange(data);
                            }
                        }
                        else
                        {
                            workingDatas = deviceElementUse.GetWorkingDatas().ToList();

                            //Add Additional Working Data
                            List<WorkingData> data = _workingDataMapper.Map(time, isoRecords, deviceElementUse, hierarchy, sections, isoProductAllocations);
                            if (data.Any())
                            {
                                workingDatas.AddRange(data);
                            }
                        }

                        deviceElementUse.GetWorkingDatas = () => workingDatas;

                        if (!sections.Contains(deviceElementUse))
                        {
                            sections.Add(deviceElementUse);
                        }
                    }
                    else if (hierarchy.DeviceElement.DeviceElementType == ISOEnumerations.ISODeviceElementType.Connector)
                    {
                        int? connectorID = TaskDataMapper.InstanceIDMap.GetADAPTID(hierarchy.DeviceElement.DeviceElementId).Value;
                        if (connectorID.HasValue)
                        {
                            Connector adaptConnector = DataModel.Catalog.Connectors.FirstOrDefault(c => c.Id.ReferenceId == connectorID.Value);
                            if (adaptConnector != null)
                            {
                                HitchPoint hitch = DataModel.Catalog.HitchPoints.FirstOrDefault(h => h.Id.ReferenceId == adaptConnector.HitchPointId);
                                if (hitch != null)
                                {
                                    hierarchy.SetWidthsAndOffsetsFromSpatialData(isoRecords, hitch, RepresentationMapper);
                                }
                            }
                        }
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
                section.Id.ReferenceId = x.Id.ReferenceId;
                section.Id.UniqueIds = x.Id.UniqueIds;
                return section;
                }).ToList();
        }

    }
}
