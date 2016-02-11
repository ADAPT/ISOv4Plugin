using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.Plugins;
using NUnit.Framework;
using System.Linq;

namespace IsoPluginTest
{
    [TestFixture]
    public class ProductMixLoaderTests
    {
        [Test]
        public void LoadProductMixTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\ProductMix\ProductMix1.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            var product = taskDocument.Products.First();
            Assert.AreEqual("PDT2", product.Key);
            Assert.AreEqual("Product 2", product.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.Value.ProductType);
            Assert.IsTrue(taskDocument.UnitsByItemId.ContainsKey(product.Key));
            Assert.AreEqual("l", taskDocument.UnitsByItemId[product.Key].Code);

            Assert.IsNotNull(taskDocument.ProductMixes);
            Assert.AreEqual(1, taskDocument.ProductMixes.Count);

            var productMix = taskDocument.ProductMixes.First();
            Assert.AreEqual("PDT1", productMix.Key);
            Assert.AreEqual("Product 1", productMix.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Mix, productMix.Value.ProductType);
            Assert.IsNotNull(productMix.Value.TotalQuantity);
            Assert.AreEqual("l", productMix.Value.TotalQuantity.Value.UnitOfMeasure.Code);

            Assert.AreEqual(1, productMix.Value.ProductComponents.Count);
            var productComponent = productMix.Value.ProductComponents.First();
            Assert.AreEqual(1, productComponent.Quantity.Value.Value);
            Assert.AreEqual("l", productComponent.Quantity.Value.UnitOfMeasure.Code);
            Assert.AreEqual(productComponent.IngredientId, product.Value.Id.ReferenceId);

            Assert.IsNotNull(taskDocument.Ingredients);
            Assert.AreEqual(1, taskDocument.Ingredients.Count);

            var ingredient = taskDocument.Ingredients.First() as ActiveIngredient;
            Assert.IsNotNull(ingredient);
        }

        [Test]
        public void ProductMixInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\ProductMix\ProductMix2.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            var product = taskDocument.Products.First();
            Assert.AreEqual("PDT2", product.Key);
            Assert.AreEqual("Product 2", product.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.Value.ProductType);
            Assert.IsTrue(taskDocument.UnitsByItemId.ContainsKey(product.Key));
            Assert.AreEqual("l", taskDocument.UnitsByItemId[product.Key].Code);

            Assert.IsNotNull(taskDocument.ProductMixes);
            Assert.AreEqual(1, taskDocument.ProductMixes.Count);

            var productMix = taskDocument.ProductMixes.First();
            Assert.AreEqual("PDT1", productMix.Key);
            Assert.AreEqual("Product 1", productMix.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Mix, productMix.Value.ProductType);
            Assert.IsNotNull(productMix.Value.TotalQuantity);
            Assert.AreEqual("l", productMix.Value.TotalQuantity.Value.UnitOfMeasure.Code);

            Assert.AreEqual(1, productMix.Value.ProductComponents.Count);
            var productComponent = productMix.Value.ProductComponents.First();
            Assert.AreEqual(1, productComponent.Quantity.Value.Value);
            Assert.AreEqual("l", productComponent.Quantity.Value.UnitOfMeasure.Code);
            Assert.AreEqual(productComponent.IngredientId, product.Value.Id.ReferenceId);

            Assert.IsNotNull(taskDocument.Ingredients);
            Assert.AreEqual(1, taskDocument.Ingredients.Count);

            var ingredient = taskDocument.Ingredients.First() as ActiveIngredient;
            Assert.IsNotNull(ingredient);
        }

        [Test]
        public void ProductMixWithMissingComponentsTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\ProductMix\ProductMix3.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.ProductMixes);
            Assert.AreEqual(0, taskDocument.ProductMixes.Count);

            Assert.IsNotNull(taskDocument.Ingredients);
            Assert.AreEqual(0, taskDocument.Ingredients.Count);
        }

        [Test]
        public void ProductMixWithInvalidProductTypeTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\ProductMix\ProductMix4.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.ProductMixes);
            Assert.AreEqual(0, taskDocument.ProductMixes.Count);

            Assert.IsNotNull(taskDocument.Ingredients);
            Assert.AreEqual(0, taskDocument.Ingredients.Count);
        }
    }
}
