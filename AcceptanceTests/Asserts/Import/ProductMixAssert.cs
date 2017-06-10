using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class ProductMixAssert
    {
        public static void AreEqual(XmlNode productNode, XmlNodeList productNodes, MixProduct productMix, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            if (productNode.GetXmlAttribute("A") == null || productNode.GetXmlAttribute("B") == null)
                return;

            UniqueIdAssert.AreEqual(linkList, productNode.GetXmlAttribute("A"), productMix.Id.UniqueIds);
            Assert.AreEqual(productNode.GetXmlAttribute("A"), productMix.Id.FindIsoId());
            Assert.AreEqual(productNode.GetXmlAttribute("B"), productMix.Description);

            var prnNodes = productNode.SelectNodes("PRN");
            ProductComponentAssert.AreEqual(prnNodes, productMix.ProductComponents, productNodes, catalog, linkList);
        }
    }
}