using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AgGateway.ADAPT.Plugins
{
    public class IsoPlugin : IPlugin
    {
        private Properties _properties = new Properties();

        public string Name { get { return "TC ISO Plugin"; } }

        public string Owner { get { return "AgGateway"; } }

        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        public void Export(ApplicationDataModel.ApplicationDataModel dataModel, string exportPath, Properties properties)
        {
        }

        public Properties GetProperties(string dataPath)
        {
            return _properties;
        }

        public ApplicationDataModel.ApplicationDataModel Import(string dataPath, Properties properties)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            if (taskDataFiles.Length == 0)
                return null;

            var dataModel = new ApplicationDataModel.ApplicationDataModel();

            foreach (var taskDataFile in taskDataFiles)
                ConvertTaskDataFileToModel(taskDataFile, dataModel);

            return dataModel;
        }

        public void Initialize(string args)
        {

        }

        public bool IsDataCardSupported(string dataPath, Properties properties)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            return taskDataFiles.Length > 0;
        }

        public List<IError> ValidateDataOnCard(string dataPath, Properties properties)
        {
            List<IError> errors = new List<IError>();
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            if (taskDataFiles.Length == 0)
                return errors;

            foreach (var taskDataFile in taskDataFiles)
            {
                var taskDocument = new TaskDataDocument();
                if (taskDocument.LoadFromFile(taskDataFile) == false)
                {
                    errors.AddRange(taskDocument.Errors);
                }
            }

            return errors;
        }


        private static string[] GetListOfTaskDataFiles(string dataPath)
        {
            string[] taskDataFiles = new string[0];

            var inputPath = Path.Combine(dataPath, "Taskdata");
            if (Directory.Exists(inputPath))
                taskDataFiles = Directory.GetFiles(inputPath, "taskdata.xml");

            if (taskDataFiles.Length == 0 && Directory.Exists(dataPath))
                taskDataFiles = Directory.GetFiles(dataPath, "taskdata.xml");

            return taskDataFiles;
        }

        private static void ConvertTaskDataFileToModel(string taskDataFile, ApplicationDataModel.ApplicationDataModel dataModel)
        {
            var taskDocument = new TaskDataDocument();
            if (taskDocument.LoadFromFile(taskDataFile) == false)
                return;

            var catalog = new Catalog();
            catalog.Growers = taskDocument.Customers.Values.ToList();
            catalog.Farms = taskDocument.Farms.Values.ToList();
            catalog.Fields = taskDocument.Fields.Values.ToList();
            catalog.GuidanceGroups = taskDocument.GuidanceGroups.Values.Select(x => x.Group).ToList();
            catalog.GuidancePatterns = taskDocument.GuidanceGroups.Values.SelectMany(x => x.Patterns.Values).ToList();
            catalog.Crops = taskDocument.Crops.Values.ToList();
            catalog.CropZones = taskDocument.CropZones.Values.ToList();
            catalog.Machines = taskDocument.Machines;
            catalog.CropVarieties = taskDocument.CropVarieties.Values.ToList();
            catalog.FieldBoundaries = taskDocument.FieldBoundaries;
            catalog.ProductMixes = taskDocument.ProductMixes.Values.ToList();
            catalog.Ingredients = taskDocument.Ingredients;
            catalog.FertilizerProducts = taskDocument.Products.Values.OfType<FertilizerProduct>().ToList();
            catalog.Prescriptions = taskDocument.RasterPrescriptions.Cast<Prescription>().ToList();

            dataModel.Catalog = catalog;

            var documents = new Documents();
            documents.GuidanceAllocations = taskDocument.GuidanceAllocations;
            documents.LoggedData = taskDocument.Tasks;

            dataModel.Documents = documents;
        }
    }
}
