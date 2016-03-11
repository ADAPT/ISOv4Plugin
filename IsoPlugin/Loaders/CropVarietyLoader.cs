using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Products;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal static class CropVarietyLoader
    {
        internal static Dictionary<string, CropVariety> Load(XmlNodeList inputNodes)
        {
            var varieties = new Dictionary<string, CropVariety>();
            foreach (XmlNode inputNode in inputNodes)
            {
                string varietyId;
                var variety = LoadVariety(inputNode, out varietyId);
                if (variety != null)
                    varieties.Add(varietyId, variety);
            }

            return varieties;
        }

        private static CropVariety LoadVariety(XmlNode inputNode, out string varietyId)
        {
            varietyId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(varietyId))
                return null;

            var variety = new CropVariety { ProductType = ProductTypeEnum.Variety };
            variety.Description = inputNode.GetXmlNodeValue("@B");
            if (string.IsNullOrEmpty(variety.Description))
                return null;

            return variety;
        }
    }
}
