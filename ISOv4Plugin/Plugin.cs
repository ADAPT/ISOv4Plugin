/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using System.Xml;
using System.IO;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Mappers;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System.Reflection;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public class Plugin : AgGateway.ADAPT.ApplicationDataModel.ADM.IPlugin
    {
        private const string FileName = "TASKDATA.XML";

        #region IPlugin Members
        string IPlugin.Name => "ISO v4 Plugin";

        string IPlugin.Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        string IPlugin.Owner => "AgGateway";

        void IPlugin.Export(ApplicationDataModel.ADM.ApplicationDataModel dataModel, string exportPath, Properties properties)
        {
            //Convert the ADAPT model into the ISO model
            string outputPath = exportPath.WithTaskDataPath();
            TaskDataMapper taskDataMapper = new TaskDataMapper(outputPath, properties);
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
            foreach (var taskData in taskDataObjects)
            {
                //Convert the ISO model to ADAPT
                TaskDataMapper taskDataMapper = new TaskDataMapper(taskData.FilePath, properties);
                ApplicationDataModel.ADM.ApplicationDataModel dataModel = taskDataMapper.Import(taskData);

                adms.Add(dataModel);
            }

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
            List<Error> errors = new List<Error>();
            var data = ReadDataCard(dataPath);
            foreach (ISO11783_TaskData datum in data)
            {
                datum.Validate(errors);
            }

            return errors.ToArray();
        }
        #endregion IPlugin Members

        private static List<string> GetListOfTaskDataFiles(string dataPath)
        {
            var taskDataFiles = new List<string>();
            if (Directory.Exists(dataPath))
            {
                taskDataFiles.AddRange(Directory.GetFiles(dataPath, FileName, SearchOption.AllDirectories));
            }
            return taskDataFiles;
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
                string filePath = Path.GetDirectoryName(taskDataFile);

                //Deserialize the ISOXML into the ISO models
                XmlDocument document = new XmlDocument();
                document.Load(taskDataFile);
                XmlNode rootNode = document.SelectSingleNode("ISO11783_TaskData");
                ISO11783_TaskData taskData = ISO11783_TaskData.ReadXML(rootNode, filePath);
                taskData.FilePath = filePath;

                taskDataObjects.Add(taskData);
            }
            return taskDataObjects;
        }
    }
}
