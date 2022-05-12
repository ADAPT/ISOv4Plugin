using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

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
                                          Dictionary<string, List<ISOProductAllocation>> productAllocations)
        {
            // Determine the lowest depth at which product allocations are reported
            DeviceHierarchyElement deviceElement = isoDeviceElementIDs
                .Select(x => TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(x))
                .Where(x => x != null)
                .FirstOrDefault();
            int lowestLevel = GetLowestProductAllocationLevel(deviceElement?.GetRootDeviceElementHierarchy(), productAllocations);
            // Remove allocations for all other levels
            Dictionary<string, List<ISOProductAllocation>> isoProductAllocations = productAllocations
                .Where(x =>TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(x.Key)?.Depth == lowestLevel)
                .ToDictionary(x => x.Key, x => x.Value);

            var sections = new List<DeviceElementUse>();
            foreach (string isoDeviceElementID in isoDeviceElementIDs)
            {
                DeviceHierarchyElement hierarchyElement = TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(isoDeviceElementID);
                if (hierarchyElement != null)
                {
                    DeviceElementUse deviceElementUse = null;
                    List<WorkingData> workingDatas = new List<WorkingData>();

                    //Get the relevant DeviceElementConfiguration
                    int adaptDeviceElementId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDeviceElementID).Value;
                    DeviceElement adaptDeviceElement = DataModel.Catalog.DeviceElements.SingleOrDefault(d => d.Id.ReferenceId == adaptDeviceElementId);
                    if (adaptDeviceElement != null)
                    {
                        DeviceElementConfiguration config = DeviceElementMapper.GetDeviceElementConfiguration(adaptDeviceElement, hierarchyElement, DataModel.Catalog);

                        int depth = hierarchyElement.Depth;
                        int order = hierarchyElement.Order;
                        if (config.DeviceElementId == adaptDeviceElement.ParentDeviceId)
                        {
                            //The configuration references the parent ISO element
                            depth = hierarchyElement.Parent.Depth;
                            order = hierarchyElement.Parent.Order;
                        }

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
                            List<WorkingData> data = _workingDataMapper.Map(time, isoRecords, deviceElementUse, hierarchyElement, sections, isoProductAllocations);
                            if (data.Any())
                            {
                                workingDatas.AddRange(data);
                            }
                        }
                        else
                        {
                            workingDatas = deviceElementUse.GetWorkingDatas().ToList();

                            //Add Additional Working Data
                            List<WorkingData> data = _workingDataMapper.Map(time, isoRecords, deviceElementUse, hierarchyElement, sections, isoProductAllocations);
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

        private int GetLowestProductAllocationLevel(DeviceHierarchyElement isoDeviceElementHierarchy, Dictionary<string, List<ISOProductAllocation>> isoProductAllocations)
        {
            int level = -1;
            // If device element has direct product allocations, use its Depth.
            if (isoProductAllocations.TryGetValue(isoDeviceElementHierarchy?.DeviceElement.DeviceElementId, out List<ISOProductAllocation> productAllocations) &&
                productAllocations.Any(x => x.DeviceElementIdRef == isoDeviceElementHierarchy.DeviceElement.DeviceElementId))
            {
                level = isoDeviceElementHierarchy.Depth;
            }

            // Get max level from children elements
            int? maxChildLevel = isoDeviceElementHierarchy?.Children?.Max(x => GetLowestProductAllocationLevel(x, isoProductAllocations));

            return Math.Max(level, maxChildLevel.GetValueOrDefault(-1));
        }
    }
}
