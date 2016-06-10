namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class ISO11783_TaskData
    {
        public int VersionMajor = 4;
        public int VersionMinor = 1;
        public string ManagementSoftwareManufacturer = "AgGateway ADAPT";
        public string ManagementSoftwareVersion = "0.1";
        public string TaskControllerManufacturer;
        public string TaskControllerVersion;
        public ISO11783_TaskDataDataTransferOrigin DataTransferOrigin = ISO11783_TaskDataDataTransferOrigin.Item1;
        public IWriter[] Items;
    }
}
