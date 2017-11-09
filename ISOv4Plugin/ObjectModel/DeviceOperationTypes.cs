using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class DeviceOperationTypes : List<DeviceOperationType>
    {
        public DeviceOperationTypes()
        {
            //Non-specific
            this.Add(new DeviceOperationType(0, DefinedTypeEnumerationInstanceList.dtiMachineTypeOther, OperationTypeEnum.Unknown, true));

            //Tractor
            this.Add(new DeviceOperationType(1, DefinedTypeEnumerationInstanceList.dtiTractor, OperationTypeEnum.Unknown, true));

            //Tillage
            this.Add(new DeviceOperationType(2, DefinedTypeEnumerationInstanceList.dtiTillage, OperationTypeEnum.Tillage, false));

            //Secondary Tillage
            this.Add(new DeviceOperationType(3, DefinedTypeEnumerationInstanceList.dtiTillage, OperationTypeEnum.Tillage, false));

            //Seeding/Planting
            this.Add(new DeviceOperationType(4, DefinedTypeEnumerationInstanceList.dtiSeedersPlanter, OperationTypeEnum.SowingAndPlanting, false));

            //Fertilizing
            this.Add(new DeviceOperationType(5, DefinedTypeEnumerationInstanceList.dtiFertilizer, OperationTypeEnum.Fertilizing, false));

            //Spraying
            this.Add(new DeviceOperationType(6, DefinedTypeEnumerationInstanceList.dtiSprayer, OperationTypeEnum.CropProtection, false));

            //Harvesting
            this.Add(new DeviceOperationType(7, DefinedTypeEnumerationInstanceList.dtiCombine, OperationTypeEnum.Harvesting, false));

            //Root harvesting
            this.Add(new DeviceOperationType(8, DefinedTypeEnumerationInstanceList.dtiLifter, OperationTypeEnum.Harvesting, false));

            //Forage
            this.Add(new DeviceOperationType(9, DefinedTypeEnumerationInstanceList.dtiForageHarvester, OperationTypeEnum.ForageHarvesting, false));

            //Irrigation
            this.Add(new DeviceOperationType(10, DefinedTypeEnumerationInstanceList.dtiIrrigationSystem, OperationTypeEnum.Fertilizing, false)); //No irrigation type?

            // Transport/trailer
            this.Add(new DeviceOperationType(11, DefinedTypeEnumerationInstanceList.dtiTransportTrailers, OperationTypeEnum.Transport, true));

            // Farm yard operations
            this.Add(new DeviceOperationType(12, DefinedTypeEnumerationInstanceList.dtiFarmsteadOperations, OperationTypeEnum.Unknown, false));
            
            // Powered auxiliary devices
            this.Add(new DeviceOperationType(13, DefinedTypeEnumerationInstanceList.dtiPoweredAuxiliaryDevices, OperationTypeEnum.Unknown, true));

            // Special crops
            this.Add(new DeviceOperationType(14, DefinedTypeEnumerationInstanceList.dtiSpecialCrop, OperationTypeEnum.Unknown, false));
            
            // Earth work
            this.Add(new DeviceOperationType(15, DefinedTypeEnumerationInstanceList.dtiEarthworks, OperationTypeEnum.Unknown, false));
            
            // Skidder
            this.Add(new DeviceOperationType(16, DefinedTypeEnumerationInstanceList.dtiSkidders, OperationTypeEnum.Unknown, true));

            // Sensor Systems
            this.Add(new DeviceOperationType(17, DefinedTypeEnumerationInstanceList.dtiSensorSystems, OperationTypeEnum.Unknown, false));

            // Slurry Applicators
            this.Add(new DeviceOperationType(25, DefinedTypeEnumerationInstanceList.dtiSlurryApplicators, OperationTypeEnum.Fertilizing, false));
        }

    }

    public class DeviceOperationType
    {
        public DeviceOperationType(byte machineType, EnumerationMember machineMember, OperationTypeEnum operationType, bool hasMachineConfiguration)
        {
            ClientNAMEMachineType = machineType;
            MachineEnumerationMember = machineMember;
            OperationType = operationType;
            HasMachineConfiguration = hasMachineConfiguration;  //Defines whether a device should be configured as a Machine or an Implement in the ADAPT model
        }

        public byte ClientNAMEMachineType { get; set; }
        public EnumerationMember MachineEnumerationMember { get; set; }
        public OperationTypeEnum OperationType { get; set; }
        public bool HasMachineConfiguration { get; set; }
    }
}
