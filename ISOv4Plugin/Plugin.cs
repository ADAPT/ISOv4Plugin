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

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public class Plugin : AgGateway.ADAPT.ApplicationDataModel.ADM.IPlugin
    {
        private const string FileName = "TASKDATA.XML";

        #region IPlugin Members
        string IPlugin.Name => "ISO v4 Plugin";

        string IPlugin.Version => "";

        string IPlugin.Owner => "";

        void IPlugin.Export(ApplicationDataModel.ADM.ApplicationDataModel dataModel, string exportPath, Properties properties)
        {
            //Convert the ADAPT model into the ISO model
            TaskDataMapper taskDataMapper = new TaskDataMapper(exportPath.WithTaskDataPath(), properties);
            ISO11783_TaskData taskData = taskDataMapper.Export(dataModel);

            //Serialize the ISO model to XML
            TaskDocumentWriter writer = new TaskDocumentWriter();
            writer.WriteTaskData(exportPath, taskData);

            //Serialize the Link List
            ISO11783_LinkList linkList = taskDataMapper.ISOLinkList;
            writer.WriteLinkList(exportPath, linkList);
        }

        public IList<ApplicationDataModel.ADM.ApplicationDataModel> Import(string dataPath, Properties properties = null)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            if (!taskDataFiles.Any())
                return null;

            var adms = new List<ApplicationDataModel.ADM.ApplicationDataModel>();

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

                //Load any LinkList
                ISO11783_LinkList linkList = null;
                ISOAttachedFile linkListFile = taskData.ChildElements.OfType<ISOAttachedFile>().SingleOrDefault(afe => afe.FileType == 1);
                if (linkListFile != null)
                {
                    XmlDocument linkDocument = new XmlDocument();
                    string linkPath = Path.Combine(filePath, linkListFile.FilenamewithExtension);
                    linkDocument.Load(linkPath);
                    XmlNode linkRoot = linkDocument.SelectSingleNode("ISO11783LinkList");
                    linkList = ISO11783_LinkList.ReadXML(linkRoot, filePath);
                }

                //Convert the ISO model to ADAPT
                TaskDataMapper taskDataMapper = new TaskDataMapper(filePath, properties);
                ApplicationDataModel.ADM.ApplicationDataModel dataModel = taskDataMapper.Import(taskData, linkList);

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
            throw new NotImplementedException();
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
    }
}
