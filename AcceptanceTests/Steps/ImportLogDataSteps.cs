using System.Collections.Generic;
using System.IO;
using System.Xml;
using AcceptanceTests.Asserts.Import;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;
using TechTalk.SpecFlow;
using TestUtilities;

namespace AcceptanceTests.Steps
{
    [Binding]
    public class ImportLogDataSteps
    {
        private readonly Plugin _plugin = new Plugin();

        [Given(@"I have datacard (.*)")]
        public void GivenIHaveDatacard(string cardname)
        {
            var cardPath = DataCardUtility.WriteDataCard(cardname);
            ScenarioContext.Current.DataCardPath(cardPath);
        }

        [When(@"I import with the plugin")]
        public void WhenIImportWithThePlugin()
        {
            var taskDataXmlFile = Path.Combine(ScenarioContext.Current.DataCardPath(),"TASKDATA");
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(Path.Combine(taskDataXmlFile, "TASKDATA.xml"));
            var taskData = xmlDocument.SelectSingleNode("ISO11783_TaskData");
            ScenarioContext.Current.OriginalIsoTaskData(taskData);

            var dataModel = _plugin.Import(ScenarioContext.Current.DataCardPath());
            ScenarioContext.Current.ApplicationDataModel(dataModel);
        }

        [When(@"I export to Iso")]
        public void WhenIExportToIso()
        {
            var exportPath = Path.Combine(ScenarioContext.Current.DataCardPath(), "export");
            Directory.CreateDirectory(exportPath);
            ScenarioContext.Current.ExportPath(exportPath);

            foreach (var applicationDataModel in ScenarioContext.Current.ApplicationDataModel())
            {
                _plugin.Export(applicationDataModel, exportPath);
            }
        }

        [Then(@"iso is imported to adapt")]
        public void ThenIsoIsImportedToAdapt()
        {
            var currentPath = ScenarioContext.Current.DataCardPath();

            var linkList = LoadLinkList(currentPath);

            foreach (var applicationDataModel in ScenarioContext.Current.ApplicationDataModel())
            {
                ApplicationDataModelAssert.AreEqual(ScenarioContext.Current.OriginalIsoTaskData(), applicationDataModel, currentPath, linkList);
            }
        }

        [Then(@"Adapt is exported to ISO")]
        public void ThenAdaptIsExportedToIso()
        {
            var currentPath = ScenarioContext.Current.ExportPath();

            var linkList = LoadLinkList(currentPath);
            foreach (var applicationDataModel in ScenarioContext.Current.ApplicationDataModel())
            {
                ApplicationDataModelAssert.AreEqual(ScenarioContext.Current.OriginalIsoTaskData(), applicationDataModel, currentPath, linkList);
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var directory = ScenarioContext.Current.DataCardPath();
            if(Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        private static Dictionary<string, List<UniqueId>> LoadLinkList(string currentPath)
        {
            var linkListFile = Path.Combine(currentPath, "TASKDATA", "LINKLIST.XML");
            if (!File.Exists(linkListFile)) 
                return new Dictionary<string, List<UniqueId>>();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(linkListFile);
            var linkList = xmlDocument.SelectSingleNode("ISO11783_LinkList");

            var lgpNodes = linkList.SelectNodes("LGP");

            return LinkGroupLoader.Load(lgpNodes);
        }
    }
}
