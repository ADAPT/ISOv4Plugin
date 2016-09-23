using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class CropAssert
    {
        public static void AreEqual(XmlNodeList ctpNodes, List<Crop> crops, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            for (var i = 0; i < ctpNodes.Count; i++)
            {
                AreEqual(ctpNodes[i], crops[i], catalog, linkList);
            }
        }

        private static void AreEqual(XmlNode ctpNode, Crop crop, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            UniqueIdAssert.AreEqual(linkList, ctpNode.GetXmlAttribute("A"), crop.Id.UniqueIds);
            Assert.AreEqual(ctpNode.GetXmlAttribute("A"), crop.Id.FindIsoId());
            Assert.AreEqual(ctpNode.GetXmlAttribute("B"), crop.Name);

            var matchingCropVarieties = catalog.Products.Where(x => x is CropVariety).Cast<CropVariety>().Where(cv => cv.CropId == crop.Id.ReferenceId).ToList();
            CropVarietyAssert.AreEqual(ctpNode.SelectNodes("CVT"), matchingCropVarieties, linkList);
        }
    }
}