/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ITaskDataMapper
    {
        ISO11783_TaskData Export(ApplicationDataModel.ADM.ApplicationDataModel adm);
        ApplicationDataModel.ADM.ApplicationDataModel Import(ISO11783_TaskData taskData);
    }

    public class TaskDataMapper : ITaskDataMapper
    {
        public const string TaskControllerManufacturerProperty = "TaskControllerManufacturer";
        public const string TaskControllerVersionProperty = "TaskControllerVersion";
        public const string DataTransferOriginProperty = "DataTransferOrigin";
        
        public TaskDataMapper(string dataPath, Properties properties)
        {
            BaseFolder = dataPath;
            RepresentationMapper = new RepresentationMapper();
            DDIs = DdiLoader.Ddis;
            Properties = properties;
            DeviceOperationTypes = new DeviceOperationTypes();
            InstanceIDMap = new InstanceIDMap();
            Errors = new List<IError>();
        }

        public string BaseFolder { get; private set; }
        public Properties Properties { get; private set; }
        public InstanceIDMap InstanceIDMap { get; private set; }
        public List<IError> Errors { get; private set; }

        public ApplicationDataModel.ADM.ApplicationDataModel AdaptDataModel { get; private set; }
        public ISO11783_TaskData ISOTaskData { get; private set; }
        public UniqueIdMapper UniqueIDMapper { get; set; }
        public DeviceElementHierarchies DeviceElementHierarchies { get; set; }

        internal RepresentationMapper RepresentationMapper { get; private set; }
        internal Dictionary<int, DdiDefinition> DDIs { get; private set; }
        internal DeviceOperationTypes DeviceOperationTypes { get; private set; }

        CodedCommentListMapper _commentListMapper;
        public CodedCommentListMapper CommentListMapper
        {
            get
            {
                if (_commentListMapper == null)
                {
                    _commentListMapper = new CodedCommentListMapper(this);
                }
                return _commentListMapper;
            }
        }

        CodedCommentMapper _commentMapper;
        public CodedCommentMapper CommentMapper
        {
            get
            {
                if (_commentMapper == null)
                {
                    _commentMapper = new CodedCommentMapper(this, CommentListMapper);
                }
                return _commentMapper;
            }
        }

        ProductGroupMapper _productGroupMapper;
        public ProductGroupMapper ProductGroupMapper
        {
            get
            {
                if (_productGroupMapper == null)
                {
                    _productGroupMapper = new ProductGroupMapper(this);
                }
                return _productGroupMapper;
            }
        }

        GuidanceGroupMapper _guidanceGroupMapper;
        public GuidanceGroupMapper GuidanceGroupMapper
        {
            get
            {
                if (_guidanceGroupMapper == null) _guidanceGroupMapper = new GuidanceGroupMapper(this);
                return _guidanceGroupMapper;
            }
        }

        GuidancePatternMapper _guidancePaddernMapper;
        public GuidancePatternMapper GuidancePatternMapper
        {
            get
            {
                if (_guidancePaddernMapper == null) _guidancePaddernMapper = new GuidancePatternMapper(this);
                return _guidancePaddernMapper;
            }
        }

        public void AddError(string error, string id = null, string source = null, string stackTrace = null)
        {
            Errors.Add(new Error() { Description = error, Id = id, Source = source, StackTrace = stackTrace });
        }

        public ISO11783_TaskData Export(ApplicationDataModel.ADM.ApplicationDataModel adm)
        {
            AdaptDataModel = adm;

            // Try to read some of the ISO11783 attributes from properties.
            // TaskControllerManufacturer
            string taskControllerManufacturer = Properties.GetProperty(TaskControllerManufacturerProperty);
            if (taskControllerManufacturer.Length > 32) taskControllerManufacturer = taskControllerManufacturer.Substring(0, 32);
            // TaskControllerVersion
            string taskControllerVersion = Properties.GetProperty(TaskControllerVersionProperty);
            if (taskControllerManufacturer.Length > 32) taskControllerVersion = taskControllerVersion.Substring(0, 32);
            // DataTransferOrigin
            ISOEnumerations.ISOTaskDataTransferOrigin dataTransferOrigin;
            string s = Properties.GetProperty(DataTransferOriginProperty);
            if (!Enum.TryParse<ISOEnumerations.ISOTaskDataTransferOrigin>(s, out dataTransferOrigin)
            || !Enum.IsDefined(typeof(ISOEnumerations.ISOTaskDataTransferOrigin), dataTransferOrigin))
            {
                dataTransferOrigin = ISOEnumerations.ISOTaskDataTransferOrigin.FMIS;    // Default
            }


            //TaskData
            ISOTaskData = new ISO11783_TaskData();
            ISOTaskData.VersionMajor = 4;
            ISOTaskData.VersionMinor = 2;
            ISOTaskData.ManagementSoftwareManufacturer = "AgGateway";
            ISOTaskData.ManagementSoftwareVersion = "1.0";
            ISOTaskData.DataTransferOrigin = dataTransferOrigin;
            ISOTaskData.TaskControllerManufacturer = taskControllerManufacturer;
            ISOTaskData.TaskControllerVersion = taskControllerVersion;
            ISOTaskData.XmlComments.Add($"Export created {DateTime.Now}");   //191022 MSp

            //LinkList
            ISOTaskData.LinkList = new ISO11783_LinkList();
            ISOTaskData.LinkList.VersionMajor = 4;
            ISOTaskData.LinkList.VersionMinor = 2;
            ISOTaskData.LinkList.ManagementSoftwareManufacturer = "AgGateway";
            ISOTaskData.LinkList.ManagementSoftwareVersion = "1.0";
            ISOTaskData.LinkList.DataTransferOrigin = dataTransferOrigin;
            ISOTaskData.LinkList.TaskControllerManufacturer = taskControllerManufacturer;
            ISOTaskData.LinkList.TaskControllerVersion = taskControllerVersion;
            ISOTaskData.LinkList.FileVersion = "";
            UniqueIDMapper = new UniqueIdMapper(ISOTaskData.LinkList);

            //Crops
            if (adm.Catalog.Crops != null)
            {
                CropTypeMapper cropMapper = new CropTypeMapper(this, ProductGroupMapper);
                IEnumerable<ISOCropType> crops = cropMapper.ExportCropTypes(adm.Catalog.Crops);
                ISOTaskData.ChildElements.AddRange(crops);
            }

            //Products
            if (adm.Catalog.Products != null)
            {
                IEnumerable<Product> products = AdaptDataModel.Catalog.Products;
                if (products.Any())
                {
                    ProductMapper productMapper = new ProductMapper(this, ProductGroupMapper);
                    IEnumerable<ISOProduct> isoProducts = productMapper.ExportProducts(products);
                    ISOTaskData.ChildElements.AddRange(isoProducts);
                }
            }

            //Growers
            if (adm.Catalog.Growers != null)
            {
                CustomerMapper customerMapper = new CustomerMapper(this);
                IEnumerable<ISOCustomer> customers = customerMapper.Export(adm.Catalog.Growers);
                ISOTaskData.ChildElements.AddRange(customers);
            }

            //Farms
            if (adm.Catalog.Farms != null)
            {
                FarmMapper farmMapper = new FarmMapper(this);
                IEnumerable<ISOFarm> farms = farmMapper.Export(adm.Catalog.Farms);
                ISOTaskData.ChildElements.AddRange(farms);
            }

            //Fields & Cropzones
            if (adm.Catalog.Fields != null)
            {
                PartfieldMapper fieldMapper = new PartfieldMapper(this);
                foreach (Field field in adm.Catalog.Fields)
                {
                    IEnumerable<CropZone> fieldCropZones = adm.Catalog.CropZones.Where(c => c.FieldId == field.Id.ReferenceId);
                    if (fieldCropZones.Count() == 0)
                    {
                        //Export Field
                        ISOPartfield isoField = fieldMapper.ExportField(field);
                        ISOTaskData.ChildElements.Add(isoField);
                    }
                    else if (fieldCropZones.Count() == 1)
                    {
                        //Export Cropzone to retain the crop reference
                        ISOPartfield isoField = fieldMapper.ExportCropZone(fieldCropZones.First());
                        ISOTaskData.ChildElements.Add(isoField);
                    }
                    else
                    {
                        //Export both
                        ISOPartfield isoField = fieldMapper.ExportField(field);
                        ISOTaskData.ChildElements.Add(isoField);
                        foreach (CropZone cropZone in fieldCropZones)
                        {
                            ISOPartfield isoCropField = fieldMapper.ExportCropZone(cropZone);
                            ISOTaskData.ChildElements.Add(isoCropField);
                        }
                    }
                }
            }

            //Workers
            if (adm.Catalog.Persons != null)
            {
                WorkerMapper workerMapper = new WorkerMapper(this);
                IEnumerable<ISOWorker> workers = workerMapper.Export(adm.Catalog.Persons);
                ISOTaskData.ChildElements.AddRange(workers);
            }

            //Devices
            if (adm.Catalog.DeviceModels.Any())
            {
                DeviceMapper dvcMapper = new DeviceMapper(this);
                IEnumerable<ISODevice> devices = dvcMapper.ExportDevices(adm.Catalog.DeviceModels);
                ISOTaskData.ChildElements.AddRange(devices);
            }

            //Tasks
            if (AdaptDataModel.Documents.WorkItems.Any() || AdaptDataModel.Documents.LoggedData.Any())
            {
                TaskMapper taskMapper = new TaskMapper(this);
                if (AdaptDataModel.Documents.WorkItems != null)
                {
                    //Prescriptions
                    int gridType = 1;
                    if (Properties != null)
                    {
                        Int32.TryParse(Properties.GetProperty(ISOGrid.GridTypeProperty), out gridType);
                    }
                    if (gridType == 1 || gridType == 2)
                    {
                        IEnumerable<ISOTask> plannedTasks = taskMapper.Export(AdaptDataModel.Documents.WorkItems, gridType);
                        ISOTaskData.ChildElements.AddRange(plannedTasks);
                    }
                    else
                    {
                        AddError($"Invalid Grid Type {gridType}.  WorkItems will not be exported", null, "TaskDataMapper");
                    }
                }

                if (AdaptDataModel.Documents.LoggedData != null)
                {
                    //LoggedData 
                    IEnumerable<ISOTask> loggedTasks = taskMapper.Export(AdaptDataModel.Documents.LoggedData);
                    ISOTaskData.ChildElements.AddRange(loggedTasks.Where(t => !ISOTaskData.ChildElements.OfType<ISOTask>().Contains(t)));
                }
            }

            //Add Comments
            ISOTaskData.ChildElements.AddRange(CommentMapper.ExportedComments);

            //Add LinkList Attached File Reference
            if (ISOTaskData.LinkList.LinkGroups.Any())
            {
                ISOAttachedFile afe = new ISOAttachedFile();
                afe.FilenamewithExtension = "LINKLIST.XML";
                afe.Preserve = ISOEnumerations.ISOAttachedFilePreserve.Preserve;
                afe.ManufacturerGLN = string.Empty;
                afe.FileType = 1;
                ISOTaskData.ChildElements.Add(afe);
            }

            return ISOTaskData;
        }

        public ApplicationDataModel.ADM.ApplicationDataModel Import(ISO11783_TaskData taskData)
        {
            ISOTaskData = taskData;
            UniqueIDMapper = new UniqueIdMapper(ISOTaskData.LinkList);

            AdaptDataModel = new ApplicationDataModel.ADM.ApplicationDataModel();
            AdaptDataModel.Catalog = new Catalog() { Description = taskData.FilePath };
            AdaptDataModel.Documents = new Documents();

            //Comments 
            CodedCommentListMapper commentListMapper = new CodedCommentListMapper(this);
            CodedCommentMapper commentMapper = new CodedCommentMapper(this, commentListMapper);

            //Crops - several dependencies require import prior to Products
            IEnumerable<ISOCropType> crops = taskData.ChildElements.OfType<ISOCropType>();
            if (crops.Any())
            {
                CropTypeMapper cropMapper = new CropTypeMapper(this, ProductGroupMapper);
                AdaptDataModel.Catalog.Crops.AddRange(cropMapper.ImportCropTypes(crops));
            }

            //Products
            IEnumerable<ISOProduct> products = taskData.ChildElements.OfType<ISOProduct>();
            if (products.Any())
            {
                ProductMapper productMapper = new ProductMapper(this, ProductGroupMapper);
                IEnumerable<Product> adaptProducts = productMapper.ImportProducts(products);
                AdaptDataModel.Catalog.Products.AddRange(adaptProducts.Where(p => !AdaptDataModel.Catalog.Products.Contains(p)));
            }

            //Growers
            IEnumerable<ISOCustomer> customers = taskData.ChildElements.OfType<ISOCustomer>();
            if (customers.Any())
            {
                CustomerMapper customerMapper = new CustomerMapper(this);
                AdaptDataModel.Catalog.Growers.AddRange(customerMapper.Import(customers));
            }

            //Farms
            IEnumerable<ISOFarm> farms = taskData.ChildElements.OfType<ISOFarm>();
            if (farms.Any())
            {
                FarmMapper farmMapper = new FarmMapper(this);
                AdaptDataModel.Catalog.Farms.AddRange(farmMapper.Import(farms));
            }

            //Fields & Cropzones
            IEnumerable<ISOPartfield> partFields = taskData.ChildElements.OfType<ISOPartfield>();
            if (partFields.Any())
            {
                PartfieldMapper partFieldMapper = new PartfieldMapper(this);
                AdaptDataModel.Catalog.Fields.AddRange(partFieldMapper.ImportFields(partFields));
                AdaptDataModel.Catalog.CropZones.AddRange(partFieldMapper.ImportCropZones(partFields));
            }

            //Devices
            IEnumerable<ISODevice> devices = taskData.ChildElements.OfType<ISODevice>();
            if (devices.Any())
            {
                DeviceElementHierarchies = new DeviceElementHierarchies(devices, RepresentationMapper);

                DeviceMapper deviceMapper = new DeviceMapper(this);
                AdaptDataModel.Catalog.DeviceModels.AddRange(deviceMapper.ImportDevices(devices));
            }

            //Workers
            IEnumerable <ISOWorker> workers = taskData.ChildElements.OfType<ISOWorker>();
            if (workers.Any())
            {
                WorkerMapper workerMapper = new WorkerMapper(this);
                AdaptDataModel.Catalog.Persons.AddRange(workerMapper.Import(workers));
            }


            //Cultural Practices
            IEnumerable<ISOCulturalPractice> practices = taskData.ChildElements.OfType<ISOCulturalPractice>();
            if (practices.Any())
            {
                foreach (ISOCulturalPractice cpc in practices)
                {
                    (AdaptDataModel.Documents.WorkOrders as List<WorkOrder>).Add(new WorkOrder() { Description = cpc.CulturalPracticeDesignator });
                }
            }

            //OperationTechniques
            IEnumerable<ISOOperationTechnique> techniques = taskData.ChildElements.OfType<ISOOperationTechnique>();
            if (techniques.Any())
            {
                foreach (ISOOperationTechnique otq in techniques)
                {
                    (AdaptDataModel.Documents.WorkOrders as List<WorkOrder>).Add(new WorkOrder() { Description = otq.OperationTechniqueDesignator });
                }
            }

            IEnumerable<ISOTask> prescribedTasks = taskData.ChildElements.OfType<ISOTask>().Where(t => t.IsWorkItemTask);
            IEnumerable<ISOTask> loggedTasks = taskData.ChildElements.OfType<ISOTask>().Where(t => t.IsLoggedDataTask || t.TimeLogs.Any());
            if (prescribedTasks.Any() || loggedTasks.Any())
            {
                TaskMapper taskMapper = new TaskMapper(this);
                if (prescribedTasks.Any())
                {
                    //Prescribed Tasks
                    IEnumerable<WorkItem> workItems = taskMapper.ImportWorkItems(prescribedTasks);
                    AdaptDataModel.Documents.WorkItems = workItems;
                }

                if (loggedTasks.Any())
                {
                    //Logged Tasks
                    IEnumerable<LoggedData> loggedDatas = taskMapper.ImportLoggedDatas(loggedTasks);
                    AdaptDataModel.Documents.LoggedData = loggedDatas;

                    //Create Work Records for Logged Tasks
                    List<WorkRecord> workRecords = new List<WorkRecord>();
                    foreach (LoggedData data in loggedDatas)
                    {
                        WorkRecord record = new WorkRecord();
                        record.LoggedDataIds.Add(data.Id.ReferenceId);
                        if (data.SummaryId.HasValue)
                        {
                            record.SummariesIds.Add(data.SummaryId.Value);
                            Summary summary = AdaptDataModel.Documents.Summaries.FirstOrDefault(s => s.Id.ReferenceId == data.SummaryId);
                            if (summary != null)
                            {
                                summary.WorkRecordId = record.Id.ReferenceId;
                            }
                        }
                        workRecords.Add(record);
                    }
                    AdaptDataModel.Documents.WorkRecords = workRecords;
                }
            }

            return AdaptDataModel;
        }
    }
}
