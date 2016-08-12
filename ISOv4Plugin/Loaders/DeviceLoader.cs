using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using EnumerationMember = AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class DeviceLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, DeviceElement> _machines;

        private DeviceLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _machines = new Dictionary<string, DeviceElement>();
        }

        public static Dictionary<string, DeviceElement> Load(TaskDataDocument taskDocument)
        {
            var loader = new DeviceLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, DeviceElement> Load()
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

        private DeviceElement LoadMachine(XmlNode inputNode, out string machineId)
        {
            var machine = new DeviceElement();

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

        private bool DecodeMachineInfo(string machineInfo, DeviceElement machine)
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

            machine.DeviceClassification = GetMachineType(deviceClass);

            return true;
        }

        private EnumeratedValue GetMachineType(byte deviceClass)
        {
            EnumerationMember machineType;

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

            return new EnumeratedValue{Representation = RepresentationInstanceList.dtMachineType.ToModelRepresentation(), Value = machineType};
        }
    }
}
