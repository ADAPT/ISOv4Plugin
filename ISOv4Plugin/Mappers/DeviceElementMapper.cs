/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System.Globalization;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

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

            DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceHierarchy.DeviceElementHierarchies[isoDevice.DeviceId];

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
            switch (isoDeviceElement.DeviceElementType)
            {
                case ISODeviceElementType.Device:  //This is the root device element
                    if (deviceClassification != null && deviceClassification.Value != null &&
                        (deviceClassification.Value.Code == DefinedTypeEnumerationInstanceList.dtiTractor.DomainTag ||
                        deviceClassification.Value.Code == DefinedTypeEnumerationInstanceList.dtiUtilityVehicle.DomainTag))
                    {
                        deviceElement.DeviceElementType = DeviceElementTypeEnum.Machine;
                        AddMachineConfiguration(isoDeviceElement, deviceElement, rootDeviceHierarchy);
                    }
                    else
                    {
                        deviceElement.DeviceElementType = DeviceElementTypeEnum.Implement;
                        AddImplementConfiguration(isoDeviceElement, deviceElement, rootDeviceHierarchy);
                    }
                    break;
                case ISODeviceElementType.Bin:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Bin;
                    break;
                case ISODeviceElementType.Function:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Function;
                    AddImplementConfiguration(isoDeviceElement, deviceElement, rootDeviceHierarchy);
                    break;
                case ISODeviceElementType.Section:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Section;
                    AddSectionConfiguration(isoDeviceElement, deviceElement, rootDeviceHierarchy);
                    break;
                case ISODeviceElementType.Unit:
                    deviceElement.DeviceElementType = DeviceElementTypeEnum.Unit;
                    AddSectionConfiguration(isoDeviceElement, deviceElement, rootDeviceHierarchy);
                    break;
                case ISODeviceElementType.Connector:
                    AddConnector(isoDeviceElement, deviceElement); //TODO review implementation
                    break;
                case ISODeviceElementType.Navigation:
                    //TODO 
                    break;
            }

            return deviceElement;
        }

        private MachineConfiguration AddMachineConfiguration(ISODeviceElement ISODeviceElement, DeviceElement adaptDeviceElement, DeviceElementHierarchy rootDeviceHierarchy)
        {
            MachineConfiguration machineConfig = new MachineConfiguration();

            //Description
            machineConfig.Description = ISODeviceElement.DeviceElementDesignator;

            //Device Element ID
            machineConfig.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Offsets
            machineConfig.Offsets = new List<NumericRepresentationValue>();
            if (rootDeviceHierarchy.XOffset.HasValue)
            {
                machineConfig.Offsets.Add(rootDeviceHierarchy.XOffset.Value.AsNumericRepresentationValue("0086", RepresentationMapper));
            }
            if (rootDeviceHierarchy.YOffset.HasValue)
            {
                machineConfig.Offsets.Add(rootDeviceHierarchy.YOffset.Value.AsNumericRepresentationValue("0087", RepresentationMapper));
            }
            if (rootDeviceHierarchy.ZOffset.HasValue)
            {
                machineConfig.Offsets.Add(rootDeviceHierarchy.ZOffset.Value.AsNumericRepresentationValue("0088", RepresentationMapper));
            }

            DataModel.Catalog.DeviceElementConfigurations.Add(machineConfig);
            return machineConfig;
        }

        private ImplementConfiguration AddImplementConfiguration(ISODeviceElement ISODeviceElement, DeviceElement adaptDeviceElement, DeviceElementHierarchy rootDeviceHierarchy)
        {
            ImplementConfiguration implementConfig = new ImplementConfiguration();

            //Description
            implementConfig.Description = ISODeviceElement.DeviceElementDesignator;

            //Device Element ID
            implementConfig.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Offsets
            if (rootDeviceHierarchy.YOffset.HasValue)
            {
                implementConfig.YOffset = rootDeviceHierarchy.YOffset.Value.AsNumericRepresentationValue("0087", RepresentationMapper);
            }


            implementConfig.Offsets = new List<NumericRepresentationValue>();
            if (rootDeviceHierarchy.XOffset.HasValue)
            {
                implementConfig.Offsets.Add(rootDeviceHierarchy.XOffset.Value.AsNumericRepresentationValue("0086", RepresentationMapper));
            }
            if (rootDeviceHierarchy.YOffset.HasValue)
            {
                implementConfig.Offsets.Add(implementConfig.YOffset);
            }
            if (rootDeviceHierarchy.ZOffset.HasValue)
            {
                implementConfig.Offsets.Add(rootDeviceHierarchy.ZOffset.Value.AsNumericRepresentationValue("0088", RepresentationMapper));
            }

            //Total Width 
            if (rootDeviceHierarchy.Width.HasValue)
            {
                implementConfig.PhysicalWidth = rootDeviceHierarchy.Width.Value.AsNumericRepresentationValue("0046", RepresentationMapper);
            }

            //Row Width
            long? rowWidth = rootDeviceHierarchy.GetLowestLevelSectionWidth();
            if (rowWidth.HasValue)
            {
                implementConfig.Width = rowWidth.Value.AsNumericRepresentationValue("0046", RepresentationMapper);
            }
            
            DataModel.Catalog.DeviceElementConfigurations.Add(implementConfig);

            return implementConfig;
        }

        private SectionConfiguration AddSectionConfiguration(ISODeviceElement isoDeviceElement, DeviceElement adaptDeviceElement, DeviceElementHierarchy rootDeviceHierarchy)
        {
            DeviceElementHierarchy section = rootDeviceHierarchy.GetModelByISODeviceElementID(isoDeviceElement.DeviceElementId);
            if (section == null)
            {
                return null;
            }

            SectionConfiguration sectionConfiguration = new SectionConfiguration();

            //Description
            sectionConfiguration.Description = isoDeviceElement.DeviceElementDesignator;

            //Device Element ID
            sectionConfiguration.DeviceElementId = adaptDeviceElement.Id.ReferenceId;

            //Width & Offsets
            if (section.Width.HasValue)
            {
                sectionConfiguration.SectionWidth = section.Width.Value.AsNumericRepresentationValue("0046", RepresentationMapper);
            }

            if (section.XOffset.HasValue)
            {
                sectionConfiguration.InlineOffset = section.XOffset.Value.AsNumericRepresentationValue("0086", RepresentationMapper);
            }
            if (section.YOffset.HasValue)
            {
                sectionConfiguration.LateralOffset = section.YOffset.Value.AsNumericRepresentationValue("0087", RepresentationMapper);
            }

            DataModel.Catalog.DeviceElementConfigurations.Add(sectionConfiguration);
            return sectionConfiguration;
        }

        private void AddConnector(ISODeviceElement isoDeviceElement, DeviceElement deviceElement)
        {
            if (isoDeviceElement.Parent is ISODeviceElement && (isoDeviceElement.Parent as ISODeviceElement).DeviceElementType != ISODeviceElementType.Connector)
            {
                DeviceElementConfiguration parentElementConfiguration = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(d => d.DeviceElementId == deviceElement.ParentDeviceId);
                if (parentElementConfiguration != null)
                {
                    Connector connector = new Connector();
                    connector.DeviceElementConfigurationId = parentElementConfiguration.Id.ReferenceId;
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
