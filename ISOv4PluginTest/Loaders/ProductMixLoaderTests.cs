using System;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class ProductMixLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadProductMixTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.ProductMix1);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.ProductMix2);
            File.WriteAllText(Path.Combine(_directory, "PDT00002.xml"), TestData.TestData.PDT00002);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.ProductMix3);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.ProductMix4);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.ProductMixes);
            Assert.AreEqual(0, taskDocument.ProductMixes.Count);

            Assert.IsNotNull(taskDocument.Ingredients);
            Assert.AreEqual(0, taskDocument.Ingredients.Count);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }
    }
}
