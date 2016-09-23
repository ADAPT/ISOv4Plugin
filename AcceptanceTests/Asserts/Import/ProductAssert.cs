using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AcceptanceTests.Asserts.Import
{
    public class ProductAssert
    {
        public static void AreEqual(XmlNodeList productNodes, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            for (int i = 0; i < productNodes.Count; i++)
            {
                if (productNodes[i].GetXmlAttribute("F") == "1")//fertilizerProduct
                {
                    var matchingFertilizer = catalog.Products.Single(fp => fp.Id.FindIsoId() == productNodes[i].GetXmlAttribute("A")) as FertilizerProduct;
                    FertilizerProductAssert.AreEqual(productNodes[i], matchingFertilizer, linkList);
                    
                } 
                else if (productNodes[i].GetXmlAttribute("F") == "2")//productMix
                {
                    var matchingProductMix = catalog.Products.Single(pm => pm.Id.FindIsoId() == productNodes[i].GetXmlAttribute("A")) as ProductMix;
                    ProductMixAssert.AreEqual(productNodes[i], productNodes, matchingProductMix, catalog, linkList);
                }
            }
        }
    }
}