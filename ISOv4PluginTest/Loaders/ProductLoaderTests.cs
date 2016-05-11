using System;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class ProductLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadLiquidProductTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product1);


            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            var product = taskDocument.Products.First();
            Assert.AreEqual("PDT1", product.Key);
            Assert.AreEqual("Product 1", product.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.Value.ProductType);
            Assert.IsTrue(taskDocument.UnitsByItemId.ContainsKey(product.Key));
            Assert.AreEqual("l", taskDocument.UnitsByItemId[product.Key].Code);
        }

        [Test]
        public void LoadDryProductTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product2);


            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            var product = taskDocument.Products.First();
            Assert.AreEqual("PDT1", product.Key);
            Assert.AreEqual("Product 1", product.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.Value.ProductType);
            Assert.IsTrue(taskDocument.UnitsByItemId.ContainsKey(product.Key));
            Assert.AreEqual("kg", taskDocument.UnitsByItemId[product.Key].Code);
        }

        [Test]
        public void LoadSeedProductTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product3);


            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            var product = taskDocument.Products.First();
            Assert.AreEqual("PDT1", product.Key);
            Assert.AreEqual("Product 1", product.Value.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.Value.ProductType);
            Assert.IsTrue(taskDocument.UnitsByItemId.ContainsKey(product.Key));
            Assert.AreEqual("count", taskDocument.UnitsByItemId[product.Key].Code);
        }

        [Test]
        public void ProductWithRequiredFieldsOnlyTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product4);


            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            var product = taskDocument.Products.First();
            Assert.AreEqual("PDT1", product.Key);
            Assert.AreEqual("Product 1", product.Value.Description);
        }

        [Test]
        public void ProductInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product5);
            File.WriteAllText(Path.Combine(_directory, "PDT00005.xml"), TestData.TestData.PDT00005);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(2, taskDocument.Products.Count);

            var product = taskDocument.Products["PDT1"];
            Assert.AreEqual("Product 1", product.Description);

            var product2 = taskDocument.Products["PDT2"];
            Assert.AreEqual("Product 2", product2.Description);
        }

        [Test]
        public void ProductWithMissingRequiredFieldTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product6);
            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(0, taskDocument.Products.Count);
        }

        [Test]
        public void ProductWithProductGroupTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product7);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(2, taskDocument.Products.Count);

            var product = taskDocument.Products["PDT1"];
            Assert.AreEqual("Product 1", product.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.ProductType);

            product = taskDocument.Products["PDT2"];
            Assert.AreEqual("Product 2", product.Description);
            Assert.AreEqual(ProductTypeEnum.Variety, product.ProductType);
        }

        [Test]
        public void ProductWithInvalidProductTypeTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Product8);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Products);
            Assert.AreEqual(1, taskDocument.Products.Count);

            Assert.IsNotNull(taskDocument.ProductMixes);
            Assert.AreEqual(0, taskDocument.ProductMixes.Count);

            var product = taskDocument.Products["PDT1"];
            Assert.AreEqual("Product 1", product.Description);
            Assert.AreEqual(ProductTypeEnum.Generic, product.ProductType);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }
    }
}
