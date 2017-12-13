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
        DeviceElementMapper _deviceElementMapper;
        public DeviceMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "DVC")
        {
            _deviceElementMapper = new DeviceElementMapper(taskDataMapper);
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
            ExportIDs(adaptDevice.Id, id);

            //Designator
            dvc.DeviceDesignator = adaptDevice.Description;

            //Device Elements
            IEnumerable<DeviceElement> deviceElements = DataModel.Catalog.DeviceElements.Where(d => d.ParentDeviceId == adaptDevice.Id.ReferenceId);
            if (deviceElements.Any())
            {
                dvc.DeviceElements = _deviceElementMapper.ExportDeviceElements(deviceElements, dvc).ToList();
            }

            //Serial Number
            if (deviceElements.Any(e => !String.IsNullOrEmpty(e.SerialNumber)))
            {
                dvc.DeviceSerialNumber = deviceElements.First(e => !String.IsNullOrEmpty(e.SerialNumber)).SerialNumber;
            }

            return dvc;
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
            ImportIDs(deviceModel.Id, isoDevice.DeviceId);

            //Description
            deviceModel.Description = isoDevice.DeviceDesignator;

            //Device Elements
            if (isoDevice.DeviceElements.Any())
            {
                IEnumerable<DeviceElement> deviceElements = _deviceElementMapper.ImportDeviceElements(isoDevice);
            }

            return deviceModel;
        }

        #endregion Import
    }
}
