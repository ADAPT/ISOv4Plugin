using System.IO;
using System.Linq;
using AcceptanceTests.Asserts.Export;
using AcceptanceTests.Asserts.Import;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
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
            var isoTaskData = new XmlReader().Read(ScenarioContext.Current.DataCardPath(), "TASKDATA.XML");
            ScenarioContext.Current.OriginalIsoTaskData(isoTaskData);

            var dataModel = _plugin.Import(ScenarioContext.Current.DataCardPath());
            ScenarioContext.Current.ApplicationDataModel(dataModel);
        }

        [Then(@"iso is imported to adapt")]
        public void ThenIsoIsImportedToAdapt()
        {
            var tsks = ScenarioContext.Current.OriginalIsoTaskData().Items.Where(x => x.GetType() == typeof(TSK)).Cast<TSK>().ToList();
            var currentPath = ScenarioContext.Current.DataCardPath();

            foreach (var applicationDataModel in ScenarioContext.Current.ApplicationDataModel())
            {
                LoggedDataAssert.AreEqual(tsks, currentPath, applicationDataModel.Documents.LoggedData.ToList(), applicationDataModel.Catalog);
            }
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

        [Then(@"Adapt is exported to ISO")]
        public void ThenAdaptIsExportedToIso()
        {
            var isoTaskData = new XmlReader().Read(ScenarioContext.Current.ExportPath(), "TASKDATA.XML");
            foreach (var applicationDataModel in ScenarioContext.Current.ApplicationDataModel())
            {
                TaskDataAssert.AreEqual(applicationDataModel, isoTaskData, ScenarioContext.Current.ExportPath()); 
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            var directory = Directory.GetParent(ScenarioContext.Current.DataCardPath()).FullName;
            if(Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }
}
