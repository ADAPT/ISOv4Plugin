/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class DeviceHierarchy
    {
        public DeviceHierarchy(IEnumerable<ISODevice> devices, RepresentationMapper representationMapper)
        {
            DeviceElementHierarchies = new Dictionary<string, DeviceElementHierarchy>();
            foreach (ISODevice device in devices)
            {
                ISODeviceElement rootDeviceElement = device.DeviceElements.SingleOrDefault(det => det.DeviceElementType == ISODeviceElementType.Device);
                if (rootDeviceElement != null)
                {
                    DeviceElementHierarchies.Add(device.DeviceId, new DeviceElementHierarchy(device, rootDeviceElement, 0, representationMapper));
                }
            }
        }

        public Dictionary<string, DeviceElementHierarchy> DeviceElementHierarchies { get; set; }

        public DeviceElementHierarchy GetRelevantHierarchy(string isoDeviceElementId)
        {
            foreach (DeviceElementHierarchy hierarchy in this.DeviceElementHierarchies.Values)
            {
                DeviceElementHierarchy foundModel = hierarchy.FromDeviceElementID(isoDeviceElementId);
                if (foundModel != null)
                {
                    return foundModel;
                }
            }
            return null;
        }

        public ISODeviceElement GetISODeviceElementFromID(string deviceElementID)
        {
            DeviceElementHierarchy hierarchy = GetRelevantHierarchy(deviceElementID);
            return hierarchy.DeviceElement;
        }
    }

    public class DeviceElementHierarchy
    {
        public DeviceElementHierarchy(ISODevice device, ISODeviceElement deviceElement, int depth, RepresentationMapper representationMapper, HashSet<int> crawledElements = null)
        {
            //This Hashset will track that we don't build infinite hierarchies.   
            //The plugin does not support peer control at this time.
            _crawledElements = crawledElements;
            if (_crawledElements == null)
            {
                _crawledElements = new HashSet<int>();
            }

            if (_crawledElements.Add(deviceElement.DeviceElementObjectId))
            {
                Type = deviceElement.DeviceElementType;
                DeviceElement = deviceElement;
                Device = device;
                Depth = depth;
                Order = deviceElement.DeviceElementNumber; //Reusing this number for now.

                //DeviceProperty assigned Widths & Offsets
                //DeviceProcessData assigned values will be assigned as the SectionMapper reads timelog data.

                //Width
                ISODeviceProperty widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0046");
                if (widthProperty != null)
                {
                    Width = widthProperty.Value.AsNumericRepresentationValue("0046", representationMapper);
                }

                //Offsets
                ISODeviceProperty xOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0086");
                if (xOffsetProperty != null)
                {
                    XOffset = xOffsetProperty.Value.AsNumericRepresentationValue("0086", representationMapper);
                }

                ISODeviceProperty yOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0087");
                if (yOffsetProperty != null)
                {
                    YOffset = yOffsetProperty.Value.AsNumericRepresentationValue("0087", representationMapper);
                }

                ISODeviceProperty zOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0088");
                if (zOffsetProperty != null)
                {
                    ZOffset = xOffsetProperty.Value.AsNumericRepresentationValue("0088", representationMapper);
                }

                //Children
                IEnumerable<ISODeviceElement> childDeviceElements = deviceElement.Device.DeviceElements.Where(det => det.ParentObjectId == deviceElement.DeviceElementObjectId);// && det.DeviceElementType == ISOEnumerations.ISODeviceElementType.Section);
                if (childDeviceElements.Any())
                {
                    int childDepth = depth + 1;
                    Children = new List<DeviceElementHierarchy>();
                    foreach (ISODeviceElement det in childDeviceElements)
                    {
                        DeviceElementHierarchy child = new DeviceElementHierarchy(device, det, childDepth, representationMapper, _crawledElements);
                        Children.Add(child);
                    }
                }
            }
        }

        public ISODeviceElement DeviceElement { get; private set; }
        public ISODevice Device { get; private set; }

        public int Depth { get; set; }
        public int Order { get; set; }
        public ISODeviceElementType Type { get; set; }
        private HashSet<int> _crawledElements;

        public NumericRepresentationValue Width { get; set; }
        public NumericRepresentationValue XOffset { get; set; }
        public NumericRepresentationValue YOffset { get; set; }
        public NumericRepresentationValue ZOffset { get; set; }

        public List<DeviceElementHierarchy> Children { get; set; }

        public DeviceElementHierarchy FromDeviceElementID(string deviceElementID)
        {
            if (DeviceElement.DeviceElementId == deviceElementID)
            {
                return this;
            }
            else if (Children != null)
            {
                foreach (DeviceElementHierarchy child in Children)
                {
                    DeviceElementHierarchy childModel = child.FromDeviceElementID(deviceElementID);
                    if (childModel != null)
                    {
                        return childModel;
                    }
                }
            }
            return null;
        }

        public NumericRepresentationValue GetLowestLevelSectionWidth()
        {
            int maxDepth = GetMaxDepth();
            IEnumerable<DeviceElementHierarchy> lowestLevelItems = GetElementsAtDepth(maxDepth);
            if (lowestLevelItems.Any() && lowestLevelItems.All(i => i.Width != null && i.Width.Value != null && i.Width.Value.Value == lowestLevelItems.First().Width.Value.Value))
            {
                return lowestLevelItems.First().Width;
            }
            else
            {
                return null;
            }
        }

        public int GetMaxDepth()
        {
            int maxDepth = 0;
            FindMaxDepth(ref maxDepth);
            return maxDepth;
        }

        private int FindMaxDepth(ref int maxDepth)
        {
            int depth = Depth;
            if (Children != null)
            {
                foreach (DeviceElementHierarchy child in Children)
                {
                    depth = child.FindMaxDepth(ref maxDepth);
                    if (depth > maxDepth)
                    {
                        maxDepth = depth;
                    }
                }
            }
            return depth;
        }

        public IEnumerable<DeviceElementHierarchy> GetElementsAtDepth(int depth)
        {
            List<DeviceElementHierarchy> list = new List<DeviceElementHierarchy>();
            if (depth == 0)
            {
                return new List<DeviceElementHierarchy>() { this };
            }
            if (Children != null)
            {
                foreach (DeviceElementHierarchy child in Children)
                {
                    if (child.Depth == depth)
                    {
                        list.Add(child);
                    }
                    else if (child.Depth < depth)
                    {
                        list.AddRange(child.GetElementsAtDepth(depth));
                    }

                }
            }
            return list;              
        }

        public void SetWidthsAndOffsetsFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, DeviceElementConfiguration config, RepresentationMapper representationMapper)
        {
            //Set values on this object and associated DeviceElementConfiguration 
            if (Width == null)
            {
                Width = GetWidthFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
            }

            if (config.Offsets == null)
            {
                config.Offsets = new List<NumericRepresentationValue>();
            }

            if (XOffset == null)
            {
                XOffset = GetXOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                if (XOffset != null)
                {
                    config.Offsets.Add(XOffset);
                }
            }

            if (YOffset == null)
            {
                YOffset = GetYOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                if (YOffset != null)
                {
                    config.Offsets.Add(YOffset);
                }
            }

            if (ZOffset == null)
            {
                ZOffset = GetZOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                if (ZOffset != null)
                {
                    config.Offsets.Add(ZOffset);
                }
            }

            //Update config values as appropriate
            if (config is SectionConfiguration)
            {
                SectionConfiguration sectionConfig = config as SectionConfiguration;
                if (sectionConfig.SectionWidth == null)
                {
                    sectionConfig.SectionWidth = Width;
                }
                if (sectionConfig.InlineOffset == null)
                {
                    sectionConfig.InlineOffset = XOffset;
                }
                if (sectionConfig.LateralOffset == null)
                {
                    sectionConfig.LateralOffset = YOffset;
                }
            }
            else if (config is ImplementConfiguration)
            {
                ImplementConfiguration implementConfig = config as ImplementConfiguration;
                if (implementConfig.Width == null)
                {
                    implementConfig.Width = Width;
                }
                if (implementConfig.YOffset == null)
                {
                    implementConfig.YOffset = YOffset;
                }
            }
            else if (config is MachineConfiguration)
            {
                MachineConfiguration machineConfig = config as MachineConfiguration;
                if (machineConfig.GpsReceiverXOffset == null)
                {
                    machineConfig.GpsReceiverXOffset = XOffset;
                }
                if (machineConfig.GpsReceiverYOffset == null)
                {
                    machineConfig.GpsReceiverYOffset = YOffset;
                }
                if (machineConfig.GpsReceiverZOffset == null)
                {
                    machineConfig.GpsReceiverZOffset = ZOffset;
                }
            }
        }

        private NumericRepresentationValue GetWidthFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
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
                return ((long)maxWidth).AsNumericRepresentationValue(ddi, representationMapper);
            }
            else
            {
                return null;
            }
        }

        private NumericRepresentationValue GetYOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
        {
            double offset = 0d;
            ISOSpatialRow firstYOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0087"));
            if (firstYOffset != null)
            {
                offset = firstYOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0087").Value;
                int ddi = "0087".AsInt32DDI();
                return ((long)offset).AsNumericRepresentationValue(ddi, representationMapper);
            }
            else
            {
                return null;
            }
        }

        private NumericRepresentationValue GetXOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
        {
            double offset = 0d;
            ISOSpatialRow firstXOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0086"));
            if (firstXOffset != null)
            {
                offset = firstXOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0086").Value;
                int ddi = "0086".AsInt32DDI();
                return ((long)offset).AsNumericRepresentationValue(ddi, representationMapper);
            }
            else
            {
                return null;
            }
        }

        private NumericRepresentationValue GetZOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
        {
            double offset = 0d;
            ISOSpatialRow firstZOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0088"));
            if (firstZOffset != null)
            {
                offset = firstZOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0088").Value;
                int ddi = "0088".AsInt32DDI();
                return ((long)offset).AsNumericRepresentationValue(ddi, representationMapper);
            }
            else
            {
                return null;
            }
        }
    }
}
