/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.Mappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
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

        public Plugin()
        {
            Errors = new List<IError>();
        }

        public void Export(ApplicationDataModel.ADM.ApplicationDataModel dataModel, string exportPath, Properties properties)
        {
            //Convert the ADAPT model into the ISO model
            string outputPath = exportPath.WithTaskDataPath();
            TaskDataMapper taskDataMapper = new TaskDataMapper(outputPath, properties);
            Errors = taskDataMapper.Errors;
            ISO11783_TaskData taskData = taskDataMapper.Export(dataModel);

            //Serialize the ISO model to XML
            TaskDocumentWriter writer = new TaskDocumentWriter();
            writer.WriteTaskData(outputPath, taskData);

            //Serialize the Link List
            if (taskData.Version > 3)
            {
                writer.WriteLinkList(outputPath, taskData.LinkList);
            }
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
                TaskDataMapper taskDataMapper = new TaskDataMapper(taskData.DataFolder, properties, taskData.VersionMajor);
                ApplicationDataModel.ADM.ApplicationDataModel dataModel = taskDataMapper.Import(taskData);
                foreach (var error in taskDataMapper.Errors)
                {
                    Errors.Add(error);
                }
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
            return dataPath.GetDirectoryFiles(FileName, SearchOption.AllDirectories).ToList();
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
                try
                {
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
                catch (Exception ex)
                {
                    Errors.Add(new Error() { Description = $"Failed to read file {taskDataFile}.  Skipping it.  Exception: {ex.Message}" });
                }
            }
            return taskDataObjects;
        }
    }
}
