/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IDeviceElementMapper
    {
        IEnumerable<ISODeviceElement> ExportDeviceElements(IEnumerable<DeviceElement> adaptDeviceElements, ISODevice isoDevice);
        ISODeviceElement ExportDeviceElement(DeviceElement adaptDeviceElement, ISODevice isoDevice, List<ISODeviceElement> pendingDeviceElements, ref int objectID, ref int deviceElementNumber);
        IEnumerable<DeviceElement> ImportDeviceElements(ISODevice isoDevice);
        DeviceElement ImportDeviceElement(ISODeviceElement isoDeviceElement, EnumeratedValue deviceClassification);
    }

    public class DeviceElementMapper : BaseMapper, IDeviceElementMapper
    {
        public DeviceElementMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "DET")
        {
        }

        #region Export
        private int _devicePropertyObjectID = 0;
        public IEnumerable<ISODeviceElement> ExportDeviceElements(IEnumerable<DeviceElement> adaptRootDeviceElements, ISODevice isoDevice)
        {
            List<ISODeviceElement> isoDeviceElements = new List<ISODeviceElement>();
            DeviceElement rootElement = null;
            if (adaptRootDeviceElements.Count() == 1)
            {
                rootElement = adaptRootDeviceElements.Single();
            }
            else
            {
                //Create a single root device element to align with the ISO requirement
                rootElement = new DeviceElement();
                rootElement.Description = isoDevice.DeviceDesignator;
                List<DeviceElement> clonedRootElements = new List<DeviceElement>();
                foreach (DeviceElement element in adaptRootDeviceElements)
                {
                    DeviceElement clone = new DeviceElement();
                    clone.BrandId = element.BrandId;
                    clone.ContextItems = element.ContextItems;
                    clone.Description = element.Description;
                    clone.DeviceClassification = element.DeviceClassification;
                    clone.DeviceElementType = element.DeviceElementType;
                    clone.DeviceModelId = element.DeviceModelId;
                    clone.Id.ReferenceId = element.Id.ReferenceId;
                    clone.Id.UniqueIds = element.Id.UniqueIds;
                    clone.ManufacturerId = element.ManufacturerId;
                    clone.ParentDeviceId = rootElement.Id.ReferenceId; //Assign the clone to the new single root
                    clone.SerialNumber = element.SerialNumber;
                    clone.SeriesId = element.SeriesId;
                }
            }

            int objectID = 1;
            int deviceElementNumber = 1;
            ExportDeviceElement(rootElement, isoDevice, isoDeviceElements, ref objectID, ref deviceElementNumber);

            return isoDeviceElements;
        }

        public ISODeviceElement ExportDeviceElement(DeviceElement adaptDeviceElement, ISODevice isoDevice, List<ISODeviceElement> pendingDeviceElements, ref int objectID, ref int elementNumber)
        {
            ISODeviceElement det = new ISODeviceElement(isoDevice);

            objectID++;
            elementNumber++;

            //ID
            string id = adaptDeviceElement.Id.FindIsoId() ?? GenerateId();
            det.DeviceElementId = id;
            ExportIDs(adaptDeviceElement.Id, id);
            ExportContextItems(adaptDeviceElement.ContextItems, id, "ADAPT_Context_Items:DeviceElement");

            //Object ID
            det.DeviceElementObjectId = (uint)objectID;

            //Device Element Number
            det.DeviceElementNumber = (uint)elementNumber;

            //Designator
            det.DeviceElementDesignator = adaptDeviceElement.Description;

            //Device Element Type
            switch (adaptDeviceElement.DeviceElementType)
            {
                case DeviceElementTypeEnum.Machine:
                case DeviceElementTypeEnum.Implement:
                    det.DeviceElementType = ISODeviceElementType.Device;
                    break;
                case DeviceElementTypeEnum.Bin:
                    det.DeviceElementType = ISODeviceElementType.Bin;
                    break;
                case DeviceElementTypeEnum.Function:
                    det.DeviceElementType = ISODeviceElementType.Function;
                    break;
                case DeviceElementTypeEnum.Section:
                    det.DeviceElementType = ISODeviceElementType.Section;
                    break;
                case DeviceElementTypeEnum.Unit:
                    det.DeviceElementType = ISODeviceElementType.Unit;
                    break;
            }

            //Parent ID
            DeviceElement parentDeviceElement = DataModel.Catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == adaptDeviceElement.ParentDeviceId);
            if (parentDeviceElement != null)
            {
                string deviceElementID = TaskDataMapper.InstanceIDMap.GetISOID(parentDeviceElement.Id.ReferenceId);
                if (pendingDeviceElements.Any(d => d.DeviceElementId == deviceElementID))
                {
                    det.ParentObjectId = pendingDeviceElements.First(d => d.DeviceElementId == deviceElementID).DeviceElementObjectId;
                }
            }
            else
            {
                DeviceModel parentDeviceModel = DataModel.Catalog.DeviceModels.FirstOrDefault(d => d.Id.ReferenceId == adaptDeviceElement.ParentDeviceId);
                if (parentDeviceModel != null)
                {
                    //Parent is Device
                    det.ParentObjectId = 0;
                }
            }
    
            //Add to the collection
            pendingDeviceElements.Add(det);

            //Child Elements
            int deviceElementNumber = 1;
            foreach (DeviceElement childElement in DataModel.Catalog.DeviceElements.Where(d => d.ParentDeviceId == adaptDeviceElement.Id.ReferenceId))
            {
                ExportDeviceElement(childElement, isoDevice, pendingDeviceElements, ref objectID, ref deviceElementNumber);
            }

            //Connectors
            if (det.DeviceElementType == ISODeviceElementType.Device)
            {
                //Connectors should only exist as children of the top level DeviceElement
                foreach (DeviceElementConfiguration config in DataModel.Catalog.DeviceElementConfigurations.Where(c => c.DeviceElementId == adaptDeviceElement.Id.ReferenceId))
                {
                    foreach (Connector connector in DataModel.Catalog.Connectors.Where(c => c.DeviceElementConfigurationId == config.Id.ReferenceId))
                    {
                        ExportConnectorElement(connector, det, pendingDeviceElements, ref objectID, ref deviceElementNumber);
                    }
                }
            }

            //Device Properties
            ExportDeviceProperties(det, adaptDeviceElement);

            return det;
        }

        private void ExportConnectorElement(Connector adaptConnector, ISODeviceElement parentElement, List<ISODeviceElement> pendingDeviceElements, ref int objectID, ref int deviceElementNumber)
        {
            objectID++;
            deviceElementNumber++;

            ISODeviceElement isoDeviceElement = new ISODeviceElement(parentElement.Device);
            isoDeviceElement.DeviceElementId = adaptConnector.Id.FindIsoId() ?? GenerateId();
            isoDeviceElement.DeviceElementDesignator = $"{parentElement.Device.DeviceDesignator}_Connector";
            isoDeviceElement.DeviceElementType = ISODeviceElementType.Connector;
            isoDeviceElement.ParentObjectId = parentElement.DeviceElementObjectId;
            isoDeviceElement.DeviceElementNumber = (uint)deviceElementNumber;
            isoDeviceElement.DeviceElementObjectId = (uint)objectID;
            pendingDeviceElements.Add(isoDeviceElement);

            TaskDataMapper.InstanceIDMap.Add(adaptConnector.Id.ReferenceId, isoDeviceElement.DeviceElementId);
        }

        private void ExportDeviceProperties(ISODeviceElement isoDeviceElement, DeviceElement adaptDeviceElement)
        {
            //Connectors
            if (isoDeviceElement.ChildDeviceElements != null)
            {
                foreach (ISODeviceElement connectorElement in isoDeviceElement.ChildDeviceElements.Where(d => d.DeviceElementType == ISODeviceElementType.Connector))
                {
                    int? connectorID = TaskDataMapper.InstanceIDMap.GetADAPTID(connectorElement.DeviceElementId);
                    if (connectorID.HasValue)
                    {
                        Connector connector = DataModel.Catalog.Connectors.First(c => c.Id.ReferenceId == connectorID.Value);
                        HitchPoint hitch = DataModel.Catalog.HitchPoints.FirstOrDefault(h => h.Id.ReferenceId == connector.HitchPointId);
                        if (hitch != null && hitch.ReferencePoint != null)
                        {
                            ExportDeviceProperty(connectorElement, hitch.ReferencePoint.XOffset, ++_devicePropertyObjectID);
                            ExportDeviceProperty(connectorElement, hitch.ReferencePoint.YOffset, ++_devicePropertyObjectID);
                            ExportDeviceProperty(connectorElement, hitch.ReferencePoint.ZOffset, ++_devicePropertyObjectID);
                        }
                    }                  
                }
            }

            //Device Element Widths & Offsets
            IEnumerable<DeviceElementConfiguration> configs = DataModel.Catalog.DeviceElementConfigurations.Where(c => c.DeviceElementId == adaptDeviceElement.Id.ReferenceId);
            foreach (DeviceElementConfiguration config in configs)
            {
                if (config is MachineConfiguration)
                {
                    MachineConfiguration machineConfig = config as MachineConfiguration;
                    ISODeviceElement navigationElement = isoDeviceElement.Device.DeviceElements.FirstOrDefault(d => d.DeviceElementType == ISODeviceElementType.Navigation);
                    if (navigationElement == null)
                    {
                        if (machineConfig.GpsReceiverXOffset != null)
                        {
                            ExportDeviceProperty(navigationElement, machineConfig.GpsReceiverXOffset, ++_devicePropertyObjectID);
                        }
                        if (machineConfig.GpsReceiverYOffset != null)
                        {
                            ExportDeviceProperty(navigationElement, machineConfig.GpsReceiverYOffset, ++_devicePropertyObjectID);
                        }
                        if (machineConfig.GpsReceiverZOffset != null)
                        {
                            ExportDeviceProperty(navigationElement, machineConfig.GpsReceiverZOffset, ++_devicePropertyObjectID);
                        }
                    }
                }
                else if (config is ImplementConfiguration)
                {
                    ImplementConfiguration implementConfig = config as ImplementConfiguration;
                    if (implementConfig.Width != null)
                    {
                        ExportDeviceProperty(isoDeviceElement, implementConfig.Width, ++_devicePropertyObjectID);
                    }
                    if (implementConfig.Offsets != null)
                    {
                        implementConfig.Offsets.ForEach(o => ExportDeviceProperty(isoDeviceElement, o, ++_devicePropertyObjectID));
                    }
                }
                else if (config is SectionConfiguration)
                {
                    SectionConfiguration sectionConfig = config as SectionConfiguration;
                    if (sectionConfig.InlineOffset != null)
                    {
                        ExportDeviceProperty(isoDeviceElement, sectionConfig.InlineOffset, ++_devicePropertyObjectID);
                    }
                    if (sectionConfig.LateralOffset != null)
                    {
                        ExportDeviceProperty(isoDeviceElement, sectionConfig.LateralOffset, ++_devicePropertyObjectID);
                    }
                    if (sectionConfig.SectionWidth != null)
                    {
                        ExportDeviceProperty(isoDeviceElement, sectionConfig.SectionWidth, ++_devicePropertyObjectID);
                    }
                }
            }
        }

        private void ExportDeviceProperty(ISODeviceElement deviceElement, NumericRepresentationValue representationValue, int objectID)
        {
            ISODeviceProperty dpt = new ISODeviceProperty();
            dpt.ObjectID = (uint)objectID;
            switch (representationValue.Representation.Code)
            {
                case "0043":
                case "0046":
                    dpt.DDI = representationValue.Representation.Code;
                    dpt.Designator = "Width";
                    dpt.Value = representationValue.AsIntViaMappedDDI(RepresentationMapper);
                    break;
                case "vrOffsetInline":
                    dpt.DDI = "0086";
                    dpt.Designator = "XOffset";
                    dpt.Value = representationValue.AsIntViaMappedDDI(RepresentationMapper);
                    break;
                case "vrOffsetLateral":
                    dpt.DDI = "0087";
                    dpt.Designator = "YOffset";
                    dpt.Value = representationValue.AsIntViaMappedDDI(RepresentationMapper);
                    break;
                case "vrOffsetVertical":
                    dpt.DDI = "0088";
                    dpt.Designator = "ZOffset";
                    dpt.Value = representationValue.AsIntViaMappedDDI(RepresentationMapper);
                    break;
            }

            if (!string.IsNullOrEmpty(dpt.DDI))
            {
                deviceElement.Device.DeviceProperties.Add(dpt);
                deviceElement.DeviceObjectReferences.Add(new ISODeviceObjectReference() { DeviceObjectId = (uint)objectID });
            }
        }

        #endregion Export 

        #region Import

        public IEnumerable<DeviceElement> ImportDeviceElements(ISODevice isoDevice)
        {
            EnumeratedValue deviceClassification = DecodeMachineInfo(isoDevice.ClientNAME);

            ISODeviceElement rootDeviceElement = isoDevice.DeviceElements.SingleOrDefault(det => det.DeviceElementType == ISODeviceElementType.Device);
            if (rootDeviceElement == null)
            {
                //Short circuit on invalid TaskData
                TaskDataMapper.AddError("Missing root DeviceElement.  Device will not be imported.", isoDevice.DeviceId.ToString(), "DeviceElementMapper");
                return null;
            }

            DeviceHierarchyElement rootDeviceElementHierarchy = TaskDataMapper.DeviceElementHierarchies.Items[isoDevice.DeviceId];

            //Import device elements
            List<DeviceElement> adaptDeviceElements = new List<DeviceElement>();

            //Import down the hierarchy to ensure integrity of parent references
            for (int i = 0; i <= rootDeviceElementHierarchy.GetMaxDepth(); i++)
            {
                IEnumerable<ISODeviceElement> isoDeviceElements = rootDeviceElementHierarchy.GetElementsAtDepth(i).Select(h => h.DeviceElement);
                foreach (ISODeviceElement isoDeviceElement in isoDeviceElements)
                {
                    if (isoDeviceElement.DeviceElementType != ISODeviceElementType.Connector)
                    {
                        DeviceElement adaptDeviceElement = ImportDeviceElement(isoDeviceElement, deviceClassification);
                        if (isoDeviceElement.DeviceElementType == ISODeviceElementType.Device)
                        {
                            //Setting the Device serial number on the root Device Element only
                            adaptDeviceElement.SerialNumber = isoDevice.DeviceSerialNumber;
                        }
                        adaptDeviceElements.Add(adaptDeviceElement);
                        DataModel.Catalog.DeviceElements.Add(adaptDeviceElement);
                    }
                    else
                    {
                        //Connectors are not represented as DeviceElements in ADAPT
                        AddConnector(rootDeviceElementHierarchy, isoDeviceElement);
                    }
                }
            }

            return adaptDeviceElements;
        }

        public DeviceElement ImportDeviceElement(ISODeviceElement isoDeviceElement, EnumeratedValue deviceClassification)
        {
            DeviceElement deviceElement = new DeviceElement();
            DeviceHierarchyElement deviceElementHierarchy = TaskDataMapper.DeviceElementHierarchies.GetMatchingElement(isoDeviceElement.DeviceElementId);
            //ID
            ImportIDs(deviceElement.Id, isoDeviceElement.DeviceElementId);
            deviceElement.ContextItems = ImportContextItems(isoDeviceElement.DeviceElementId, "ADAPT_Context_Items:DeviceElement", isoDeviceElement);

            //Device ID
            int? deviceModelId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDeviceElement.Device.DeviceId);
            if (deviceModelId.HasValue)
            {
                deviceElement.DeviceModelId = deviceModelId.Value;
            }

            //Description
            deviceElement.Description = isoDeviceElement.DeviceElementDesignator;

            //Classification
            deviceElement.DeviceClassification = deviceClassification;

            //Parent ID
            if (isoDeviceElement.Parent != null)
            {
                int? parentDeviceId = null;
                if (isoDeviceElement.ParentObjectId == isoDeviceElement.DeviceElementObjectId)
                {
                    //Element has listed itself as its own parent.   Do not include a parent on the adapt element as it will invalidate logic in the hierarchy creation.
                }
                else if (isoDeviceElement.Parent is ISODeviceElement)
                {
                    //Read the parent off of the DeviceElementHierarchy in case we merged the parent into another DeviceElement
                    parentDeviceId = TaskDataMapper.InstanceIDMap.GetADAPTID(deviceElementHierarchy.Parent.DeviceElement.DeviceElementId);
                }
                else if (isoDeviceElement.Parent is ISODevice parentDevice)
                {
                    parentDeviceId = TaskDataMapper.InstanceIDMap.GetADAPTID(parentDevice.DeviceId);
                }
                if (parentDeviceId.HasValue)
                {
                    deviceElement.ParentDeviceId = parentDeviceId.Value;
                }
            }

            //Device Element Type
            switch (isoDeviceElement.DeviceElementType)
            {
                case ISODeviceElementType.Device:  //This is the root device element
                    if (deviceClassification != null &&
                        deviceClassification.Value != null &&
                        TaskDataMapper.DeviceOperationTypes.First(d => d.MachineEnumerationMember.DomainTag == deviceClassification.Value.Code).HasMachineConfiguration)
                    {
                        //Device is a machine
                        deviceElement.DeviceElementType = DeviceElementTypeEnum.Machine;
                    }
                    else if (deviceElementHierarchy.Children != null &&
                             deviceElementHierarchy.Children.Any(d => d?.DeviceElement.DeviceElementType == ISODeviceElementType.Navigation) && //The Nav element should be a direct descendant of the root
                             (!deviceElementHierarchy.Children.Any(d => d?.DeviceElement.DeviceElementType == ISODeviceElementType.Section) && //If there are section or function elements, classify as an implement vs. a machine
                             !deviceElementHierarchy.Children.Any(d => d?.DeviceElement.DeviceElementType == ISODeviceElementType.Function)))
                    {
                        //Device is a machine
                        deviceElement.DeviceElementType = DeviceElementTypeEnum.Machine;
                    }
                    else
                    {
                        //Default: classify as an implement
                        deviceElement.DeviceElementType = DeviceElementTypeEnum.Implement;
                    }
                    break;
                case ISODeviceElementType.Bin:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Bin;
                    break;
                case ISODeviceElementType.Function:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Function;
                    break;
                case ISODeviceElementType.Section:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Section;
                    break;
                case ISODeviceElementType.Unit:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Unit;
                    break;
                case ISODeviceElementType.Navigation:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Function;
                    break;
            }


            if (HasGeometryInformation(deviceElementHierarchy)) 
            {
                //Geometry information is on DeviceProperty elements. 
                GetDeviceElementConfiguration(deviceElement, deviceElementHierarchy, DataModel.Catalog); //Add via the Get method to invoke business rules for configs
            }

            return deviceElement;
        }

        private bool HasGeometryInformation(DeviceHierarchyElement deviceHierarchyElement)
        {
            return deviceHierarchyElement.Width.HasValue || deviceHierarchyElement.XOffset.HasValue || deviceHierarchyElement.YOffset.HasValue || deviceHierarchyElement.ZOffset.HasValue;
        }

        public static DeviceElementConfiguration GetDeviceElementConfiguration(DeviceElement adaptDeviceElement, DeviceHierarchyElement isoHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            if (isoHierarchy.DeviceElement.DeviceElementType == ISOEnumerations.ISODeviceElementType.Connector ||
                 isoHierarchy.DeviceElement.DeviceElementType == ISOEnumerations.ISODeviceElementType.Navigation)
            {
                //Data belongs to the parent device element from the ISO element referenced
                //Connector and Navigation data may be stored in Timelog data, but Connectors are not DeviceElements in ADAPT.
                //The data refers to the parent implement, which must always be a Device DET per the ISO spec.
                DeviceElement parent = catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == adaptDeviceElement.ParentDeviceId);
                while (parent != null && (parent.DeviceElementType != DeviceElementTypeEnum.Machine && parent.DeviceElementType != DeviceElementTypeEnum.Implement))
                {
                    parent = catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == parent.ParentDeviceId);
                }
                if (parent == null)
                {
                    throw new ApplicationException($"Cannot identify Device for Navigation/Connector DeviceElement: {adaptDeviceElement.Description}.");
                }

                DeviceElementConfiguration parentConfig = catalog.DeviceElementConfigurations.FirstOrDefault(c => c.DeviceElementId == parent.Id.ReferenceId);
                if (parentConfig == null)
                {
                    DeviceElement parentElement = catalog.DeviceElements.Single(d => d.Id.ReferenceId == adaptDeviceElement.ParentDeviceId);
                    parentConfig = AddDeviceElementConfiguration(parentElement, isoHierarchy.Parent, catalog);
                }
                return parentConfig;
            }
            else
            {
                DeviceElementConfiguration deviceConfiguration = catalog.DeviceElementConfigurations.FirstOrDefault(c => c.DeviceElementId == adaptDeviceElement.Id.ReferenceId);
                if (deviceConfiguration == null)
                {
                    deviceConfiguration = AddDeviceElementConfiguration(adaptDeviceElement, isoHierarchy, catalog);
                }
                return deviceConfiguration;
            }
        }

        private static DeviceElementConfiguration AddDeviceElementConfiguration(DeviceElement adaptDeviceElement, DeviceHierarchyElement isoHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            switch (adaptDeviceElement.DeviceElementType)
            {
                case DeviceElementTypeEnum.Machine:
                    return AddMachineConfiguration(adaptDeviceElement, isoHierarchy, catalog);
                case DeviceElementTypeEnum.Implement:
                    return AddImplementConfiguration(adaptDeviceElement, isoHierarchy, catalog);
                case DeviceElementTypeEnum.Function:
                    if (isoHierarchy.Parent.DeviceElement.DeviceElementType == ISODeviceElementType.Function)
                    {
                        //Function is part of the implement.  I.e., TC-GEO example 9 / ISO 11783-10:2015(E) Figure F.24
                        return AddSectionConfiguration(adaptDeviceElement, isoHierarchy, catalog);
                    }
                    else
                    {                       
                        //Function is the entire implement
                        return AddImplementConfiguration(adaptDeviceElement, isoHierarchy, catalog);
                    }
                case DeviceElementTypeEnum.Section:
                case DeviceElementTypeEnum.Bin:
                case DeviceElementTypeEnum.Unit:
                    return AddSectionConfiguration(adaptDeviceElement, isoHierarchy, catalog);
                default:
                    return null;
            }
        }

        private static MachineConfiguration AddMachineConfiguration(DeviceElement adaptDeviceElement, DeviceHierarchyElement deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            MachineConfiguration machineConfig = new MachineConfiguration();

            //Description
            machineConfig.Description = $"{deviceHierarchy.DeviceElement.Device.DeviceDesignator} : {deviceHierarchy.DeviceElement.DeviceElementDesignator}";

            //Device Element ID
            machineConfig.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Offsets
            if (deviceHierarchy.XOffset.HasValue ||
                deviceHierarchy.YOffset.HasValue ||
                deviceHierarchy.ZOffset.HasValue)
            {
                machineConfig.Offsets = new List<NumericRepresentationValue>();
                if (deviceHierarchy.XOffset != null)
                {
                    machineConfig.Offsets.Add(deviceHierarchy.XOffsetRepresentation);
                }
                if (deviceHierarchy.YOffset != null)
                {
                    machineConfig.Offsets.Add(deviceHierarchy.YOffsetRepresentation);
                }
                if (deviceHierarchy.ZOffset != null)
                {
                    machineConfig.Offsets.Add(deviceHierarchy.ZOffsetRepresentation);
                }
            }

            //GPS Offsets
            if (deviceHierarchy.Children != null && deviceHierarchy.Children.Any(h=>h.DeviceElement!=null &&  h.DeviceElement.DeviceElementType == ISODeviceElementType.Navigation))
            {
                DeviceHierarchyElement navigation = (deviceHierarchy.Children.First(h => h.DeviceElement.DeviceElementType == ISODeviceElementType.Navigation));
                machineConfig.GpsReceiverXOffset = navigation.XOffsetRepresentation;
                machineConfig.GpsReceiverYOffset = navigation.YOffsetRepresentation;
                machineConfig.GpsReceiverZOffset = navigation.ZOffsetRepresentation;
            }
            

            catalog.DeviceElementConfigurations.Add(machineConfig);
            return machineConfig;
        }

        private static ImplementConfiguration AddImplementConfiguration(DeviceElement adaptDeviceElement, DeviceHierarchyElement deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            ImplementConfiguration implementConfig = new ImplementConfiguration();

            //Description
            implementConfig.Description = $"{deviceHierarchy.DeviceElement.Device.DeviceDesignator} : {deviceHierarchy.DeviceElement.DeviceElementDesignator}";

            //Device Element ID
            implementConfig.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Offsets
            implementConfig.Offsets = new List<NumericRepresentationValue>();
            if (deviceHierarchy.XOffsetRepresentation != null)
            {
                implementConfig.Offsets.Add(deviceHierarchy.XOffsetRepresentation);
            }
            if (deviceHierarchy.YOffsetRepresentation != null)
            {
                implementConfig.Offsets.Add(deviceHierarchy.YOffsetRepresentation);
            }
            if (deviceHierarchy.ZOffsetRepresentation != null)
            {
                implementConfig.Offsets.Add(deviceHierarchy.ZOffsetRepresentation);
            }

            //Total Width 
            if (deviceHierarchy.Width != null)
            {
                //In an earlier implementation, we used Width for an individual Row Width and Physical Width for the max width.
                //Going forward, we are adopting a more conventional definition that Width is any reported width of the implement, and
                //Physical width is a reported max width specifically.
                //Widths set from Timelog binaries will have already observed this convention as they did not contain the row width variation.
                implementConfig.Width = deviceHierarchy.WidthRepresentation;
                if (deviceHierarchy.WidthDDI == "0046")
                {
                    implementConfig.PhysicalWidth = deviceHierarchy.WidthRepresentation;
                }
            }
            
            catalog.DeviceElementConfigurations.Add(implementConfig);

            return implementConfig;
        }

        private static SectionConfiguration AddSectionConfiguration(DeviceElement adaptDeviceElement, DeviceHierarchyElement deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            SectionConfiguration sectionConfiguration = new SectionConfiguration();

            //Description
            sectionConfiguration.Description = $"{deviceHierarchy.DeviceElement.Device.DeviceDesignator} : {deviceHierarchy.DeviceElement.DeviceElementDesignator}";


            //Device Element ID
            sectionConfiguration.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            NumericRepresentationValue width = deviceHierarchy.WidthRepresentation;
            NumericRepresentationValue xOffset = deviceHierarchy.XOffsetRepresentation;
            NumericRepresentationValue yOffset = deviceHierarchy.YOffsetRepresentation;
            if (adaptDeviceElement.DeviceElementType == DeviceElementTypeEnum.Bin)
            {
                //A bin carries no geometry information and should inherit width from its parent and carry 0 offsets from its parent.
                width = deviceHierarchy.Parent.WidthRepresentation;
                xOffset = 0.AsNumericRepresentationValue("0086", deviceHierarchy.RepresentationMapper);
                yOffset = 0.AsNumericRepresentationValue("0087", deviceHierarchy.RepresentationMapper);
            }

            //Width & Offsets
            sectionConfiguration.SectionWidth = width;
            sectionConfiguration.Offsets = new List<NumericRepresentationValue>();
            if (xOffset != null)
            {
                sectionConfiguration.InlineOffset = xOffset;
                sectionConfiguration.Offsets.Add(xOffset);
            }
            if (yOffset != null)
            {
                sectionConfiguration.LateralOffset = yOffset;
                sectionConfiguration.Offsets.Add(yOffset);
            }

            catalog.DeviceElementConfigurations.Add(sectionConfiguration);
            return sectionConfiguration;
        }

        /// <summary>
        /// Adds a connector with a hitch at the given reference point, referencing the parent DeviceElement as the DeviceElementConfiguration
        /// </summary>
        /// <param name="rootDeviceHierarchy"></param>
        /// <param name="connectorDeviceElement"></param>
        /// <param name="deviceElement"></param>
        private void AddConnector(DeviceHierarchyElement rootDeviceHierarchy, ISODeviceElement connectorDeviceElement)
        {
            //Per the TC-GEO specification, the connector is always a child of the root device element.
            DeviceHierarchyElement hierarchyElement = rootDeviceHierarchy.FromDeviceElementID(connectorDeviceElement.DeviceElementId);
            if (hierarchyElement.Parent == rootDeviceHierarchy)
            {
                int? rootDeviceElementID = TaskDataMapper.InstanceIDMap.GetADAPTID(rootDeviceHierarchy.DeviceElement.DeviceElementId);
                if (rootDeviceElementID.HasValue)
                {
                    HitchPoint hitch = new HitchPoint();
                    hitch.ReferencePoint = new ReferencePoint() { XOffset = hierarchyElement.XOffsetRepresentation, YOffset = hierarchyElement.YOffsetRepresentation, ZOffset = hierarchyElement.ZOffsetRepresentation };
                    hitch.HitchTypeEnum = HitchTypeEnum.Unkown;
                    DataModel.Catalog.HitchPoints.Add(hitch);

                    //Get the DeviceElementConfiguration for the root element in order that we may link the Connector to it.
                    DeviceElement root = DataModel.Catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == rootDeviceElementID);
                    if (root != null)
                    {
                        DeviceElementConfiguration rootDeviceConfiguration = DeviceElementMapper.GetDeviceElementConfiguration(root, rootDeviceHierarchy, DataModel.Catalog);
                        if (rootDeviceConfiguration != null)
                        {
                            Connector connector = new Connector();
                            ImportIDs(connector.Id, hierarchyElement.DeviceElement.DeviceElementId);
                            connector.DeviceElementConfigurationId = rootDeviceConfiguration.Id.ReferenceId;
                            connector.HitchPointId = hitch.Id.ReferenceId;
                            DataModel.Catalog.Connectors.Add(connector);

                            //The ID mapping will link the ADAPT Connector to the ISO Device Element
                            TaskDataMapper.InstanceIDMap.Add(connector.Id.ReferenceId, connectorDeviceElement.DeviceElementId);
                        }
                    }
                }
            }
        }

        private EnumeratedValue DecodeMachineInfo(string clientNAME)
        {
            if (string.IsNullOrEmpty(clientNAME) ||
                clientNAME.Length != 16)
                return null;

            byte deviceGroup;
            if (!byte.TryParse(clientNAME.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceGroup))
                return null;
            deviceGroup >>= 4;

            if ((deviceGroup & 0x07) != 2) // Agricultural devices
                return null;

            byte deviceClass;
            if (!byte.TryParse(clientNAME.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceClass))
                return null;
            deviceClass >>= 1;

            AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember machineType = DefinedTypeEnumerationInstanceList.dtiTractor.ToModelEnumMember(); //Default

            DeviceOperationType deviceType = DeviceOperationTypes.SingleOrDefault(d => d.ClientNAMEMachineType == deviceClass);
            if (deviceType != null)
            {
                machineType = deviceType.MachineEnumerationMember.ToModelEnumMember();
            }

            return new EnumeratedValue { Representation = RepresentationInstanceList.dtMachineType.ToModelRepresentation(), Value = machineType };
        }

        #endregion Import
    }
}
