using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
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
            : base(taskDataMapper, null)
        {
            _workingDataMapper = meterMapper;
        }

        public List<DeviceElementUse> Map(ISOTime time,
                                          IEnumerable<ISOSpatialRow> isoRecords,
                                          int operationDataId,
                                          IEnumerable<string> isoDeviceElementIDs,
                                          Dictionary<string, List<ISOProductAllocation>> isoProductAllocations)
        {
            var usedDataLogValues = new List<ISODataLogValue>();

            foreach (string isoDeviceElementID in isoDeviceElementIDs)
            {
                DeviceHierarchyElement hierarchyElement = TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(isoDeviceElementID);
                int? adaptDeviceElementId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDeviceElementID);
                if (hierarchyElement != null &&
                    adaptDeviceElementId.HasValue &&
                    DataModel.Catalog.DeviceElements.Any(d => d.Id.ReferenceId == adaptDeviceElementId.Value))
                { 
                    usedDataLogValues.AddRange(_workingDataMapper.GetDataLogValuesForDeviceElement(time, hierarchyElement));
                }
            }

            List<ISOSpatialRow> isoRecordsWithData = new List<ISOSpatialRow>();
            if (usedDataLogValues.Any())
            {
                foreach (var isoRecord in isoRecords)
                {
                    int beforeCount = usedDataLogValues.Count;
                    var notReferencedDataLogValues = usedDataLogValues.Where(x =>
                        !isoRecord.SpatialValues.Any(y => y.DataLogValue.ProcessDataIntDDI == x.ProcessDataIntDDI &&
                                                          y.DataLogValue.DeviceElementIdRef.ReverseEquals(x.DeviceElementIdRef)));
                    usedDataLogValues = notReferencedDataLogValues.ToList();
                    if (beforeCount != usedDataLogValues.Count)
                    {
                        isoRecordsWithData.Add(isoRecord);
                    }
                    if (usedDataLogValues.Count == 0)
                    {
                        break;
                    }
                }
            }


            var sections = new List<DeviceElementUse>();
            foreach (string isoDeviceElementID in isoDeviceElementIDs)
            {
                DeviceHierarchyElement hierarchyElement = TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(isoDeviceElementID);
                if (hierarchyElement != null)
                {
                    //Get the relevant DeviceElementConfiguration
                    int? adaptDeviceElementId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDeviceElementID);
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

                        List<WorkingData> workingDatas = new List<WorkingData>();
                        DeviceElementUse deviceElementUse = sections.FirstOrDefault(d => d.DeviceConfigurationId == config.Id.ReferenceId);
                        if (deviceElementUse == null)
                        {
                            //Create the DeviceElementUse
                            deviceElementUse = new DeviceElementUse
                            {
                                Depth = depth,
                                Order = order,
                                OperationDataId = operationDataId,
                                DeviceConfigurationId = config.Id.ReferenceId
                            };
                        }
                        else
                        {
                            workingDatas = deviceElementUse.GetWorkingDatas().ToList();
                        }

                        //Add Working Data for any data on this device element
                        List<WorkingData> data = _workingDataMapper.Map(time, isoRecordsWithData, deviceElementUse, hierarchyElement, sections, isoProductAllocations);
                        if (data.Any())
                        {
                            workingDatas.AddRange(data);
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
            return sections.Select(x =>
            {
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
