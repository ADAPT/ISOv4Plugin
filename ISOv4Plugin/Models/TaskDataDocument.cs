using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.ReferenceLayers;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TaskDataDocument
    {
        private XmlDocument _taskDataXmlDocument;

        public XmlNode RootNode { get; private set; }
        public string BaseFolder { get; private set; }
        public IsoVersionEnum IsoVersion { get; private set; }

        public IReadOnlyCollection<IError> Errors { get; private set; }

        public Dictionary<string, Grower> Customers { get; private set; }
        public Dictionary<string, Farm> Farms { get; private set; }
        public Dictionary<string, Field> Fields { get; private set; }
        public List<FieldBoundary> FieldBoundaries { get; private set; }
        public Dictionary<string, Crop> Crops { get; private set; }
        public Dictionary<string, CropVariety> CropVarieties { get; private set; }
        public Dictionary<string, CropZone> CropZones { get; private set; }
        public List<ContactInfo> Contacts { get; private set; }

        public List<Machine> Machines { get; private set; }
        public List<MachineModel> MachineModels { get; private set; }
        public List<MachineSeries> MachineSeries { get; private set; }
        public Dictionary<byte, MachineType> MachineTypes { get; private set; }

        public Dictionary<string, Product> Products { get; private set; }

        public Dictionary<string, GuidanceGroupDescriptor> GuidanceGroups { get; private set; }
        public Dictionary<string, ProductMix> ProductMixes { get; private set; }
        public List<Ingredient> Ingredients { get; private set; }

        public List<RasterGridPrescription> RasterPrescriptions { get; private set; }

        public List<LoggedData> Tasks { get; private set; }
        public List<GuidanceAllocation> GuidanceAllocations { get; private set; }
        public List<GuidanceShift> GuidanceShifts { get; private set; }

        public Dictionary<string, Person> Workers { get; private set; }
        public Dictionary<string, CodedComment> Comments { get; private set; }


        // Helper property to allow lookup of associated unit of measure based on object id
        public Dictionary<string, IsoUnit> UnitsByItemId { get; private set; }
        // Helper property to allow lookup of VPN data based on id
        public Dictionary<string, ValuePresentation> Units { get; private set; }
        // Helper property to store linked ids by object id
        public Dictionary<string, List<UniqueId>> LinkedIds { get; set; }


        public TaskDataDocument()
        {
            FieldBoundaries = new List<FieldBoundary>();
            GuidanceGroups = new Dictionary<string, GuidanceGroupDescriptor>();
            CropVarieties = new Dictionary<string, CropVariety>();
            Ingredients = new List<Ingredient>();
            CropZones = new Dictionary<string, CropZone>();
            UnitsByItemId = new Dictionary<string, IsoUnit>();
            GuidanceAllocations = new List<GuidanceAllocation>();
            Contacts = new List<ContactInfo>();
        }

        public void LoadLinkedIds(string elementId, CompoundIdentifier id)
        {
            var linkedIds = LinkedIds.FindById(elementId);
            if (linkedIds == null || linkedIds.Count == 0)
                return;

            id.UniqueIds.AddRange(linkedIds);
        }

        public bool LoadFromFile(string taskDataFile)
        {
            if (!LoadXmlFile(taskDataFile))
                return false;

            if (!VerifyIsoVersion())
                return false;

            LinkedIds = LinkListLoader.Load(this);
            Units = UnitLoader.Load(this);
            Customers = CustomerLoader.Load(this);
            Farms = FarmLoader.Load(this);
            Crops = CropLoader.Load(this);
            Fields = FieldLoader.Load(this);
            Products = ProductLoader.Load(this);
            ProductMixes = ProductMixLoader.Load(this);
            Workers = WorkerLoader.Load(this);
            Comments = CommentLoader.Load(this);
            Tasks = TaskLoader.Load(this);
            RasterPrescriptions = PrescriptionLoader.Load(this);

            return true;
        }

        private bool LoadXmlFile(string taskDataFile)
        {
            BaseFolder = Path.GetDirectoryName(taskDataFile);
            try
            {
                _taskDataXmlDocument = new XmlDocument();
                _taskDataXmlDocument.Load(taskDataFile);
            }
            catch (XmlException ex)
            {
                SetError(ex);
                return false;
            }
            catch (IOException ex)
            {
                SetError(ex);
                return false;
            }
            return true;
        }

        private void SetError(Exception ex)
        {
            List<IError> errors = new List<IError>();

            errors.Add(new TCError
            {
                Id = ex.HResult.ToString(CultureInfo.InvariantCulture),
                Description = ex.Message,
                Source = ex.Source,
                StackTrace = ex.StackTrace
            });

            Errors = new ReadOnlyCollection<IError>(errors);
        }

        private void SetError(string message)
        {
            List<IError> errors = new List<IError>();

            errors.Add(new TCError
            {
                Id = string.Empty,
                Description = message,
                Source = "IsoPlugin",
                StackTrace = string.Empty
            });

            Errors = new ReadOnlyCollection<IError>(errors);
        }

        private bool VerifyIsoVersion()
        {
            RootNode = _taskDataXmlDocument.SelectSingleNode("ISO11783_TaskData");
            if (RootNode == null)
            {
                SetError("Missing required ISO11783_TaskData");
                return false;
            }

            var majorVersion = RootNode.GetXmlNodeValue("@VersionMajor");
            IsoVersionEnum isoVersion;
            if (majorVersion == null || Enum.TryParse(majorVersion, true, out isoVersion) == false)
            {
                SetError("Missing required VersionMajor attribute or its value is not supported");
                return false;
            }
            IsoVersion = isoVersion;

            return true;
        }
    }
}
