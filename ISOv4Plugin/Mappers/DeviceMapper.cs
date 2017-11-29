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

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IDeviceMapper
    {
        IEnumerable<ISODevice> ExportDevices(IEnumerable<DeviceModel> adaptDevices);
        ISODevice ExportDevice(DeviceModel adaptDevice);
        IEnumerable<DeviceModel> ImportDevices(IEnumerable<ISODevice> isoDevices);
        DeviceModel ImportDevice(ISODevice isoDevice);
    }

    public class DeviceMapper : BaseMapper, IDeviceMapper
    {
        public DeviceMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "DVC")
        {
        }

        #region Export
        public IEnumerable<ISODevice> ExportDevices(IEnumerable<DeviceModel> adaptDevices)
        {
            List <ISODevice> devices = new List<ISODevice>();
            foreach (DeviceModel adaptDevice in adaptDevices)
            {
                ISODevice isoDevice = ExportDevice(adaptDevice);
                devices.Add(isoDevice);
            }
            return devices;
        }

        public ISODevice ExportDevice(DeviceModel adaptDevice)
        {
            ISODevice dvc = new ISODevice();

            //ID
            string id = adaptDevice.Id.FindIsoId() ?? GenerateId();
            dvc.DeviceId = id;
            ExportUniqueIDs(adaptDevice.Id, id);
            TaskDataMapper.ISOIdMap.Add(adaptDevice.Id.ReferenceId, id);

            //Designator
            dvc.DeviceDesignator = adaptDevice.Description;

            //Device Elements
            IEnumerable<DeviceElement> deviceElements = DataModel.Catalog.DeviceElements.Where(d => d.ParentDeviceId == adaptDevice.Id.ReferenceId);
            if (deviceElements.Any())
            {
                DeviceElementMapper deviceElementMapper = new DeviceElementMapper(TaskDataMapper);
                dvc.DeviceElements = deviceElementMapper.ExportDeviceElements(deviceElements, dvc).ToList();

                //Device Properties
                ExportDeviceProperties(dvc, deviceElements);
            }

            //Serial Number
            if (deviceElements.Any(e => !String.IsNullOrEmpty(e.SerialNumber)))
            {
                dvc.DeviceSerialNumber = deviceElements.First(e => !String.IsNullOrEmpty(e.SerialNumber)).SerialNumber;
            }

            return dvc;
        }

        private void ExportDeviceProperties(ISODevice device, IEnumerable<DeviceElement> deviceElements)
        {
            int objectID = 0;
            foreach (DeviceElement deviceElement in deviceElements)
            {
                string isoID = TaskDataMapper.ISOIdMap.FindByADAPTId(deviceElement.Id.ReferenceId);
                if (!string.IsNullOrEmpty(isoID))
                {
                    ISODeviceElement det = device.DeviceElements.SingleOrDefault(d => d.DeviceElementId == isoID);
                    if (det != null)
                    {
                        IEnumerable<DeviceElementConfiguration> configs = DataModel.Catalog.DeviceElementConfigurations.Where(c => c.DeviceElementId == deviceElement.Id.ReferenceId);
                        foreach (DeviceElementConfiguration config in configs)
                        {
                            if (config is MachineConfiguration)
                            {
                                MachineConfiguration machineConfig = config as MachineConfiguration;
                                ISODeviceElement navigationElement = device.DeviceElements.FirstOrDefault(d => d.DeviceElementType == ISODeviceElementType.Navigation);
                                if (navigationElement == null)
                                {
                                    if (machineConfig.GpsReceiverXOffset != null)
                                    {
                                        ExportDeviceProperty(device, navigationElement, machineConfig.GpsReceiverXOffset, ++objectID);
                                    }
                                    if (machineConfig.GpsReceiverYOffset != null)
                                    {
                                        ExportDeviceProperty(device, navigationElement, machineConfig.GpsReceiverYOffset, ++objectID);
                                    }
                                    if (machineConfig.GpsReceiverZOffset != null)
                                    {
                                        ExportDeviceProperty(device, navigationElement, machineConfig.GpsReceiverZOffset, ++objectID);
                                    }
                                }
                            }
                            else if (config is ImplementConfiguration)
                            {
                                ImplementConfiguration implementConfig = config as ImplementConfiguration;
                                if (implementConfig.Width != null)
                                {
                                    ExportDeviceProperty(device, det, implementConfig.Width, ++objectID);
                                }
                                if (implementConfig.Offsets != null)
                                {
                                    implementConfig.Offsets.ForEach(o => ExportDeviceProperty(device, det, o, ++objectID));
                                }
                            }
                            else if (config is SectionConfiguration)
                            {
                                SectionConfiguration sectionConfig = config as SectionConfiguration;
                                if (sectionConfig.InlineOffset != null)
                                {
                                    ExportDeviceProperty(device, det, sectionConfig.InlineOffset, ++objectID);
                                }
                                if (sectionConfig.LateralOffset != null)
                                {
                                    ExportDeviceProperty(device, det, sectionConfig.LateralOffset, ++objectID);
                                }
                                if (sectionConfig.SectionWidth != null)
                                {
                                    ExportDeviceProperty(device, det, sectionConfig.SectionWidth, ++objectID);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExportDeviceProperty(ISODevice device, ISODeviceElement deviceElement, NumericRepresentationValue representationValue, int objectID)
        {
            ISODeviceProperty dpt = new ISODeviceProperty();
            dpt.ObjectID = objectID;
            switch (representationValue.Representation.Code)
            {
                case "0043":
                case "0046":
                    dpt.DDI = representationValue.Representation.Code;
                    dpt.Designator = "Width";
                    dpt.Value = representationValue.AsLongViaMappedDDI(RepresentationMapper);
                    break;
                case "vrOffsetInline":
                    dpt.DDI = "0086";
                    dpt.Designator = "XOffset";
                    dpt.Value = representationValue.AsLongViaMappedDDI(RepresentationMapper);
                    break;
                case "vrOffsetLateral":
                    dpt.DDI = "0087";
                    dpt.Designator = "YOffset";
                    dpt.Value = representationValue.AsLongViaMappedDDI(RepresentationMapper);
                    break;
                case "vrOffsetVertical":
                    dpt.DDI = "0088";
                    dpt.Designator = "ZOffset";
                    dpt.Value = representationValue.AsLongViaMappedDDI(RepresentationMapper);
                    break;
            }

            if (!string.IsNullOrEmpty(dpt.DDI))
            {
                device.DeviceProperties.Add(dpt);
                deviceElement.DeviceObjectReferences.Add(new ISODeviceObjectReference() { DeviceObjectId = objectID });
            }
        }

        #endregion Export 

        #region Import

        public IEnumerable<DeviceModel> ImportDevices(IEnumerable<ISODevice> isoDevices)
        {
            //Import devices
            List<DeviceModel> adaptDevices = new List<DeviceModel>();
            foreach (ISODevice isoDevice in isoDevices)
            {
                DeviceModel adaptDevice = ImportDevice(isoDevice);
                adaptDevices.Add(adaptDevice);
            }
            return adaptDevices;
        }

        public DeviceModel ImportDevice(ISODevice isoDevice)
        {
            DeviceModel deviceModel = new DeviceModel();

            //ID
            deviceModel.Id.UniqueIds.AddRange(ImportUniqueIDs(isoDevice.DeviceId));
            TaskDataMapper.ADAPTIdMap.Add(isoDevice.DeviceId, deviceModel.Id.ReferenceId);

            //Description
            deviceModel.Description = isoDevice.DeviceDesignator;

            //Device Elements
            if (isoDevice.DeviceElements.Any())
            {
                DeviceElementMapper detMapper = new DeviceElementMapper(TaskDataMapper);
                IEnumerable<DeviceElement> deviceElements = detMapper.ImportDeviceElements(isoDevice);
            }

            return deviceModel;
        }

        #endregion Import
    }
}
