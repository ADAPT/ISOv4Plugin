/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/


using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.Mappers.Manufacturers;
using AgGateway.ADAPT.ISOv4Plugin.Mappers;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class DeviceElementHierarchies
    {
        private readonly List<IError> _errors;

        public DeviceElementHierarchies(IEnumerable<ISODevice> devices,
                                        RepresentationMapper representationMapper,
                                        bool mergeBins,
                                        IEnumerable<ISOTimeLog> timeLogs,
                                        string dataPath,
                                        TaskDataMapper taskDataMapper)
        {
            Items = new Dictionary<string, DeviceHierarchyElement>();
            _errors = taskDataMapper.Errors;

            //Track any device element geometries not logged as a DPT
            Dictionary<string, List<string>> missingGeometryDefinitions = new Dictionary<string, List<string>>();

            var manufacturer = ManufacturerFactory.GetManufacturer(taskDataMapper);

            foreach (ISODevice device in devices)
            {
                ISODeviceElement rootDeviceElement = device.DeviceElements.SingleOrDefault(det => det.DeviceElementType == ISODeviceElementType.Device);
                if (rootDeviceElement != null)
                {
                    DeviceHierarchyElement hierarchyElement = new DeviceHierarchyElement(rootDeviceElement, 0, representationMapper, mergeBins, missingGeometryDefinitions);
                    hierarchyElement.HandleBinDeviceElements();
                    Items.Add(device.DeviceId, hierarchyElement);

                    manufacturer?.ProcessDeviceElementHierarchy(hierarchyElement, missingGeometryDefinitions);
                }
            }

            //Address the missing geometry data with targeted reads of the TLG binaries for any DPDs
            if (missingGeometryDefinitions.Any())
            {
                FillDPDGeometryDefinitions(missingGeometryDefinitions, timeLogs, dataPath, taskDataMapper.Version);
            }
        }

        public Dictionary<string, DeviceHierarchyElement> Items { get; set; }

        private Dictionary<string, DeviceHierarchyElement> _mainMatchingElements;
        private Dictionary<string, DeviceHierarchyElement> _mergedMatchingElements;

        public void CacheDeviceElementIds()
        {
            _mainMatchingElements = new Dictionary<string, DeviceHierarchyElement>();
            _mergedMatchingElements = new Dictionary<string, DeviceHierarchyElement>();
            foreach (DeviceHierarchyElement hierarchy in this.Items.Values)
            {
                hierarchy.CacheDeviceElementIds(_mainMatchingElements, _mergedMatchingElements);
                hierarchy.CacheDeviceElementIds();
            }
        }

        public DeviceHierarchyElement GetMatchingElement(string isoDeviceElementId, bool includeMergedElements = false)
        {
            if (_mainMatchingElements != null)
            {
                DeviceHierarchyElement el;
                if (_mainMatchingElements.TryGetValue(isoDeviceElementId, out el))
                {
                    return el;
                }

                if (includeMergedElements && _mergedMatchingElements.TryGetValue(isoDeviceElementId, out el))
                {
                    return el;
                }
            }
            else
            {
                foreach (DeviceHierarchyElement hierarchy in this.Items.Values)
                {
                    DeviceHierarchyElement foundModel = hierarchy.FromDeviceElementID(isoDeviceElementId, includeMergedElements);
                    if (foundModel != null)
                    {
                        return foundModel;
                    }
                }
            }
            return null;
        }

        public ISODeviceElement GetISODeviceElementFromID(string deviceElementID)
        {
            DeviceHierarchyElement hierarchyElement = GetMatchingElement(deviceElementID);
            if (hierarchyElement != null)
            {
                return hierarchyElement.DeviceElement;
            }
            return null;
        }

        /// <summary>
        /// Perform a targeted read of the Timelog binary files to obtain implement geometry details logged via DeviceProcessData
        /// </summary>
        /// <param name="missingDefinitions"></param>
        /// <param name="timeLogTimeElements"></param>
        /// <param name="taskDataPath"></param>
        /// <param name="allDeviceHierarchyElements"></param>
        public void FillDPDGeometryDefinitions(Dictionary<string, List<string>> missingDefinitions, IEnumerable<ISOTimeLog> timeLogs, string taskDataPath, int version)
        {
            Dictionary<string, int?> reportedValues = new Dictionary<string, int?>(); //DLV signature / value 
            foreach (ISOTimeLog timeLog in timeLogs)
            {
                ISOTime time = timeLog.GetTimeElement(taskDataPath);
                if (time != null &&
                    time.DataLogValues.Any(dlv => missingDefinitions.ContainsKey(dlv.DeviceElementIdRef)))
                {
                    List<ISODataLogValue> dlvsToRead = time.DataLogValues.Where(dlv => missingDefinitions.ContainsKey(dlv.DeviceElementIdRef) &&
                                                                                                         missingDefinitions[dlv.DeviceElementIdRef].Contains(dlv.ProcessDataDDI)).ToList();
                    foreach (string deviceElementID in missingDefinitions.Keys)
                    {
                        List<string> ddis = missingDefinitions[deviceElementID];
                        if (ddis.Any(d => d == "0046"))
                        {
                            //We used 0046 generically for a missing width.  Check for any 0044 or 0043
                            var defaultWidthDLV = time.DataLogValues.FirstOrDefault(gt => gt.ProcessDataDDI == "0044" && gt.DeviceElementIdRef == deviceElementID);
                            if (defaultWidthDLV != null)
                            {
                                dlvsToRead.Add(defaultWidthDLV);
                            }
                            else
                            {
                                var workingWidthDLV = time.DataLogValues.FirstOrDefault(gt => gt.ProcessDataDDI == "0043" && gt.DeviceElementIdRef == deviceElementID);
                                if (workingWidthDLV != null)
                                {
                                    dlvsToRead.Add(workingWidthDLV);
                                }
                            }
                        }
                    }

                    if (dlvsToRead.Any())
                    {
                        string binaryName = string.Concat(timeLog.Filename, ".bin");
                        string binaryPath = taskDataPath.GetDirectoryFiles(binaryName, SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (binaryPath != null)
                        {
                            Dictionary<byte, int> timelogValues = Mappers.TimeLogMapper.ReadImplementGeometryValues(dlvsToRead.Select(d => d.Index), time, binaryPath, version, _errors);

                            foreach (byte reportedDLVIndex in timelogValues.Keys)
                            {
                                ISODataLogValue reportedDLV = dlvsToRead.First(d => d.Index == reportedDLVIndex);
                                string dlvKey = DeviceHierarchyElement.GetDataLogValueKey(reportedDLV);
                                if (!reportedValues.ContainsKey(dlvKey))
                                {
                                    //First occurence of this DET and DDI in the timelogs
                                    //We take the max width/max (from 0) offset from the 1st timelog.  This matches existing functionality, and is workable for foreseeable cases where working width is the only dynamic value.
                                    //ISOXML supports changing widths and offsets dynamically throughout and across timelogs.    
                                    //Should max width and/or offset parameters change dynamically, data consumers will need to obtain this information via the OperationData.SpatialRecords as is commonly done with 0043 (working width) today
                                    //An alternative would be to enhance this logic to clone the entire DeviceModel hierarchy for each variation in offset value, should such data ever occur in the field.
                                    reportedValues.Add(dlvKey, timelogValues[reportedDLV.Index]);

                                    //Add to this element
                                    var matchingElement = GetMatchingElement(reportedDLV.DeviceElementIdRef);
                                    if (matchingElement != null)
                                    {
                                        switch (reportedDLV.ProcessDataDDI)
                                        {
                                            case "0046":
                                            case "0044":
                                            case "0043":
                                                if (matchingElement.Width == null || timelogValues[reportedDLV.Index] > matchingElement.Width)
                                                {
                                                    //If max 0043 is greater than 0046, then take max 0043
                                                    matchingElement.Width = timelogValues[reportedDLV.Index];
                                                    matchingElement.WidthDDI = reportedDLV.ProcessDataDDI;
                                                }
                                                break;
                                            case "0086":
                                                matchingElement.XOffset = timelogValues[reportedDLV.Index];
                                                break;
                                            case "0087":
                                                matchingElement.YOffset = timelogValues[reportedDLV.Index];
                                                break;
                                            case "0088":
                                                matchingElement.ZOffset = timelogValues[reportedDLV.Index];
                                                break;
                                            case "0252":
                                                matchingElement.OriginAxleLocation = timelogValues[reportedDLV.Index] == 1 || timelogValues[reportedDLV.Index] == 4
                                                    ? OriginAxleLocationEnum.Front : OriginAxleLocationEnum.Rear;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This utility class serves to map the hierarchies of ISO DeviceElements within a single ISO Device.   Each ISODeviceElement will have a hierarchy object that references its parents and children.
    /// Where the parent is null, it is the root device element in a device.
    /// </summary>
    public class DeviceHierarchyElement
    {
        public DeviceHierarchyElement(ISODeviceElement deviceElement,
                                      int depth,
                                      RepresentationMapper representationMapper,
                                      bool mergeSingleBinsIntoBoom,
                                      Dictionary<string, List<string>> missingGeometryDefinitions,
                                      HashSet<int> crawledElements = null,
                                      DeviceHierarchyElement parent = null)
        {
            RepresentationMapper = representationMapper;
            MergedElements = new List<ISODeviceElement>();
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
                ISODeviceProperty widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0046"); //Max width
                if (widthProperty != null)
                {
                    Width = widthProperty.Value;
                    WidthDDI = "0046";
                }
                else
                {
                    widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0044"); //Default working width
                    if (widthProperty != null)
                    {
                        Width = widthProperty.Value;
                        WidthDDI = "0044";
                    }

                    if (widthProperty == null)
                    {
                        widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0043"); //Actual working width
                        if (widthProperty != null)
                        {
                            Width = widthProperty.Value;
                            WidthDDI = "0043";
                        }
                    }
                }

                if (Width == null)
                {
                    //We are missing a width DPT.   Log it (0046 as a substitute here for any valid width) for possible retrieval from the TLG binaries.
                    AddMissingGeometryDefinition(missingGeometryDefinitions, deviceElement.DeviceElementId, "0046");
                }

                //Offsets
                ISODeviceProperty xOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0086");
                if (xOffsetProperty != null)
                {
                    XOffset = xOffsetProperty.Value;
                }
                else
                {
                    AddMissingGeometryDefinition(missingGeometryDefinitions, deviceElement.DeviceElementId, "0086");
                }

                ISODeviceProperty yOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0087");
                if (yOffsetProperty != null)
                {
                    YOffset = yOffsetProperty.Value;
                }
                else
                {
                    AddMissingGeometryDefinition(missingGeometryDefinitions, deviceElement.DeviceElementId, "0087");
                }

                ISODeviceProperty zOffsetProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0088");
                if (zOffsetProperty != null)
                {
                    ZOffset = zOffsetProperty.Value;
                }
                else
                {
                    AddMissingGeometryDefinition(missingGeometryDefinitions, deviceElement.DeviceElementId, "0088");
                }

                //Children
                IEnumerable<ISODeviceElement> childDeviceElements = deviceElement.Device.DeviceElements.Where(det => det.ParentObjectId == deviceElement.DeviceElementObjectId && det.ParentObjectId != det.DeviceElementObjectId); //Do not create children for an element classified as its own parent
                if (childDeviceElements.Any())
                {
                    int childDepth = depth + 1;
                    Children = new List<DeviceHierarchyElement>();
                    foreach (ISODeviceElement childDeviceElement in childDeviceElements)
                    {
                        //If there is a single bin child of the boom (usually alongside sections),
                        //we can logically combine the bin and boom to have a clean hierarchy
                        //where sections are the direct children of the element containing the rates.
                        //We currently use an import property (MergeSingleBinsIntoBoom) to enable this functionality.
                        //Note that if there are any duplicate DDIs on both the Bin and Boom (non-standard per Annex F.3),
                        //the FirstOrDefault() logic in the setter methods in the SpatialRecordMapper will prefer the Boom and suppress the Bin data.
                        if (mergeSingleBinsIntoBoom &&
                            (DeviceElement.DeviceElementType == ISODeviceElementType.Device || DeviceElement.DeviceElementType == ISODeviceElementType.Function) &&
                            childDeviceElement.DeviceElementType == ISODeviceElementType.Bin &&
                            childDeviceElements.Count(c => c.DeviceElementType == ISODeviceElementType.Bin) == 1)
                        {
                            //Set the element into the MergedElements list
                            MergedElements.Add(childDeviceElement);

                            //Set its children as children of the boom
                            foreach (ISODeviceElement binChild in childDeviceElement.ChildDeviceElements.Where(det => det.ParentObjectId == childDeviceElement.DeviceElementObjectId && det.ParentObjectId != det.DeviceElementObjectId))
                            {
                                Children.Add(new DeviceHierarchyElement(binChild, childDepth, representationMapper, mergeSingleBinsIntoBoom, missingGeometryDefinitions, _crawledElements, this));
                            }

                            //This functionality will not work in the ADAPT framework today for multiple bins on one boom (i.e., ISO 11783-10:2015(E) F.23 & F.33).
                            //For these, we will fall back to the more basic default functionality in HandleBinDeviceElements()
                            //where we separate bins and sections into different depths within the ADAPT device hierarchy.
                            //Plugin implementers will need to rationalize the separate bins to the single boom, 
                            //with the rate for each bin associated to the corresponding DeviceElement in the ADAPT model.
                            //Were this multi-bin/single boom DDOP common, we could perhaps extend the WorkingData(?) class with some new piece of information
                            //To differentiate like data elements from different bins and thereby extend the merge functionality to this case.
                        }
                        else if (DeviceElement.DeviceElementType == ISODeviceElementType.Device &&
                                 DeviceElement.ChildDeviceElements.Count(x => x.DeviceElementType == ISODeviceElementType.Function) > 1 &&
                                 GetChildElementWithYieldSensor(DeviceElement) != null &&
                                 GetChildElementWithMoistureSensor(DeviceElement) != null &&
                                 (GetChildElementWithYieldSensor(DeviceElement).DeviceElementId != GetChildElementWithMoistureSensor(DeviceElement).DeviceElementId)
                                )
                        {
                            //This is a Combine with yield and moisture data on different device elements 
                            //While a valid ISO11783-10 DDOP modeling approach, for ADAPT's purposes yield and moisture need to be considered together.
                            //Merge all the child functions onto the parent.
                            MergedElements.Add(childDeviceElement);
                        }
                        else
                        {
                            //Add the child device element
                            DeviceHierarchyElement child = new DeviceHierarchyElement(childDeviceElement, childDepth, representationMapper, mergeSingleBinsIntoBoom, missingGeometryDefinitions, _crawledElements, this);
                            Children.Add(child);
                        }
                    }
                }

                //Parent
                Parent = parent;
            }
        }

        private static void AddMissingGeometryDefinition(Dictionary<string, List<string>> missingDefinitions, string isoDeviceElementId, string geometryDDI)
        {
            if (missingDefinitions == null)
            {
                missingDefinitions = new Dictionary<string, List<string>>();
            }
            if (!missingDefinitions.ContainsKey(isoDeviceElementId))
            {
                missingDefinitions.Add(isoDeviceElementId, new List<string>());
            }
            missingDefinitions[isoDeviceElementId].Add(geometryDDI);
        }

        internal RepresentationMapper RepresentationMapper { get; set; }

        public ISODeviceElement DeviceElement { get; private set; }

        public int Depth { get; set; }
        public int Order { get; set; }
        public ISODeviceElementType Type { get; set; }
        private HashSet<int> _crawledElements;

        private Dictionary<string, DeviceHierarchyElement> _mainDeviceElementCache;
        private Dictionary<string, DeviceHierarchyElement> _mergedDeviceElementCache;

        /// <summary>
        /// Tracks any secondary DeviceElements that exist independently in the ISOXML
        /// but have been merged into another DeviceElement in the ADAPT model
        /// </summary>
        public List<ISODeviceElement> MergedElements { get; private set; }

        public string WidthDDI { get; set; }
        public int? Width { get; set; }
        public int? XOffset { get; set; }
        public int? YOffset { get; set; }
        public int? ZOffset { get; set; }

        public OriginAxleLocationEnum? OriginAxleLocation { get; set; }

        public NumericRepresentationValue WidthRepresentation { get { return Width.HasValue ? Width.Value.AsNumericRepresentationValue(WidthDDI, RepresentationMapper) : null; } }
        public NumericRepresentationValue XOffsetRepresentation { get { return XOffset.HasValue ? XOffset.Value.AsNumericRepresentationValue("0086", RepresentationMapper) : null; } }
        public NumericRepresentationValue YOffsetRepresentation { get { return YOffset.HasValue ? YOffset.Value.AsNumericRepresentationValue("0087", RepresentationMapper) : null; } }
        public NumericRepresentationValue ZOffsetRepresentation { get { return ZOffset.HasValue ? ZOffset.Value.AsNumericRepresentationValue("0088", RepresentationMapper) : null; } }

        public List<DeviceHierarchyElement> Children { get; set; }
        public DeviceHierarchyElement Parent { get; set; }

        public void CacheDeviceElementIds()
        {
            _mainDeviceElementCache = new Dictionary<string, DeviceHierarchyElement>();

            _mergedDeviceElementCache = new Dictionary<string, DeviceHierarchyElement>();

            CacheDeviceElementIds(_mainDeviceElementCache, _mergedDeviceElementCache);

            if (Children != null)
            {
                foreach (DeviceHierarchyElement child in Children)
                {
                    child.CacheDeviceElementIds();
                }
            }
        }

        public void CacheDeviceElementIds(Dictionary<string, DeviceHierarchyElement> mainDeviceElementCache,
            Dictionary<string, DeviceHierarchyElement> mergedDeviceElementCache)
        {
            if (DeviceElement != null)
            {
                if (!mainDeviceElementCache.TryGetValue(DeviceElement.DeviceElementId, out _))
                {
                    mainDeviceElementCache[DeviceElement.DeviceElementId] = this;
                }
            }
            foreach (ISODeviceElement element in MergedElements)
            {
                if (!mergedDeviceElementCache.TryGetValue(element.DeviceElementId, out _))
                {
                    mergedDeviceElementCache[element.DeviceElementId] = this;
                }
            }

            if (Children != null)
            {
                foreach (DeviceHierarchyElement child in Children)
                {
                    child.CacheDeviceElementIds(mainDeviceElementCache, mergedDeviceElementCache);
                }
            }
        }

        public DeviceHierarchyElement FromDeviceElementID(string deviceElementID, bool includeMergedElements = false)
        {
            if (_mainDeviceElementCache != null)
            {
                DeviceHierarchyElement el;
                if (_mainDeviceElementCache.TryGetValue(deviceElementID, out el))
                {
                    return el;
                }
                else if (includeMergedElements && _mergedDeviceElementCache.TryGetValue(deviceElementID, out el))
                {
                    return el;
                }
            }
            else
            {
                if (DeviceElement?.DeviceElementId == deviceElementID || (includeMergedElements && MergedElements.Any(x => x.DeviceElementId == deviceElementID)))
                {
                    return this;
                }
                else if (Children != null)
                {
                    foreach (DeviceHierarchyElement child in Children)
                    {
                        DeviceHierarchyElement childModel = child.FromDeviceElementID(deviceElementID, includeMergedElements);
                        if (childModel != null)
                        {
                            return childModel;
                        }
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
            IEnumerable<DeviceHierarchyElement> lowestLevelItems = GetElementsAtDepth(maxDepth);
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
                foreach (DeviceHierarchyElement child in Children)
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

        public IEnumerable<DeviceHierarchyElement> GetElementsAtDepth(int depth)
        {
            List<DeviceHierarchyElement> list = new List<DeviceHierarchyElement>();
            if (depth == 0)
            {
                return new List<DeviceHierarchyElement>() { this };
            }
            if (Children != null)
            {
                foreach (DeviceHierarchyElement child in Children)
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

        public DeviceHierarchyElement GetRootDeviceElementHierarchy()
        {
            DeviceHierarchyElement item = this;
            while (item.Parent != null)
            {
                item = item.Parent;
            }
            return item;
        }

        internal static string GetDataLogValueKey(ISODataLogValue dlv)
        {
            return string.Concat(dlv.DeviceElementIdRef, "_", dlv.ProcessDataDDI);
        }

        /// <summary>
        /// As-applied/planted data will often report rates on bin device elements.   These device elements are the geometrical equivalent of the parent boom for mapping purposes.
        /// They are modeled as sections in ADAPT so that we can detect individual products/rates from these different device elements (tanks).
        /// Since they fall at the same level in the hierarchy as true implement sections, we need to reorder the section depths so that anything below the boom
        /// that is not a bin is moved down one level.   As such, the bin will not effect the geometric modeling of individual sections from left to right.
        /// </summary>
        /// <param name="h"></param>
        public void HandleBinDeviceElements()
        {
            for (int i = 0; i <= GetMaxDepth(); i++)
            {
                if (GetElementsAtDepth(i).Any(d => d.DeviceElement.DeviceElementType == ISODeviceElementType.Bin) &&
                    GetElementsAtDepth(i).Any(d => d.DeviceElement.DeviceElementType != ISODeviceElementType.Bin))
                {
                    //There are both bin and non-bin elements at this depth

                    //Move everything deeper than this level down
                    for (int d = GetMaxDepth(); d > i; d--)
                    {
                        GetElementsAtDepth(d)
                            .ToList()
                            .ForEach(e => e.Depth++);
                    }

                    //Drop the non-bin elements at this level down to the new gap just created
                    GetElementsAtDepth(i)
                        .Where(e => e.DeviceElement.DeviceElementType != ISODeviceElementType.Bin)
                        .ToList()
                        .ForEach(x => x.Depth++);
                }
            }
        }

        private ISODeviceElement GetChildElementWithYieldSensor(ISODeviceElement parentElement)
        {
            return parentElement.ChildDeviceElements.FirstOrDefault(d => d.DeviceProcessDatas.Any(p => p.DDI == "0063"));
        }

        private ISODeviceElement GetChildElementWithMoistureSensor(ISODeviceElement parentElement)
        {
            return parentElement.ChildDeviceElements.FirstOrDefault(d => d.DeviceProcessDatas.Any(p => p.DDI == "0054" || p.DDI == "0057"));
        }
    }
}
