using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using TechTalk.SpecFlow;

namespace AcceptanceTests
{
    public static class ScenarioContextExtensions
    {
        private const string DataCardPathKey = "DataCardPath";
        private const string ExportPathKey = "ExportPath";
        private const string OriginalIsoTaskDataKey = "OriginalIsoTaskData";
        private const string ApplicationDataModelKey = "ApplicationDataModel";

        public static string DataCardPath(this ScenarioContext scenarioContext)
        {
            return scenarioContext.SafeGet<string>(DataCardPathKey);
        }

        public static void DataCardPath(this ScenarioContext scenarioContext, string dataCardPath)
        {
            scenarioContext.Set(dataCardPath, DataCardPathKey);
        }

        public static string ExportPath(this ScenarioContext scenarioContext)
        {
            return scenarioContext.SafeGet<string>(ExportPathKey);
        }

        public static void ExportPath(this ScenarioContext scenarioContext, string exportPath)
        {
            scenarioContext.Set(exportPath, ExportPathKey);
        }

        public static ISO11783_TaskData OriginalIsoTaskData(this ScenarioContext scenarioContext)
        {
            return scenarioContext.SafeGet<ISO11783_TaskData>(OriginalIsoTaskDataKey);
        }

        public static void OriginalIsoTaskData(this ScenarioContext scenarioContext, ISO11783_TaskData taskData)
        {
            scenarioContext.Set(taskData, OriginalIsoTaskDataKey);
        }

        public static ApplicationDataModel ApplicationDataModel(this ScenarioContext scenarioContext)
        {
            return scenarioContext.SafeGet<ApplicationDataModel>(ApplicationDataModelKey);
        }

        public static void ApplicationDataModel(this ScenarioContext scenarioContext, ApplicationDataModel applicationDataModel)
        {
            scenarioContext.Set(applicationDataModel, ApplicationDataModelKey);
        }

        private static T SafeGet<T>(this ScenarioContext scenarioContext, string key)
        {
            if (!scenarioContext.ContainsKey(key))
                return default(T);

            return scenarioContext.Get<T>(key);
        }

    }
}