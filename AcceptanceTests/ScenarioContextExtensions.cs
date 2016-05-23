using System.Collections.Generic;
using System.Xml;
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

        public static XmlNode OriginalIsoTaskData(this ScenarioContext scenarioContext)
        {
            return scenarioContext.SafeGet<XmlNode>(OriginalIsoTaskDataKey);
        }

        public static void OriginalIsoTaskData(this ScenarioContext scenarioContext, XmlNode taskData)
        {
            scenarioContext.Set(taskData, OriginalIsoTaskDataKey);
        }

        public static IList<ApplicationDataModel> ApplicationDataModel(this ScenarioContext scenarioContext)
        {
            return scenarioContext.SafeGet<IList<ApplicationDataModel>>(ApplicationDataModelKey);
        }

        public static void ApplicationDataModel(this ScenarioContext scenarioContext, IList<ApplicationDataModel> applicationDataModel)
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