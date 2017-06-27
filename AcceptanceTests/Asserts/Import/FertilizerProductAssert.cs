using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class FertilizerProductAssert
    {
        public static void AreEqual(XmlNode productNode, CropNutritionProduct fertilizerProduct, Dictionary<string, List<UniqueId>> linkList)
        {
            if (productNode.GetXmlAttribute("A") == null || productNode.GetXmlAttribute("B") == null)
                return;

            UniqueIdAssert.AreEqual(linkList, productNode.GetXmlAttribute("A"), fertilizerProduct.Id.UniqueIds);
            Assert.AreEqual(productNode.GetXmlAttribute("A"), fertilizerProduct.Id.FindIsoId());
            Assert.AreEqual(productNode.GetXmlAttribute("B"), fertilizerProduct.Description);
        }
    }
}