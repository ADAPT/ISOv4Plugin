using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System;
using AgGateway.ADAPT.ApplicationDataModel.Representations;

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
                DeviceElementHierarchy hierarchy = TaskDataMapper.DeviceHierarchy.GetRelevantHierarchy(isoDeviceElementID);
                if (hierarchy != null)
                {
                    //Find DeviceConfigurationID
                    int adaptDeviceElementId = TaskDataMapper.ADAPTIdMap[isoDeviceElementID].Value;
                    int adaptDeviceConfigurationId = 0;
                    DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.SingleOrDefault(c => c.DeviceElementId == adaptDeviceElementId);
                    if (config != null)
                    {
                        //Set the DeviceElementConfiguration ID 
                        adaptDeviceConfigurationId = config.Id.ReferenceId;

                        //Update Width as appropriate
                        if (config is SectionConfiguration)
                        {
                            SectionConfiguration sectionConfig = config as SectionConfiguration;
                            if (sectionConfig.SectionWidth == null || sectionConfig.SectionWidth.Value == null || sectionConfig.SectionWidth.Value.Value == 0d)
                            {
                                sectionConfig.SectionWidth = GetWidthFromSpatialData(isoRecords, isoDeviceElementID);
                            }
                            if (sectionConfig.InlineOffset == null || sectionConfig.InlineOffset.Value == null || sectionConfig.InlineOffset.Value.Value == 0d)
                            {
                                sectionConfig.InlineOffset = GetXOffsetFromSpatialData(isoRecords, isoDeviceElementID);
                            }
                            if (sectionConfig.LateralOffset == null || sectionConfig.LateralOffset.Value == null || sectionConfig.LateralOffset.Value.Value == 0d)
                            {
                                sectionConfig.LateralOffset = GetYOffsetFromSpatialData(isoRecords, isoDeviceElementID);
                            }
                        }
                        else if (config is ImplementConfiguration)
                        {
                            ImplementConfiguration implementConfig = config as ImplementConfiguration;
                            if (implementConfig.Width == null || implementConfig.Width.Value == null || implementConfig.Width.Value.Value == 0d)
                            {
                                implementConfig.Width = GetWidthFromSpatialData(isoRecords, isoDeviceElementID);
                            }
                            if (implementConfig.YOffset == null || implementConfig.YOffset.Value == null || implementConfig.YOffset.Value.Value == 0d)
                            {
                                implementConfig.YOffset = GetYOffsetFromSpatialData(isoRecords, isoDeviceElementID);
                            }
                        }
                    }

                    //Create the DeviceElementUse
                    var deviceElementUse = new DeviceElementUse();
                    deviceElementUse.Depth = hierarchy.Depth;
                    deviceElementUse.Order = hierarchy.Order; //TODO this may have repeated order ids.
                    deviceElementUse.OperationDataId = operationDataId;
                    deviceElementUse.DeviceConfigurationId = adaptDeviceConfigurationId;

                    var meters = _workingDataMapper.Map(time, isoRecords, deviceElementUse, isoDeviceElementID, sections);
                    deviceElementUse.GetWorkingDatas = () => meters;

                    sections.Add(deviceElementUse);
                }
            }

            return sections;
        }

        private NumericRepresentationValue GetWidthFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID)
        {
            double maxWidth = 0d;
            string updatedWidthDDI = null;
            ISOSpatialRow rowWithMaxWidth = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0046"));
            if (rowWithMaxWidth != null)
            {
                maxWidth = rowWithMaxWidth.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID && s.DataLogValue.ProcessDataDDI == "0046").Value;
                updatedWidthDDI = "0046";
            }
            else
            {
                //Find the largest working width
                IEnumerable<ISOSpatialRow> rows = isoRecords.Where(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                 s.DataLogValue.ProcessDataDDI == "0043"));
                if (rows.Any())
                {
                    foreach (ISOSpatialRow row in rows)
                    {
                        double value = row.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID && s.DataLogValue.ProcessDataDDI == "0043").Value;
                        if (value > maxWidth)
                        {
                            maxWidth = value;
                        }
                    }
                    updatedWidthDDI = "0043";
                }
            }

            if (updatedWidthDDI != null)
            {
                int ddi = updatedWidthDDI.AsInt32DDI();
                return ((long)maxWidth).AsNumericRepresentationValue(ddi, RepresentationMapper);
            }
            else
            {
                return null;
            }
        }

        private NumericRepresentationValue GetYOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID)
        {
            double offset = 0d;
            ISOSpatialRow firstYOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0087"));
            if (firstYOffset != null)
            {
                offset = firstYOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0087").Value;
                int ddi = "0087".AsInt32DDI();
                return ((long)offset).AsNumericRepresentationValue(ddi, RepresentationMapper);
            }
            else
            {
                return null;
            }
        }

        private NumericRepresentationValue GetXOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID)
        {
            double offset = 0d;
            ISOSpatialRow firstXOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0086"));
            if (firstXOffset != null)
            {
                offset = firstXOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0086").Value;
                int ddi = "0086".AsInt32DDI();
                return ((long)offset).AsNumericRepresentationValue(ddi, RepresentationMapper);
            }
            else
            {
                return null;
            }
        }

        public List<DeviceElementUse> ConvertToBaseTypes(List<DeviceElementUse> sections)
        {
            return sections.Select(x => {
                var section = new DeviceElementUse();
                var meters = x.GetWorkingDatas().Select(y => _workingDataMapper.ConvertToBaseType(y)).ToList();
                section.GetWorkingDatas = () => meters;
                return section;
                }).ToList();
        }

    }
}
