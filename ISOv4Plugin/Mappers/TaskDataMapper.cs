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
        ApplicationDataModel.ADM.ApplicationDataModel Import(ISO11783_TaskData taskData, ISO11783_LinkList linkList);
    }

    public class TaskDataMapper : ITaskDataMapper
    {
        public TaskDataMapper(string dataPath, Properties properties)
        {
            BaseFolder = dataPath;
            RepresentationMapper = new RepresentationMapper();
            DDIs = DdiLoader.Ddis;
            Properties = properties;
        }

        public string BaseFolder { get; private set; }
        public Properties Properties { get; private set; }
        public Dictionary<int, string> ISOIdMap { get; set; }
        public Dictionary<string, int?> ADAPTIdMap { get; set; }

        public ApplicationDataModel.ADM.ApplicationDataModel AdaptDataModel { get; private set; }
        public ISO11783_TaskData ISOTaskData { get; private set; }
        public ISO11783_LinkList ISOLinkList { get; private set; }
        public UniqueIdMapper UniqueIDMapper { get; set; }
        public DeviceElementHierarchies DeviceElementHierarchies { get; set; }

        internal RepresentationMapper RepresentationMapper { get; private set; }
        internal Dictionary<int, DdiDefinition> DDIs { get; private set; }

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


        public ISO11783_TaskData Export(ApplicationDataModel.ADM.ApplicationDataModel adm)
        {
            AdaptDataModel = adm;
            ISOIdMap = new Dictionary<int, string>();

            //TaskData
            ISOTaskData = new ISO11783_TaskData();
            ISOTaskData.VersionMajor = 4;
            ISOTaskData.VersionMinor = 0;
            ISOTaskData.ManagementSoftwareManufacturer = "AgGateway";
            ISOTaskData.ManagementSoftwareVersion = "1.0";
            ISOTaskData.DataTransferOrigin = ISOEnumerations.ISOTaskDataTransferOrigin.FMIS;
            ISOTaskData.TaskControllerManufacturer = "";
            ISOTaskData.TaskControllerVersion = "";

            //LinkList
            ISOLinkList = new ISO11783_LinkList();
            ISOLinkList.VersionMajor = 4;
            ISOLinkList.VersionMinor = 0;
            ISOLinkList.ManagementSoftwareManufacturer = "AgGateway";
            ISOLinkList.ManagementSoftwareVersion = "1.0";
            ISOLinkList.DataTransferOrigin = ISOEnumerations.ISOTaskDataTransferOrigin.FMIS;
            ISOLinkList.TaskControllerManufacturer = "";
            ISOLinkList.TaskControllerVersion = "";
            ISOLinkList.FileVersion = "";
            UniqueIDMapper = new UniqueIdMapper(ISOLinkList);

            //Crops
            if (adm.Catalog.Crops != null)
            {
                CropTypeMapper cropMapper = new CropTypeMapper(this);
                IEnumerable<ISOCropType> crops = cropMapper.ExportCropTypes(adm.Catalog.Crops);
                ISOTaskData.ChildElements.AddRange(crops);
            }

            //Non-Variety Products
            if (adm.Catalog.Products != null)
            {
                IEnumerable<Product> nonSeedProducts = AdaptDataModel.Catalog.Products.Where(p => p.ProductType != ProductTypeEnum.Variety);
                if (nonSeedProducts.Any())
                {
                    ProductMapper productMapper = new ProductMapper(this);
                    IEnumerable<ISOProduct> products = productMapper.ExportProducts(nonSeedProducts);
                    ISOTaskData.ChildElements.AddRange(products);
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
                    if (gridType != 1 && gridType != 2)
                    {
                        throw new ApplicationException("Invalid Grid Type Set for Export");
                    }
                    
                    IEnumerable<ISOTask> plannedTasks = taskMapper.Export(AdaptDataModel.Documents.WorkItems, gridType);
                    ISOTaskData.ChildElements.AddRange(plannedTasks);
                }

                if (AdaptDataModel.Documents.LoggedData != null)
                {
                    //LoggedData 
                    IEnumerable<ISOTask> loggedTasks = taskMapper.Export(AdaptDataModel.Documents.LoggedData);
                    ISOTaskData.ChildElements.AddRange(loggedTasks);
                }
            }

            //Add Comments
            ISOTaskData.ChildElements.AddRange(CommentMapper.ExportedComments);

            //Add LinkList Attached File Reference
            if (ISOLinkList.LinkGroups.Any())
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

        public ApplicationDataModel.ADM.ApplicationDataModel Import(ISO11783_TaskData taskData, ISO11783_LinkList linkList)
        {
            ISOTaskData = taskData;
            ISOLinkList = linkList ?? new ISO11783_LinkList() { VersionMajor = 4, VersionMinor = 0, DataTransferOrigin = ISOEnumerations.ISOTaskDataTransferOrigin.FMIS, FileVersion = "", ManagementSoftwareManufacturer = "AgGateway", ManagementSoftwareVersion = "1.0", TaskControllerManufacturer = "", TaskControllerVersion = "" };
            UniqueIDMapper = new UniqueIdMapper(ISOLinkList);

            AdaptDataModel = new ApplicationDataModel.ADM.ApplicationDataModel();
            ADAPTIdMap = new Dictionary<string, int?>();
            AdaptDataModel.Catalog = new Catalog() { Description = "ISO TaskData" };
            AdaptDataModel.Documents = new Documents();

            //Comments 
            CodedCommentListMapper commentListMapper = new CodedCommentListMapper(this);
            CodedCommentMapper commentMapper = new CodedCommentMapper(this, commentListMapper);

            //Crops
            IEnumerable<ISOCropType> crops = taskData.ChildElements.OfType<ISOCropType>();
            if (crops.Any())
            {
                CropTypeMapper cropMapper = new CropTypeMapper(this);
                AdaptDataModel.Catalog.Crops.AddRange(cropMapper.ImportCropTypes(crops));
            }

            //Products
            IEnumerable<ISOProduct> products = taskData.ChildElements.OfType<ISOProduct>();
            if (products.Any())
            {
                ProductMapper productMapper = new ProductMapper(this);
                AdaptDataModel.Catalog.Products.AddRange(productMapper.ImportProducts(products));
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

            IEnumerable<ISOTask> prescribedTasks = taskData.ChildElements.OfType<ISOTask>().Where(t => t.IsWorkItemTask);
            IEnumerable<ISOTask> loggedTasks = taskData.ChildElements.OfType<ISOTask>().Where(t => t.IsLoggedDataTask);
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
