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
            this.Add(new DeviceOperationType(0, DefinedTypeEnumerationInstanceList.dtiMachineTypeOther, OperationTypeEnum.Unknown));

            //Tractor
            this.Add(new DeviceOperationType(1, DefinedTypeEnumerationInstanceList.dtiTractor, OperationTypeEnum.Unknown));

            //Tillage
            this.Add(new DeviceOperationType(2, DefinedTypeEnumerationInstanceList.dtiTillage, OperationTypeEnum.Tillage));

            //Secondary Tillage
            this.Add(new DeviceOperationType(3, DefinedTypeEnumerationInstanceList.dtiTillage, OperationTypeEnum.Tillage));

            //Seeding/Planting
            this.Add(new DeviceOperationType(4, DefinedTypeEnumerationInstanceList.dtiSeedersPlanter, OperationTypeEnum.SowingAndPlanting));

            //Fertilizing
            this.Add(new DeviceOperationType(5, DefinedTypeEnumerationInstanceList.dtiFertilizer, OperationTypeEnum.Fertilizing));

            //Spraying
            this.Add(new DeviceOperationType(6, DefinedTypeEnumerationInstanceList.dtiSprayer, OperationTypeEnum.CropProtection));

            //Harvesting
            this.Add(new DeviceOperationType(7, DefinedTypeEnumerationInstanceList.dtiCombine, OperationTypeEnum.Harvesting));

            //Root harvesting
            this.Add(new DeviceOperationType(8, DefinedTypeEnumerationInstanceList.dtiLifter, OperationTypeEnum.Harvesting));

            //Forage
            this.Add(new DeviceOperationType(9, DefinedTypeEnumerationInstanceList.dtiCombine, OperationTypeEnum.ForageHarvesting));

            //Irrigation
            this.Add(new DeviceOperationType(10, DefinedTypeEnumerationInstanceList.dtiIrrigationSystem, OperationTypeEnum.Fertilizing)); //No irrigation type?

            // Transport/trailer
            this.Add(new DeviceOperationType(11, DefinedTypeEnumerationInstanceList.dtiTransportTrailers, OperationTypeEnum.Transport));

            // Farm yard operations
            this.Add(new DeviceOperationType(12, DefinedTypeEnumerationInstanceList.dtiFarmsteadOperations, OperationTypeEnum.Unknown));
            
            // Powered auxilliary devices
            this.Add(new DeviceOperationType(13, DefinedTypeEnumerationInstanceList.dtiPoweredAuxiliaryDevices, OperationTypeEnum.Unknown));

            // Special crops
            this.Add(new DeviceOperationType(14, DefinedTypeEnumerationInstanceList.dtiSpecialCrop, OperationTypeEnum.Unknown));
            
            // Earth work
            this.Add(new DeviceOperationType(15, DefinedTypeEnumerationInstanceList.dtiEarthworks, OperationTypeEnum.Unknown));
            
            // Skidder
            this.Add(new DeviceOperationType(16, DefinedTypeEnumerationInstanceList.dtiSkidders, OperationTypeEnum.Unknown));
        }

    }

    public class DeviceOperationType
    {
        public DeviceOperationType(byte machineType, EnumerationMember machineMember, OperationTypeEnum operationType)
        {
            ClientNAMEMachineType = machineType;
            MachineEnumerationMember = machineMember;
            OperationType = operationType;
        }

        public byte ClientNAMEMachineType { get; set; }
        public EnumerationMember MachineEnumerationMember { get; set; }
        public OperationTypeEnum OperationType { get; set; }
    }
}
