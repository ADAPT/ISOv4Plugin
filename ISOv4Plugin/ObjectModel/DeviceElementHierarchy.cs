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
    public class DeviceElementHierarchies
    {
        public DeviceElementHierarchies(IEnumerable<ISODevice> devices, RepresentationMapper representationMapper)
        {
            Items = new Dictionary<string, DeviceElementHierarchy>();
            foreach (ISODevice device in devices)
            {
                ISODeviceElement rootDeviceElement = device.DeviceElements.SingleOrDefault(det => det.DeviceElementType == ISODeviceElementType.Device);
                if (rootDeviceElement != null)
                {
                    DeviceElementHierarchy hierarchy = new DeviceElementHierarchy(rootDeviceElement, 0, representationMapper);
                    DeviceElementHierarchy.HandleBinDeviceElements(hierarchy);
                    Items.Add(device.DeviceId, hierarchy);
                }
            }
        }

        public Dictionary<string, DeviceElementHierarchy> Items { get; set; }

        public DeviceElementHierarchy GetRelevantHierarchy(string isoDeviceElementId)
        {
            foreach (DeviceElementHierarchy hierarchy in this.Items.Values)
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
            if (hierarchy != null)
            {
                return hierarchy.DeviceElement;
            }
            return null;
        }
    }

    /// <summary>
    /// This utility class serves to map the hierarchies of ISO DeviceElements within a single ISO Device.   Each ISODeviceElement will have a hierarchy object that references its parents and children.
    /// Where the parent is null, it is the root device element in a device.
    /// </summary>
    public class DeviceElementHierarchy
    {
        public DeviceElementHierarchy(ISODeviceElement deviceElement, int depth, RepresentationMapper representationMapper, HashSet<int> crawledElements = null, DeviceElementHierarchy parent = null)
        {
            RepresentationMapper = representationMapper;
            //This Hashset will track that we don't build infinite hierarchies.   
            //The plugin does not support peer control at this time.
            _crawledElements = crawledElements;
            if (_crawledElements == null)
            {
                _crawledElements = new HashSet<int>();
            }

            if (_crawledElements.Add((int)deviceElement.DeviceElementObjectId))
            {
                Type = deviceElement.DeviceElementType;
                DeviceElement = deviceElement;
                Depth = depth;
                Order = (int)deviceElement.DeviceElementNumber; //Using this number as analagous to this purpose.  The ISO spec requires these numbers increment from left to right as in ADAPT.  (See ISO11783-10:2015(E) B.3.2 Element number)

                //DeviceProperty assigned Widths & Offsets
                //DeviceProcessData assigned values will be assigned as the SectionMapper reads timelog data.

                //Width
                ISODeviceProperty widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0046");
                if (widthProperty != null)
                {
                    Width = widthProperty.Value;
                    WidthDDI = "0046";
                }
                else
                {
                    widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0043");
                    if (widthProperty != null)
                    {
                        Width = widthProperty.Value;
                        WidthDDI = "0043";
                    }
                }

                //Offsets
                ISODeviceProperty xOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0086");
                if (xOffsetProperty != null)
                {
                    XOffset = xOffsetProperty.Value;
                }

                ISODeviceProperty yOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0087");
                if (yOffsetProperty != null)
                {
                    YOffset = yOffsetProperty.Value;
                }

                ISODeviceProperty zOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0088");
                if (zOffsetProperty != null)
                {
                    ZOffset = zOffsetProperty.Value;
                }

                //Children
                IEnumerable<ISODeviceElement> childDeviceElements = deviceElement.Device.DeviceElements.Where(det => det.ParentObjectId == deviceElement.DeviceElementObjectId && det.ParentObjectId != det.DeviceElementObjectId); //Do not create children for an element classified as its own parent
                if (childDeviceElements.Any())
                {
                    int childDepth = depth + 1;
                    Children = new List<DeviceElementHierarchy>();
                    foreach (ISODeviceElement det in childDeviceElements)
                    {
                        DeviceElementHierarchy child = new DeviceElementHierarchy(det, childDepth, representationMapper, _crawledElements, this);
                        Children.Add(child);
                    }
                }

                //Parent
                Parent = parent;
            }
        }
        internal RepresentationMapper RepresentationMapper { get; set; }

        public ISODeviceElement DeviceElement { get; private set; }

        public int Depth { get; set; }
        public int Order { get; set; }
        public ISODeviceElementType Type { get; set; }
        private HashSet<int> _crawledElements;

        public string WidthDDI { get; set; }
        public int? Width { get; set; }
        public int? XOffset { get; set; }
        public int? YOffset { get; set; }
        public int? ZOffset { get; set; }

        public NumericRepresentationValue WidthRepresentation { get { return Width.HasValue ? Width.Value.AsNumericRepresentationValue(WidthDDI, RepresentationMapper) : null; } }
        public NumericRepresentationValue XOffsetRepresentation { get { return XOffset.HasValue ? XOffset.Value.AsNumericRepresentationValue("0086", RepresentationMapper) : null; } }
        public NumericRepresentationValue YOffsetRepresentation { get { return YOffset.HasValue ? YOffset.Value.AsNumericRepresentationValue("0087", RepresentationMapper) : null; } }
        public NumericRepresentationValue ZOffsetRepresentation { get { return ZOffset.HasValue ? ZOffset.Value.AsNumericRepresentationValue("0088", RepresentationMapper) : null; } }

        public List<DeviceElementHierarchy> Children { get; set; }
        public DeviceElementHierarchy Parent { get; set; }


        public DeviceElementHierarchy FromDeviceElementID(string deviceElementID)
        {
            if (DeviceElement?.DeviceElementId == deviceElementID)
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

        public IEnumerable<ISODeviceElement> AllDescendants
        {
            get
            {
                List<ISODeviceElement> output = new List<ISODeviceElement>();
                output.Add(DeviceElement);
                if (Children != null && Children.Any())
                {
                    Children.ForEach(i => output.AddRange(i.AllDescendants));
                }
                return output;
            }
        }

        public NumericRepresentationValue GetLowestLevelSectionWidth()
        {
            int maxDepth = GetMaxDepth();
            IEnumerable<DeviceElementHierarchy> lowestLevelItems = GetElementsAtDepth(maxDepth);
            if (lowestLevelItems.Any() && lowestLevelItems.All(i => i.WidthRepresentation != null && i.WidthRepresentation.Value != null && i.WidthRepresentation.Value.Value == lowestLevelItems.First().WidthRepresentation.Value.Value))
            {
                return lowestLevelItems.First().WidthRepresentation;
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

        public DeviceElementHierarchy GetRootDeviceElementHierarchy()
        {
            DeviceElementHierarchy item = this;
            while (item.Parent != null)
            {
                item = item.Parent;
            }
            return item;
        }
        public void SetWidthsAndOffsetsFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, HitchPoint hitchPoint, RepresentationMapper representationMapper)
        {
            if (hitchPoint.ReferencePoint == null)
            {
                hitchPoint.ReferencePoint = new ReferencePoint();
            }

            if (XOffset == null)
            {
                XOffset = GetXOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                hitchPoint.ReferencePoint.XOffset = XOffsetRepresentation;
            }

            if (YOffset == null)
            {
                YOffset = GetYOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                hitchPoint.ReferencePoint.YOffset = YOffsetRepresentation;
            }

            if (ZOffset == null)
            {
                ZOffset = GetZOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                hitchPoint.ReferencePoint.ZOffset = ZOffsetRepresentation;
            }
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
                if (XOffsetRepresentation != null)
                {
                    config.Offsets.Add(XOffsetRepresentation);
                }
            }

            if (YOffset == null)
            {
                YOffset = GetYOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                if (YOffsetRepresentation != null)
                {
                    config.Offsets.Add(YOffsetRepresentation);
                }
            }

            if (ZOffset == null)
            {
                ZOffset = GetZOffsetFromSpatialData(isoRecords, DeviceElement.DeviceElementId, representationMapper);
                if (ZOffsetRepresentation != null)
                {
                    config.Offsets.Add(ZOffsetRepresentation);
                }
            }

            //Update config values as appropriate
            if (this.DeviceElement.DeviceElementType == ISODeviceElementType.Navigation &&
                config is MachineConfiguration machineConfig)
            {
                    if (machineConfig.GpsReceiverXOffset == null)
                    {
                        machineConfig.GpsReceiverXOffset = XOffsetRepresentation;
                    }
                    if (machineConfig.GpsReceiverYOffset == null)
                    {
                        machineConfig.GpsReceiverYOffset = YOffsetRepresentation;
                    }
                    if (machineConfig.GpsReceiverZOffset == null)
                    {
                        machineConfig.GpsReceiverZOffset = ZOffsetRepresentation;
                    }
            }
            else
            {
                if (config is SectionConfiguration)
                {
                    SectionConfiguration sectionConfig = config as SectionConfiguration;
                    if (sectionConfig.SectionWidth == null)
                    {
                        sectionConfig.SectionWidth = WidthRepresentation;
                    }
                    if (sectionConfig.InlineOffset == null)
                    {
                        sectionConfig.InlineOffset = XOffsetRepresentation;
                    }
                    if (sectionConfig.LateralOffset == null)
                    {
                        sectionConfig.LateralOffset = YOffsetRepresentation;
                    }
                }
                else if (config is ImplementConfiguration)
                {
                    ImplementConfiguration implementConfig = config as ImplementConfiguration;
                    if (implementConfig.Width == null)
                    {
                        implementConfig.Width = WidthRepresentation;
                    }
                }
            }
        }

        private int? GetWidthFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
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
                WidthDDI = updatedWidthDDI;
                return (int)maxWidth;
            }
            else
            {
                return null;
            }
        }

        private int? GetYOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
        {
            double offset = 0d;
            ISOSpatialRow firstYOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0087"));
            if (firstYOffset != null)
            {
                offset = firstYOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0087").Value;
                return (int)offset;
            }
            else
            {
                return null;
            }
        }

        private int? GetXOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
        {
            double offset = 0d;
            ISOSpatialRow firstXOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0086"));
            if (firstXOffset != null)
            {
                offset = firstXOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0086").Value;
                return (int)offset;
            }
            else
            {
                return null;
            }
        }

        private int? GetZOffsetFromSpatialData(IEnumerable<ISOSpatialRow> isoRecords, string isoDeviceElementID, RepresentationMapper representationMapper)
        {
            double offset = 0d;
            ISOSpatialRow firstZOffset = isoRecords.FirstOrDefault(r => r.SpatialValues.Any(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID &&
                                                                                                    s.DataLogValue.ProcessDataDDI == "0088"));
            if (firstZOffset != null)
            {
                offset = firstZOffset.SpatialValues.Single(s => s.DataLogValue.DeviceElementIdRef == isoDeviceElementID && s.DataLogValue.ProcessDataDDI == "0088").Value;
                return (int)offset;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Seeder/Drill data will often report rates on bin device elements.   These device elements are the geometrical equivalent of the parent boom for mapping purposes.
        /// They are modeled as sections in ADAPT so that we can detect individual products/rates from these different device elements (tanks).
        /// Since they fall at the same level in the hierarchy as true implement sections, we need to reorder the section depths so that anything below the boom
        /// that is not a bin is moved down one level.   As such, the bin will not effect the geometric modeling of individual sections from left to right.
        /// </summary>
        /// <param name="h"></param>
        public static void HandleBinDeviceElements(DeviceElementHierarchy h)
        {
            for (int i = 0; i <= h.GetMaxDepth(); i++)
            {
                if (h.GetElementsAtDepth(i).Any(d => d.DeviceElement.DeviceElementType == ISODeviceElementType.Bin) &&
                    h.GetElementsAtDepth(i).Any(d => d.DeviceElement.DeviceElementType != ISODeviceElementType.Bin))
                {
                    //There are both bin and non-bin elements at this depth

                    //Move everything deeper than this level down
                    for (int d = h.GetMaxDepth(); d > i; d--)
                    {
                        h.GetElementsAtDepth(d)
                            .ToList()
                            .ForEach(e => e.Depth++);
                    }

                    //Drop the non-bin elements at this level down to the new gap just created
                    h.GetElementsAtDepth(i)
                        .Where(e => e.DeviceElement.DeviceElementType != ISODeviceElementType.Bin)
                        .ToList()
                        .ForEach(x => x.Depth++);
                }
            }
        }
    }  
}
