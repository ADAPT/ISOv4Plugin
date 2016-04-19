using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class DeviceLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, Machine> _machines;

        private DeviceLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _machines = new Dictionary<string, Machine>();
        }

        public static Dictionary<string, Machine> Load(TaskDataDocument taskDocument)
        {
            var loader = new DeviceLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, Machine> Load()
        {
            LoadMachines(_rootNode.SelectNodes("DVC"));
            ProcessExternalNodes();

            return _machines;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'DVC')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadMachines(inputNodes);
            }
        }

        private void LoadMachines(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string machineId;
                var machine = LoadMachine(inputNode, out machineId);
                if (machine != null)
                    _machines.Add(machineId, machine);
            }
        }

        private Machine LoadMachine(XmlNode inputNode, out string machineId)
        {
            var machine = new Machine();

            // Required fields. Do not proceed if they are missing
            machineId = inputNode.GetXmlNodeValue("@A");
            if (machineId == null ||
                !DecodeMachineInfo(inputNode.GetXmlNodeValue("@D"), machine))
                return null;

            // Optional fields
            machine.Description = inputNode.GetXmlNodeValue("@B");
            machine.SerialNumber = inputNode.GetXmlNodeValue("@E");

            _taskDocument.LoadLinkedIds(machineId, machine.Id);
            return machine;
        }

        private bool DecodeMachineInfo(string machineInfo, Machine machine)
        {
            if (string.IsNullOrEmpty(machineInfo) ||
                machineInfo.Length != 16)
                return false;

            byte deviceGroup;
            if (!byte.TryParse(machineInfo.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceGroup))
                return false;

            if (deviceGroup != 2) // Agricultural devices
                return false;

            byte deviceClass;
            if (!byte.TryParse(machineInfo.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceClass))
                return false;

            var machineType = GetMachineType(deviceClass);

            machine.MachineTypeId = machineType.Id.ReferenceId;

            return true;
        }

        private MachineType GetMachineType(byte deviceClass)
        {
            MachineType machineType;
            if (_taskDocument.MachineTypes.TryGetValue(deviceClass, out machineType))
                return machineType;

            machineType = new MachineType();
            switch (deviceClass)
            {
                //case 0: // Non-specific systems
                //case 1: // Tractor
                //case 2: // Tillage
                //case 3: // Secondary tillage

                case 4: // Planters/seeders
                    machineType.MachineTypeEnum = MachineTypeEnum.Sprayer;
                    break;

                case 5: // Fertilizers
                case 6: // Sprayers
                    machineType.MachineTypeEnum = MachineTypeEnum.Sprayer;
                    break;

                case 7: // Harvesters
                case 8: // Root harvesters
                    machineType.MachineTypeEnum = MachineTypeEnum.Combine;
                    break;

                case 9: // Forage
                    machineType.MachineTypeEnum = MachineTypeEnum.ForageHarvester;
                    break;

                case 10: // Irrigation
                    machineType.MachineTypeEnum = MachineTypeEnum.IrrigationSystem;
                    break;

                case 11: // Transport/trailer
                case 12: // Farm yard operations
                case 13: // Powered auxilliary devices
                case 14: // Special crops
                case 15: // Earth work
                case 16: // Skidder
                    machineType.MachineTypeEnum = MachineTypeEnum.UtilityVehicle;
                    break;

                default:
                    machineType.MachineTypeEnum = MachineTypeEnum.Tractor;
                    break;
            }

            _taskDocument.MachineTypes.Add(deviceClass, machineType);

            return machineType;
        }
    }
}
