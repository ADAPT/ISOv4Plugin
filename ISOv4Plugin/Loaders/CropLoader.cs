using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class CropLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, Crop> _crops;

        private CropLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _crops = new Dictionary<string, Crop>();
        }

        public static Dictionary<string, Crop> Load(TaskDataDocument taskDocument)
        {
            var cropLoader = new CropLoader(taskDocument);

            return cropLoader.Load();
        }

        private Dictionary<string, Crop> Load()
        {
            LoadCrops(_rootNode.SelectNodes("CTP"));
            ProcessExternalNodes();

            return _crops;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'CTP')]");
            for (int i = 0; i < externalNodes.Count; i++)
            {
                var inputNodes = externalNodes[i].LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadCrops(inputNodes);
            }
        }

        private void LoadCrops(XmlNodeList inputNodes)
        {
            for (int i = 0; i < inputNodes.Count; i++)
            {
                string cropId;
                var crop = LoadCrop(inputNodes[i], out cropId);
                if (crop != null)
                    _crops.Add(cropId, crop);
            }
        }

        private Crop LoadCrop(XmlNode inputNode, out string cropId)
        {
            var crop = new Crop();

            // Required fields. Do not proceed if they are missing
            cropId = inputNode.GetXmlNodeValue("@A");
            crop.Name = inputNode.GetXmlNodeValue("@B");
            if (cropId == null || crop.Name == null)
                return null;
            crop.Id.UniqueIds.Add(ImportHelper.CreateUniqueId(cropId));

            // Optional fields
            LoadVarieties(inputNode, crop);

            _taskDocument.LoadLinkedIds(cropId, crop.Id);
            return crop;
        }

        private void LoadVarieties(XmlNode inputNode, Crop crop)
        {
            var varieties = CropVarietyLoader.Load(inputNode.SelectNodes("CVT"));

            foreach (var variety in varieties)
            {
                variety.Value.CropId = crop.Id.ReferenceId;

                _taskDocument.CropVarieties.Add(variety.Key, variety.Value);

                _taskDocument.LoadLinkedIds(variety.Key, variety.Value.Id);
            }
        }
    }
}
