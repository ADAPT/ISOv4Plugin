using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
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
                    var matchingFertilizer = catalog.FertilizerProducts.Single(fp => fp.Id.FindIsoId() == productNodes[i].GetXmlAttribute("A"));
                    FertilizerProductAssert.AreEqual(productNodes[i], matchingFertilizer, linkList);
                    
                } 
                else if (productNodes[i].GetXmlAttribute("F") == "2")//productMix
                {
                    var matchingProductMix = catalog.ProductMixes.Single(pm => pm.Id.FindIsoId() == productNodes[i].GetXmlAttribute("A"));
                    ProductMixAssert.AreEqual(productNodes[i], productNodes, matchingProductMix, catalog, linkList);
                }
            }
        }
    }
}