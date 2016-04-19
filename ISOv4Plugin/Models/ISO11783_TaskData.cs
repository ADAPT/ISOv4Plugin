using System.Linq;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class ISO11783_TaskData : IWriter
    {
        public int VersionMajor;
        public int VersionMinor;
        public string ManagementSoftwareManufacturer;
        public string ManagementSoftwareVersion;
        public string TaskControllerManufacturer;
        public string TaskControllerVersion;
        public ISO11783_TaskDataDataTransferOrigin DataTransferOrigin;
        public object[] Items;
        
        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<ISO11783_TaskData xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" ");
            xmlBuilder.Append(string.Format("VersionMajor=\"{0}\" ", VersionMajor));
            xmlBuilder.Append(string.Format("VersionMinor=\"{0}\" ", VersionMinor));
            xmlBuilder.Append(string.Format("ManagementSoftwareManufacturer=\"{0}\" ", ManagementSoftwareManufacturer));
            xmlBuilder.Append(string.Format("ManagementSoftwareVersion=\"{0}\" ", ManagementSoftwareVersion));
            xmlBuilder.Append(string.Format("DataTransferOrigin=\"{0}\" ", (int)DataTransferOrigin));
            xmlBuilder.Append(string.Format("TaskControllerManufacturer=\"{0}\" ", TaskControllerManufacturer));
            xmlBuilder.Append(string.Format("TaskControllerVersion=\"{0}\">", TaskControllerVersion));
            if(Items != null)
            {
                foreach (var item in Items.Cast<IWriter>())
                {
                    xmlBuilder.Append(item.WriteXML());
                }
            }
            xmlBuilder.Append("</ISO11783_TaskData>");
            return xmlBuilder.ToString();
        }
    }
}
