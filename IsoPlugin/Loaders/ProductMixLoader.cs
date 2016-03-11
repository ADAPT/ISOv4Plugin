using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal class ProductMixLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, ProductMix> _productMixes;

        private ProductMixLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _productMixes = new Dictionary<string, ProductMix>();
        }

        internal static Dictionary<string, ProductMix> Load(TaskDataDocument taskDocument)
        {
            var loader = new ProductMixLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, ProductMix> Load()
        {
            LoadProductMixes(_rootNode.SelectNodes("PDT"));
            ProcessExternalNodes();

            return _productMixes;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'PDT')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadProductMixes(inputNodes);
            }
        }

        private void LoadProductMixes(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string productMixId;
                var productMix = LoadProductMix(inputNode, out productMixId);
                if (productMix != null)
                    _productMixes.Add(productMixId, productMix);
            }
        }

        private ProductMix LoadProductMix(XmlNode inputNode, out string productId)
        {
            productId = string.Empty;
            if (!IsProductMix(inputNode))
                return null;

            var productMix = GetProduct(inputNode);

            // Required fields. Do not proceed if they are missing
            productId = inputNode.GetXmlNodeValue("@A");
            productMix.Description = inputNode.GetXmlNodeValue("@B");
            if (productId == null || productMix.Description == null)
                return null;

            LoadTotalQuantity(inputNode, productMix);
            if (!LoadProductComponents(inputNode.SelectNodes("PRN"), productMix))
                return null;

            _taskDocument.LoadLinkedIds(productId, productMix.Id);
            return productMix;
        }

        private static bool IsProductMix(XmlNode inputNode)
        {
            var productType = inputNode.GetXmlNodeValue("@F");

            return !string.IsNullOrEmpty(productType) &&
                string.Equals(productType, "2", StringComparison.OrdinalIgnoreCase) &&
                inputNode.SelectNodes("PRN").Count > 0;
        }

        private ProductMix GetProduct(XmlNode inputNode)
        {
            var product = new ProductMix { ProductType = ProductTypeEnum.Mix, Form = ProductFormEnum.Unknown };

            var groupId = inputNode.GetXmlNodeValue("@C");
            if (string.IsNullOrEmpty(groupId))
                return product;

            var groupNode = _taskDocument.RootNode.SelectSingleNode(
                string.Format(CultureInfo.InvariantCulture, "PGP[@A='{0}']", groupId));
            if (groupNode == null)
                return product;

            var groupType = groupNode.GetXmlNodeValue("@C");
            if (string.IsNullOrEmpty(groupType))
                return product;

            if (string.Equals(groupType, "2", StringComparison.OrdinalIgnoreCase))
                product.ProductType = ProductTypeEnum.Variety;

            return product;
        }

        private static IsoUnit LoadUnit(XmlNode inputNode)
        {
            //var valueUnitId = inputNode.GetXmlNodeValue("@D");
            var quantityDdiValue = inputNode.GetXmlNodeValue("@E");

            int quantityDdi = Convert.ToInt32(quantityDdiValue, 16);

            switch (quantityDdi)
            {
                case 0x48: // DDI 72
                case 0x4B: // DDI 75
                case 0x4E: // DDI 78
                    return UnitFactory.Instance.GetUnitByDdi(quantityDdi);
            }

            return null;
        }

        private void LoadTotalQuantity(XmlNode inputNode, ProductMix productMix)
        {
            var quantityValue = inputNode.GetXmlNodeValue("@G");

            double quantity;
            if (!double.TryParse(quantityValue, NumberStyles.Float, CultureInfo.InvariantCulture, out quantity))
                return;

            var unit = LoadUnit(inputNode);
            var userUnit = _taskDocument.Units.FindById(inputNode.GetXmlNodeValue("@D"));

            var numericValue = new NumericValue(unit.ToAdaptUnit(), unit.ConvertFromIsoUnit(quantity));
            productMix.TotalQuantity = new NumericRepresentationValue(null, userUnit.ToAdaptUnit(), numericValue);
        }

        private bool LoadProductComponents(XmlNodeList inputNodes, ProductMix productMix)
        {
            List<Ingredient> components = new List<Ingredient>();
            foreach (XmlNode productRelationNode in inputNodes)
            {
                var component = LoadProductRelation(productRelationNode, productMix);
                if (component == null)
                    return false;
                components.Add(component);
            }

            _taskDocument.Ingredients.AddRange(components);
            return true;
        }

        private Ingredient LoadProductRelation(XmlNode productRelationNode, ProductMix productMix)
        {
            var productId = productRelationNode.GetXmlNodeValue("@A");
            var productQuantity = productRelationNode.GetXmlNodeValue("@B");

            if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(productQuantity))
                return null;

            long quantity;
            if (!long.TryParse(productQuantity, NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity) ||
                quantity < 0)
                return null;

            Product product;
            if (_taskDocument.Products.TryGetValue(productId, out product) == false)
                return null;

            var unit = _taskDocument.UnitsByItemId.FindById(productId);
            var numericValue = new NumericValue(unit.ToAdaptUnit(), unit.ConvertFromIsoUnit(quantity));

            var ingredient = new ActiveIngredient
            {
                Description = product.Description,
            };

            var productComponent = new ProductComponent
            {
                IngredientId = product.Id.ReferenceId,
                Quantity = new NumericRepresentationValue(null, numericValue.UnitOfMeasure, numericValue)
            };

            if (productMix.ProductComponents == null)
                productMix.ProductComponents = new List<ProductComponent>();
            productMix.ProductComponents.Add(productComponent);

            return ingredient;
        }
    }
}
