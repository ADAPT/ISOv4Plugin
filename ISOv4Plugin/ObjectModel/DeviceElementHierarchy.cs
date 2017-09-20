/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class DeviceHierarchy
    {
        public DeviceHierarchy(IEnumerable<ISODevice> devices)
        {
            DeviceElementHierarchies = new Dictionary<string, DeviceElementHierarchy>();
            foreach (ISODevice device in devices)
            {
                ISODeviceElement rootDeviceElement = device.DeviceElements.SingleOrDefault(det => det.DeviceElementType == ISODeviceElementType.Device);
                if (rootDeviceElement != null)
                {
                    DeviceElementHierarchies.Add(device.DeviceId, new DeviceElementHierarchy(rootDeviceElement, 0));
                }
            }
        }

        public Dictionary<string, DeviceElementHierarchy> DeviceElementHierarchies { get; set; }

        public DeviceElementHierarchy GetRelevantHierarchy(string isoDeviceElementId)
        {
            foreach (DeviceElementHierarchy hierarchy in this.DeviceElementHierarchies.Values)
            {
                DeviceElementHierarchy foundModel = hierarchy.GetModelByISODeviceElementID(isoDeviceElementId);
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
        public DeviceElementHierarchy(ISODeviceElement deviceElement, int depth, HashSet<int> crawledElements = null)
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
                Depth = depth;
                Order = deviceElement.DeviceElementNumber; //Reusing this number for now.

                //Device Property Assigned Widths & Offsets
                //Width
                ISODeviceProperty widthProperty = deviceElement.DeviceProperties.FirstOrDefault(dpt => dpt.DDI == "0046");
                if (widthProperty != null)
                {
                    Width = widthProperty.Value;
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
                    ZOffset = xOffsetProperty.Value;
                }

                //Children
                IEnumerable<ISODeviceElement> childDeviceElements = deviceElement.Device.DeviceElements.Where(det => det.ParentObjectId == deviceElement.DeviceElementObjectId);// && det.DeviceElementType == ISOEnumerations.ISODeviceElementType.Section);
                if (childDeviceElements.Any())
                {
                    int childDepth = depth + 1;
                    Children = new List<DeviceElementHierarchy>();
                    foreach (ISODeviceElement det in childDeviceElements)
                    {
                        DeviceElementHierarchy child = new DeviceElementHierarchy(det, childDepth, _crawledElements);
                        Children.Add(child);
                        //Width += child.Width;
                    }
                }
            }
        }

        public ISODeviceElement DeviceElement { get; private set; }

        public int Depth { get; set; }
        public int Order { get; set; }
        public ISODeviceElementType Type { get; set; }
        private HashSet<int> _crawledElements;

        public long? Width { get; set; }
        public long? XOffset { get; set; }
        public long? YOffset { get; set; }
        public long? ZOffset { get; set; }
        ////public long LeftExtreme { get { return YOffset - (Width / 2); } }  
        ////public long RightExtreme { get { return YOffset + (Width / 2); } }


        public List<DeviceElementHierarchy> Children { get; set; }

        public DeviceElementHierarchy GetModelByISODeviceElementID(string deviceElementID)
        {
            if (DeviceElement.DeviceElementId == deviceElementID)
            {
                return this;
            }
            else if (Children != null)
            {
                foreach (DeviceElementHierarchy child in Children)
                {
                    DeviceElementHierarchy childModel = child.GetModelByISODeviceElementID(deviceElementID);
                    if (childModel != null)
                    {
                        return childModel;
                    }
                }
            }
            return null;
        }

        public long? GetLowestLevelSectionWidth()
        {
            int maxDepth = GetMaxDepth();
            IEnumerable<DeviceElementHierarchy> lowestLevelItems = GetElementsAtDepth(maxDepth);
            if (lowestLevelItems.Any() && lowestLevelItems.All(i => i.Width.HasValue && i.Width.Value == lowestLevelItems.First().Width.Value))
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
    }


}
