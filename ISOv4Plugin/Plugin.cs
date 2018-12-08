/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Mappers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public class Plugin : IPlugin
    {
        private const string FileName = "taskdata.xml";

        #region IPlugin Members
        string IPlugin.Name => "ISO v4 Plugin";

        string IPlugin.Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        string IPlugin.Owner => "AgGateway";

        void IPlugin.Export(ApplicationDataModel.ADM.ApplicationDataModel dataModel, string exportPath, Properties properties)
        {
            //Convert the ADAPT model into the ISO model
            string outputPath = exportPath.WithTaskDataPath();
            TaskDataMapper taskDataMapper = new TaskDataMapper(outputPath, properties);
            Errors = taskDataMapper.Errors;
            ISO11783_TaskData taskData = taskDataMapper.Export(dataModel);

            //Serialize the ISO model to XML
            TaskDocumentWriter writer = new TaskDocumentWriter();
            writer.WriteTaskData(exportPath, taskData);

            //Serialize the Link List
            writer.WriteLinkList(exportPath, taskData.LinkList);
        }

        public IList<ApplicationDataModel.ADM.ApplicationDataModel> Import(string dataPath, Properties properties = null)
        {
            var taskDataObjects = ReadDataCard(dataPath);
            if (taskDataObjects == null)
                return null;

            var adms = new List<ApplicationDataModel.ADM.ApplicationDataModel>();
            List<IError> errors = new List<IError>();

            foreach (var taskData in taskDataObjects)
            {
                //Convert the ISO model to ADAPT
                TaskDataMapper taskDataMapper = new TaskDataMapper(taskData.DataFolder, properties);
                ApplicationDataModel.ADM.ApplicationDataModel dataModel = taskDataMapper.Import(taskData);
                errors.AddRange(taskDataMapper.Errors);
                adms.Add(dataModel);
            }
            Errors = errors;

            return adms;
        }

        Properties _properties = null;
        Properties IPlugin.GetProperties(string dataPath)
        {
            if (_properties == null)
            {
                _properties = new Properties();
                _properties.SetProperty(ISOGrid.GridTypeProperty, "2");
            }
            return _properties;
        }

        void IPlugin.Initialize(string args)
        {
        }

        public bool IsDataCardSupported(string dataPath, Properties properties = null)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            return taskDataFiles.Any();
        }

        IList<IError> IPlugin.ValidateDataOnCard(string dataPath, Properties properties)
        {
            List<IError> errors = new List<IError>();
            var data = ReadDataCard(dataPath);
            foreach (ISO11783_TaskData datum in data)
            {
                datum.Validate(errors);
            }

            return errors;
        }

        public IList<IError> Errors { get; set; }
        #endregion IPlugin Members

        private static List<string> GetListOfTaskDataFiles(string dataPath)
        {
            //The directory check here is case sensitive for unix-based OSes (see comment below)
            if (Directory.Exists(dataPath))
            {
                //Note! We need to iterate through all files and do a ToLower for this to work in .Net Core in Linux since that filesystem
                //is case sensitive and the NetStandard interface for Directory.GetFiles doesn't account for that yet.
                var fileNameToFind = FileName.ToLower();
                var allFiles = Directory.GetFiles(dataPath, "*.*", SearchOption.AllDirectories);
                var matchedFiles = allFiles.Where(file => file.ToLower().EndsWith(fileNameToFind));
                return matchedFiles.ToList();
            }
            return new List<string>();
        }

        private List<ISO11783_TaskData> ReadDataCard(string dataPath)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            if (!taskDataFiles.Any())
                return null;

            List<ISO11783_TaskData> taskDataObjects = new List<ISO11783_TaskData>();
            foreach (var taskDataFile in taskDataFiles)
            {
                //Per ISO11783-10:2015(E) 8.5, all related files are in the same directory as the TASKDATA.xml file.
                //The TASKDATA directory is only required when exporting to removable media.
                //As such, the plugin will import data in any directory structure, and always export to a TASKDATA directory.
                string dataFolder = Path.GetDirectoryName(taskDataFile);

                //Deserialize the ISOXML into the ISO models
                XmlDocument document = new XmlDocument();
                document.Load(taskDataFile);
                XmlNode rootNode = document.SelectSingleNode("ISO11783_TaskData");
                ISO11783_TaskData taskData = ISO11783_TaskData.ReadXML(rootNode, dataFolder);
                taskData.DataFolder = dataFolder;
                taskData.FilePath = taskDataFile;

                taskDataObjects.Add(taskData);
            }
            return taskDataObjects;
        }
    }
}
