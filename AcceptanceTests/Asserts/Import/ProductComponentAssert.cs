using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class ProductComponentAssert
    {
        public static void AreEqual(XmlNodeList prnNodes, List<ProductComponent> productComponents, XmlNodeList productNodes, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            for (int i = 0; i < prnNodes.Count; i++)
            {
                AreEqual(prnNodes[i], productComponents[i], productNodes, catalog, linkList);
            }
        }

        private static void AreEqual(XmlNode prnNode, ProductComponent productComponent, XmlNodeList productNodes, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            var productNode = FindMatchingProductNode(prnNode.GetXmlAttribute("A"), productNodes);
            if (productNode == null)
                return;

            var quantityDdi = Int32.Parse(productNode.GetXmlAttribute("E"));
            var uom = new RepresentationMapper().GetUnitForDdi(quantityDdi);
            Assert.AreEqual(uom, productComponent.Quantity.Value.UnitOfMeasure);
            Assert.AreEqual(prnNode.GetXmlAttribute("B"), productComponent.Quantity.Value.Value);

            var ingredient = catalog.Ingredients.Single(x => x.Id.ReferenceId == productComponent.IngredientId);
            Assert.AreEqual(productNode.GetXmlAttribute("B"), ingredient.Description);
        }

        private static XmlNode FindMatchingProductNode(string productReferenceId, XmlNodeList productNodes)
        {
            for (int i = 0; i < productNodes.Count; i++)
            {
                if (productNodes[i].GetXmlAttribute("A") == productReferenceId)
                    return productNodes[i];
            }
            return null;
        }
    }
}