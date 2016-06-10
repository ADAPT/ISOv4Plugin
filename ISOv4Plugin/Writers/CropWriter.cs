using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class CropWriter : BaseWriter
    {
        private CropVarietyWriter _cropVarietyWriter;

        private CropWriter(TaskDocumentWriter taskWriter)
            :base(taskWriter, "CTP")
        {
            _cropVarietyWriter = new CropVarietyWriter(taskWriter);
        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Crops == null ||
                taskWriter.DataModel.Catalog.Crops.Count == 0)
                return;

            var writer = new CropWriter(taskWriter);
            writer.WriteCrops(taskWriter.RootWriter);
        }

        private void WriteCrops(XmlWriter writer)
        {
            foreach (var crop in TaskWriter.DataModel.Catalog.Crops)
            {
                var cropId = WriteCrop(writer, crop);
                TaskWriter.Crops[crop.Id.ReferenceId] = cropId;
            }
        }

        private string WriteCrop(XmlWriter writer, Crop crop)
        {
            var cropId = crop.Id.FindIsoId() ?? GenerateId();
            TaskWriter.Ids.Add(cropId, crop.Id);

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", cropId);
            writer.WriteAttributeString("B", crop.Name);

            WriteVarieties(writer, crop.Id);

            writer.WriteEndElement();

            return cropId;
        }

        private void WriteVarieties(XmlWriter writer, CompoundIdentifier cropId)
        {
            if (TaskWriter.DataModel.Catalog.CropVarieties == null ||
                TaskWriter.DataModel.Catalog.CropVarieties.Count == 0)
                return;

            var cropVarieties = new List<CropVariety>();
            foreach (var cropVariety in TaskWriter.DataModel.Catalog.CropVarieties)
            {
                if (cropVariety.CropId == cropId.ReferenceId)
                    cropVarieties.Add(cropVariety);
            }

            _cropVarietyWriter.Write(writer, cropVarieties);
        }
    }
}