using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Products;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class CropVarietyWriter : BaseWriter
    {
        public CropVarietyWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "CVT")
        {
        }

        public void Write(XmlWriter writer, List<CropVariety> cropVarieties)
        {
            if (cropVarieties.Count == 0)
                return;

            foreach (var cropVariety in cropVarieties)
            {
                WriteCropVariety(writer, cropVariety);
            }
        }

        private void WriteCropVariety(XmlWriter writer, CropVariety cropVariety)
        {
            var cropVarietyId = GenerateId();
            TaskWriter.Ids.Add(cropVarietyId, cropVariety.Id);

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", cropVarietyId);
            writer.WriteAttributeString("B", cropVariety.Description);
            writer.WriteEndElement();

            TaskWriter.CropVarieties[cropVariety.Id.ReferenceId] = cropVarietyId;
        }
    }
}