using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.ReferenceLayers;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class FieldLoader
    {
        private TaskDataDocument _taskDocument;
        private XmlNode _rootNode;
        private string _baseFolder;
        private Dictionary<string, Field> _fields;

        private FieldLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _fields = new Dictionary<string, Field>();
        }

        internal static Dictionary<string, Field> Load(TaskDataDocument taskDocument)
        {
            var fieldLoader = new FieldLoader(taskDocument);

            return fieldLoader.Load();
        }

        private Dictionary<string, Field> Load()
        {
            LoadFields(_rootNode.SelectNodes("PFD"));
            ProcessExternalNodes();

            return _fields;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'PFD')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadFields(inputNodes);
            }
        }

        private void LoadFields(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string fieldId;
                var field = LoadField(inputNode, out fieldId);
                if (field != null)
                    _fields.Add(fieldId, field);
            }
        }

        private Field LoadField(XmlNode inputNode, out string fieldId)
        {
            var field = new Field();

            // Required fields. Do not proceed if they are missing
            fieldId = inputNode.GetXmlNodeValue("@A");
            field.Description = inputNode.GetXmlNodeValue("@C");
            LoadArea(inputNode.GetXmlNodeValue("@D"), field);
            if (fieldId == null || field.Description == null || field.Area == null)
                return null;

            // Optional fields
            AssignFarm(inputNode, field);
            LoadFieldBoundary(inputNode, field);

            LoadGuidance(inputNode, field);

            LoadCropZone(inputNode, field, fieldId);

            _taskDocument.LoadLinkedIds(fieldId, field.Id);
            return field;
        }

        private static void LoadArea(string inputValue, Field field)
        {
            long areaValue;
            if (inputValue.ParseValue(out areaValue) == false || areaValue < 0)
                return;

            var numericValue = new NumericValue(new CompositeUnitOfMeasure("m2").ToModelUom(), areaValue);
            field.Area = new NumericRepresentationValue(null, numericValue.UnitOfMeasure, numericValue);
        }

        private void AssignFarm(XmlNode inputNode, Field field)
        {
            var farmId = inputNode.GetXmlNodeValue("@F");
            if (string.IsNullOrEmpty(farmId))
                return;

            var farm = _taskDocument.Farms.FindById(farmId);
            if (farm != null)
                field.FarmId = farm.Id.ReferenceId;
        }

        private void LoadFieldBoundary(XmlNode inputNode, Field field)
        {
            var polygon = ShapeLoader.LoadPolygon(inputNode.SelectNodes("PLN"));
            if (polygon != null)
            {
                var fieldBoundary = new FieldBoundary
                {
                    FieldId = field.Id.ReferenceId,
                    SpatialData = polygon
                };

                _taskDocument.FieldBoundaries.Add(fieldBoundary);

                field.ActiveBoundaryId = fieldBoundary.Id.ReferenceId;
            }
        }

        private void LoadCropZone(XmlNode inputNode, Field field, string fieldId)
        {
            var cropId = inputNode.GetXmlNodeValue("@G");
            if (string.IsNullOrEmpty(cropId))
                return;

            Crop crop;
            if (!_taskDocument.Crops.TryGetValue(cropId, out crop))
                return;

            var cropZone = new CropZone();
            cropZone.CropId = crop.Id.ReferenceId;
            cropZone.FieldId = field.Id.ReferenceId;
            cropZone.Description = field.Description;
            cropZone.Area = field.Area.Copy();
            cropZone.GuidanceGroupIds = field.GuidanceGroupIds != null ? field.GuidanceGroupIds.ToList() : null;

            _taskDocument.CropZones[fieldId] = cropZone;
        }

        private void LoadGuidance(XmlNode inputNode, Field field)
        {
            var guidanceGroups = GuidanceGroupLoader.LoadGuidanceGroups(inputNode.SelectNodes("GGP"));
            if (guidanceGroups != null)
            {
                foreach (var guidanceGroup in guidanceGroups)
                {
                    _taskDocument.GuidanceGroups.Add(guidanceGroup.Key, guidanceGroup.Value);
                    field.GuidanceGroupIds = guidanceGroups.Values.Select(x => x.Group.Id.ReferenceId).ToList();

                    _taskDocument.LoadLinkedIds(guidanceGroup.Key, guidanceGroup.Value.Group.Id);

                    guidanceGroup.Value.Patterns.All(x => { _taskDocument.LoadLinkedIds(x.Key, x.Value.Id); return true; });
                }
            }
        }
    }
}