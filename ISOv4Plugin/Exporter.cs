using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public interface IExporter
    {
        XmlWriter Export(ApplicationDataModel.ADM.ApplicationDataModel applicationDataModel, string datacardPath, XmlWriter isoTaskData, MemoryStream xmlStream);
    }

    public class Exporter : IExporter
    {
        private readonly ITaskMapper _taskMapper;

        public Exporter()
            : this(new TaskMapper())
        {
        }

        public Exporter(ITaskMapper taskMapper)
        {
            _taskMapper = taskMapper;
        }

        public XmlWriter Export(ApplicationDataModel.ADM.ApplicationDataModel applicationDataModel, string datacardPath, XmlWriter isoTaskData, MemoryStream xmlStream)
        {
            if (applicationDataModel == null)
                return isoTaskData;
            
            var numberOfExistingTasks = GetNumberOfExistingTasks(isoTaskData, xmlStream);
            var tasks = applicationDataModel.Documents == null ? null : _taskMapper.Map(applicationDataModel.Documents.LoggedData, applicationDataModel.Catalog, datacardPath, numberOfExistingTasks);
            if (tasks != null)
            {
                var taskList = tasks.ToList();
                taskList.ForEach(t => t.WriteXML(isoTaskData));
            }
            //Close the root element with </ISO11783_TaskData>
            isoTaskData.WriteEndElement();
            return isoTaskData;
        }

        private static int GetNumberOfExistingTasks(XmlWriter data, MemoryStream isoTaskData)
        {
            data.Flush();
            var xml = Encoding.UTF8.GetString(isoTaskData.ToArray());
            if(!xml.EndsWith(">"))
                xml += ">";
            xml += "</ISO11783_TaskData>";
            var xDocument = XDocument.Parse(xml);
            return xDocument.Root.Descendants("TSK").Count();
        }
    }
}