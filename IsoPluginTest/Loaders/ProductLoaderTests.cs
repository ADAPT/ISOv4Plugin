using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.Plugins;
using NUnit.Framework;
using System.Linq;

namespace IsoPluginTest
{
    [TestFixture]
    public class ProductLoaderTests
    {
        [Test]
        public void LoadLiquidProductTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product1.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product2.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product3.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product4.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product5.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product6.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product7.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Product\Product8.xml");

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
    }
}
