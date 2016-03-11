using AgGateway.ADAPT.ApplicationDataModel.Products;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.IsoPlugin.Writers
{
    internal class CropVarietyWriter : BaseWriter
    {
        internal CropVarietyWriter()
            : base(null, "CVT")
        {
        }

        internal void Write(XmlWriter writer, List<CropVariety> cropVarieties)
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

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", cropVarietyId);
            writer.WriteAttributeString("B", cropVariety.Description);
            writer.WriteEndElement();
        }
    }
}