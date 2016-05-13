using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Writers;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public class Plugin : IPlugin
    {
        private readonly IXmlReader _xmlReader;
        private readonly IImporter _importer;
        private readonly IExporter _exporter;
        private const string FileName = "TASKDATA.XML";

        public Plugin() : this(new XmlReader(), new Importer(), new Exporter())
        {
            
        }
        public Plugin(IXmlReader xmlReader, IImporter importer, IExporter exporter)
        {
            _xmlReader = xmlReader;
            _importer = importer;
            _exporter = exporter;
            Name = "ISO Plugin";
            Version = "0.1.1";
            Owner = "AgGateway & Contributors";
        }

        public void Initialize(string args = null)
        {
        }

        public IList<IError> ValidateDataOnCard(string dataPath, Properties properties = null)
        {
            var errors = new List<IError>();
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            if (!taskDataFiles.Any())
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

        public bool IsDataCardSupported(string dataPath, Properties properties = null)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            return taskDataFiles.Any();
        }

        public IList<ApplicationDataModel.ADM.ApplicationDataModel> Import(string dataPath, Properties properties = null)
        {
            var taskDataFiles = GetListOfTaskDataFiles(dataPath);
            if (!taskDataFiles.Any())
                return null;

            var adms = new List<ApplicationDataModel.ADM.ApplicationDataModel>();

            foreach (var taskDataFile in taskDataFiles)
            {
                var dataModel = new ApplicationDataModel.ADM.ApplicationDataModel();

                ConvertTaskDataFileToModel(taskDataFile, dataModel);


                var iso11783TaskData = _xmlReader.Read(taskDataFile);
                _importer.Import(iso11783TaskData, dataPath, dataModel);
                adms.Add(dataModel);
            }

            return adms;
        }

        public void Export(ApplicationDataModel.ADM.ApplicationDataModel dataModel, string exportPath, Properties properties = null)
        {
            using (var taskWriter = new TaskDocumentWriter())
            {
                var xmlWriter = taskWriter.Write(exportPath, dataModel);

                var iso11783TaskData = _exporter.Export(dataModel, exportPath, xmlWriter, taskWriter.XmlStream);

                var filePath = Path.Combine(exportPath, "TASKDATA", FileName);
                if (iso11783TaskData != null)
                {
                    xmlWriter.Flush();
                    var xml = Encoding.UTF8.GetString(taskWriter.XmlStream.ToArray());
                    File.WriteAllText(filePath, xml);
                }
            }
        }
        
        public Properties GetProperties(string dataPath)
        {
            return new Properties();
        }

        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Owner { get; private set; }

        private static List<string> GetListOfTaskDataFiles(string dataPath)
        {
            var taskDataFiles = new List<string>();

            var inputPath = Path.Combine(dataPath, "Taskdata");
            if (Directory.Exists(inputPath))
                taskDataFiles.AddRange(Directory.GetFiles(inputPath, "taskdata.xml"));

            if (!taskDataFiles.Any() && Directory.Exists(dataPath))
                taskDataFiles.AddRange(Directory.GetFiles(dataPath, "taskdata.xml"));

            return taskDataFiles;
        }

        private static void ConvertTaskDataFileToModel(string taskDataFile, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
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
            catalog.ContactInfo = taskDocument.Contacts;

            dataModel.Catalog = catalog;

            var documents = new Documents();
            documents.GuidanceAllocations = taskDocument.GuidanceAllocations;
            documents.LoggedData = taskDocument.Tasks;

            dataModel.Documents = documents;
        }
    }
}
