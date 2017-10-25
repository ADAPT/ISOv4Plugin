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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IDeviceElementMapper
    {
        IEnumerable<ISODeviceElement> ExportDeviceElements(IEnumerable<DeviceElement> adaptDeviceElements, ISODevice isoDevice);
        ISODeviceElement ExportDeviceElement(DeviceElement adaptDeviceElement, ISODevice isoDevice, List<ISODeviceElement> pendingDeviceElements);
        IEnumerable<DeviceElement> ImportDeviceElements(ISODevice isoDevice);
        DeviceElement ImportDeviceElement(ISODeviceElement isoDeviceElement, EnumeratedValue deviceClassification, DeviceElementHierarchy rootDeviceHierarchy);
    }

    public class DeviceElementMapper : BaseMapper, IDeviceElementMapper
    {
        public DeviceElementMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "DET")
        {
        }

        #region Export
        public IEnumerable<ISODeviceElement> ExportDeviceElements(IEnumerable<DeviceElement> adaptDeviceElements, ISODevice isoDevice)
        {
            List<ISODeviceElement> isoDeviceElements = new List<ISODeviceElement>();
            foreach (DeviceElement adaptDeviceElement in adaptDeviceElements)
            {
                ISODeviceElement isoGroup = ExportDeviceElement(adaptDeviceElement, isoDevice, isoDeviceElements);
                isoDeviceElements.Add(isoGroup);
            }
            return isoDeviceElements;
        }

        public ISODeviceElement ExportDeviceElement(DeviceElement adaptDeviceElement, ISODevice isoDevice, List<ISODeviceElement> pendingDeviceElements)
        {
            ISODeviceElement det = new ISODeviceElement(isoDevice);

            //ID
            string id = adaptDeviceElement.Id.FindIsoId() ?? GenerateId();
            det.DeviceElementId = id;
            ExportUniqueIDs(adaptDeviceElement.Id, id);
            TaskDataMapper.ISOIdMap.Add(adaptDeviceElement.Id.ReferenceId, id);

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
                string deviceElementID = TaskDataMapper.ISOIdMap[parentDeviceElement.Id.ReferenceId];
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

            return det;
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
                return null;
            }

            DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceElementHierarchies.Items[isoDevice.DeviceId];

            //Import device elements
            List<DeviceElement> adaptDeviceElements = new List<DeviceElement>();

            //Import down the hierarchy to ensure integrity of parent references
            for (int i = 0; i <= deviceElementHierarchy.GetMaxDepth(); i++)
            {
                IEnumerable<ISODeviceElement> isoDeviceElements = deviceElementHierarchy.GetElementsAtDepth(i).Select(h => h.DeviceElement);
                foreach (ISODeviceElement isoDeviceElement in isoDeviceElements)
                {
                    DeviceElement adaptDeviceElement = ImportDeviceElement(isoDeviceElement, deviceClassification, deviceElementHierarchy);
                    if (isoDeviceElement.DeviceElementType == ISODeviceElementType.Device)
                    {
                        //Setting the Device serial number on the root Device Element only
                        adaptDeviceElement.SerialNumber = isoDevice.DeviceSerialNumber;
                    }
                    adaptDeviceElements.Add(adaptDeviceElement);
                }
            }

            return adaptDeviceElements;
        }

        public DeviceElement ImportDeviceElement(ISODeviceElement isoDeviceElement, EnumeratedValue deviceClassification, DeviceElementHierarchy rootDeviceHierarchy)
        {
            DeviceElement deviceElement = new DeviceElement();

            //ID
            deviceElement.Id.UniqueIds.AddRange(ImportUniqueIDs(isoDeviceElement.DeviceElementId));
            TaskDataMapper.ADAPTIdMap.Add(isoDeviceElement.DeviceElementId, deviceElement.Id.ReferenceId);

            //Device ID
            deviceElement.DeviceModelId = TaskDataMapper.ADAPTIdMap[isoDeviceElement.Device.DeviceId].Value;

            //Description
            deviceElement.Description = isoDeviceElement.DeviceElementDesignator;

            //Classification
            deviceElement.DeviceClassification = deviceClassification;

            //Parent ID
            if (isoDeviceElement.Parent != null)
            {
                if (isoDeviceElement.Parent is ISODeviceElement)
                {
                    ISODeviceElement parentElement = isoDeviceElement.Parent as ISODeviceElement;
                    deviceElement.ParentDeviceId = TaskDataMapper.ADAPTIdMap[parentElement.DeviceElementId].Value;
                }
                else
                {
                    ISODevice parentDevice = isoDeviceElement.Parent as ISODevice;
                    deviceElement.ParentDeviceId = TaskDataMapper.ADAPTIdMap[parentDevice.DeviceId].Value;
                }
            }

            //Device Element Type
            if (IsConnectorDeviceElement(isoDeviceElement))
            {
                AddConnector(rootDeviceHierarchy, isoDeviceElement, deviceElement);
            }
            else
            {
                switch (isoDeviceElement.DeviceElementType)
                {
                    case ISODeviceElementType.Device:  //This is the root device element
                        if (deviceClassification != null && deviceClassification.Value != null &&
                            (deviceClassification.Value.Code == DefinedTypeEnumerationInstanceList.dtiTractor.DomainTag ||
                            deviceClassification.Value.Code == DefinedTypeEnumerationInstanceList.dtiUtilityVehicle.DomainTag))
                        {
                            deviceElement.DeviceElementType = DeviceElementTypeEnum.Machine;
                        }
                        else
                        {
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
                        deviceElement.DeviceElementType = DeviceElementTypeEnum.Unit; 
                        break;
                }
            }

            DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceElementHierarchies.GetRelevantHierarchy(isoDeviceElement.DeviceElementId);
            if (HasGeometryInformation(deviceElementHierarchy))
            {
                AddDeviceElementConfiguration(deviceElement, deviceElementHierarchy, DataModel.Catalog);
            }

            return deviceElement;
        }

        private bool HasGeometryInformation(DeviceElementHierarchy deviceElementHierarchy)
        {
            return deviceElementHierarchy.Width.HasValue || deviceElementHierarchy.XOffset.HasValue || deviceElementHierarchy.YOffset.HasValue || deviceElementHierarchy.ZOffset.HasValue;
        }

        public static DeviceElementConfiguration AddDeviceElementConfiguration(DeviceElement adaptDeviceElement, DeviceElementHierarchy deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            switch (adaptDeviceElement.DeviceElementType)
            {
                case DeviceElementTypeEnum.Machine:
                    return AddMachineConfiguration(adaptDeviceElement, deviceHierarchy, catalog);
                case DeviceElementTypeEnum.Implement:
                    return AddImplementConfiguration(adaptDeviceElement, deviceHierarchy, catalog);
                case DeviceElementTypeEnum.Function:
                    if (deviceHierarchy.Parent.DeviceElement.DeviceElementType != ISODeviceElementType.Function)
                    {
                        //Function is the entire implement
                        return AddImplementConfiguration(adaptDeviceElement, deviceHierarchy, catalog);
                    }
                    else
                    {
                        //Function is part of the implement.  I.e., TC-GEO example 9
                        return AddSectionConfiguration(adaptDeviceElement, deviceHierarchy, catalog);
                    }
                case DeviceElementTypeEnum.Section:
                case DeviceElementTypeEnum.Unit:
                    return AddSectionConfiguration(adaptDeviceElement, deviceHierarchy, catalog);
                default:
                    return null;
            }
        }

        public static MachineConfiguration AddMachineConfiguration(DeviceElement adaptDeviceElement, DeviceElementHierarchy deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            MachineConfiguration machineConfig = new MachineConfiguration();

            //Description
            machineConfig.Description = deviceHierarchy.DeviceElement.DeviceElementDesignator;

            //Device Element ID
            machineConfig.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Offsets
            machineConfig.Offsets = new List<NumericRepresentationValue>();
            if (deviceHierarchy.XOffset != null)
            {
                machineConfig.Offsets.Add(deviceHierarchy.XOffsetRepresentation);
                machineConfig.GpsReceiverXOffset = deviceHierarchy.XOffsetRepresentation;
            }
            if (deviceHierarchy.YOffset != null)
            {
                machineConfig.Offsets.Add(deviceHierarchy.YOffsetRepresentation);
                machineConfig.GpsReceiverYOffset = deviceHierarchy.YOffsetRepresentation;
            }
            if (deviceHierarchy.ZOffset != null)
            {
                machineConfig.Offsets.Add(deviceHierarchy.ZOffsetRepresentation);
                machineConfig.GpsReceiverZOffset = deviceHierarchy.ZOffsetRepresentation;
            }

            catalog.DeviceElementConfigurations.Add(machineConfig);
            return machineConfig;
        }

        public static ImplementConfiguration AddImplementConfiguration(DeviceElement adaptDeviceElement, DeviceElementHierarchy deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            ImplementConfiguration implementConfig = new ImplementConfiguration();

            //Description
            implementConfig.Description = deviceHierarchy.DeviceElement.DeviceElementDesignator;

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
                implementConfig.PhysicalWidth = deviceHierarchy.WidthRepresentation;
            }

            //Row Width
            NumericRepresentationValue rowWidth = deviceHierarchy.GetLowestLevelSectionWidth();
            if (rowWidth != null)
            {
                implementConfig.Width = rowWidth;
            }
            
            catalog.DeviceElementConfigurations.Add(implementConfig);

            return implementConfig;
        }

        public static SectionConfiguration AddSectionConfiguration(DeviceElement adaptDeviceElement, DeviceElementHierarchy deviceHierarchy, AgGateway.ADAPT.ApplicationDataModel.ADM.Catalog catalog)
        {
            SectionConfiguration sectionConfiguration = new SectionConfiguration();

            //Description
            sectionConfiguration.Description = deviceHierarchy.DeviceElement.DeviceElementDesignator;

            //Device Element ID
            sectionConfiguration.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Width & Offsets
            if (deviceHierarchy.Width != null)
            {
                sectionConfiguration.SectionWidth = deviceHierarchy.WidthRepresentation;
            }

            sectionConfiguration.Offsets = new List<NumericRepresentationValue>();
            if (deviceHierarchy.XOffset != null)
            {
                sectionConfiguration.InlineOffset = deviceHierarchy.XOffsetRepresentation;
                sectionConfiguration.Offsets.Add(deviceHierarchy.XOffsetRepresentation);
            }
            if (deviceHierarchy.YOffset != null)
            {
                sectionConfiguration.LateralOffset = deviceHierarchy.YOffsetRepresentation;
                sectionConfiguration.Offsets.Add(deviceHierarchy.YOffsetRepresentation);
            }

            catalog.DeviceElementConfigurations.Add(sectionConfiguration);
            return sectionConfiguration;
        }

        private bool IsConnectorDeviceElement(ISODeviceElement isoDeviceElement)
        {
            if (isoDeviceElement.DeviceElementType == ISODeviceElementType.Connector)
            {
                return true;
            }
            else
            {
                //Treat any other type except Navigation (e.g., Function) with X,Y,Z offsets only as Connectors
                if (isoDeviceElement.DeviceElementType != ISODeviceElementType.Navigation &&
                    ((isoDeviceElement.DeviceProcessDatas.Count() == 3 && !isoDeviceElement.DeviceProcessDatas.Any(d => d.DDI != "0086" || d.DDI != "0087" || d.DDI != "0088"))
                    || (isoDeviceElement.DeviceProperties.Count() == 3 && !isoDeviceElement.DeviceProperties.Any(d => d.DDI != "0086" || d.DDI != "0087" || d.DDI != "0088"))))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        /// <summary>
        /// Adds a connector with a hitch at the given reference point, referencing the parent DeviceElement as the DeviceElementConfiguration
        /// </summary>
        /// <param name="rootDeviceHierarchy"></param>
        /// <param name="isoDeviceElement"></param>
        /// <param name="deviceElement"></param>
        private void AddConnector(DeviceElementHierarchy rootDeviceHierarchy, ISODeviceElement isoDeviceElement, DeviceElement deviceElement)
        {
            DeviceElementHierarchy hierarchy = rootDeviceHierarchy.FromDeviceElementID(isoDeviceElement.DeviceElementId);

            HitchPoint hitch = new HitchPoint();
            hitch.ReferencePoint = new ReferencePoint() { XOffset = hierarchy.XOffsetRepresentation, YOffset = hierarchy.YOffsetRepresentation, ZOffset = hierarchy.ZOffsetRepresentation };
            hitch.HitchTypeEnum = HitchTypeEnum.Unkown;
            DataModel.Catalog.HitchPoints.Add(hitch);

            if (isoDeviceElement.Parent is ISODeviceElement && (isoDeviceElement.Parent as ISODeviceElement).DeviceElementType != ISODeviceElementType.Connector)
            {
                DeviceElementConfiguration parentElementConfiguration = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(d => d.DeviceElementId == deviceElement.ParentDeviceId);
                if (parentElementConfiguration != null)
                {
                    Connector connector = new Connector();
                    connector.DeviceElementConfigurationId = parentElementConfiguration.Id.ReferenceId;
                    connector.HitchPointId = hitch.Id.ReferenceId;
                    DataModel.Catalog.Connectors.Add(connector);
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

            AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember machineType;
            switch (deviceClass)
            {
                case 0: // Non-specific systems
                    machineType = DefinedTypeEnumerationInstanceList.dtiMachineTypeOther.ToModelEnumMember();
                    break;
                case 1: // Tractor
                    machineType = DefinedTypeEnumerationInstanceList.dtiTractor.ToModelEnumMember();
                    break;
                //case 2: // Tillage
                //case 3: // Secondary tillage
                //case 4: // Planters/seeders

                case 5: // Fertilizers
                case 6: // Sprayers
                    machineType = DefinedTypeEnumerationInstanceList.dtiSprayer.ToModelEnumMember();
                    break;

                case 7: // Harvesters
                case 8: // Root harvesters
                    machineType = DefinedTypeEnumerationInstanceList.dtiCombine.ToModelEnumMember();
                    break;

                case 9: // Forage
                    machineType = DefinedTypeEnumerationInstanceList.dtiForageHarvester.ToModelEnumMember();
                    break;

                case 10: // Irrigation
                    machineType = DefinedTypeEnumerationInstanceList.dtiIrrigationSystem.ToModelEnumMember();
                    break;

                case 11: // Transport/trailer
                case 12: // Farm yard operations
                case 13: // Powered auxilliary devices
                case 14: // Special crops
                case 15: // Earth work
                case 16: // Skidder
                    machineType = DefinedTypeEnumerationInstanceList.dtiUtilityVehicle.ToModelEnumMember();
                    break;

                default:
                    machineType = DefinedTypeEnumerationInstanceList.dtiTractor.ToModelEnumMember();
                    break;
            }

            return new EnumeratedValue { Representation = RepresentationInstanceList.dtMachineType.ToModelRepresentation(), Value = machineType };
        }
        #endregion Import
    }
}
