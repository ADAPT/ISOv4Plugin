using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class CropLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadCropAndVarietiesTest()
        {
            var unit = UnitFactory.Instance.GetUnitByDdi(1);
            // Setup
            var taskDocument = new  TaskDataDocument();
            var path = Path.Combine(_directory, "Crop1.xml");
            File.WriteAllText(path, TestData.TestData.Crop1);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Crops);
            Assert.AreEqual(1, taskDocument.Crops.Count);

            var crop = taskDocument.Crops.First();
            Assert.AreEqual("CTP1", crop.Key);
            Assert.AreEqual("Crop 1", crop.Value.Name);

            Assert.IsNotNull(taskDocument.CropVarieties);
            Assert.AreEqual(2, taskDocument.CropVarieties.Count);
            for (int i = 0; i < taskDocument.CropVarieties.Count; i++)
            {
                var variety = taskDocument.CropVarieties.ElementAt(i);
                Assert.AreEqual(crop.Value.Id.ReferenceId, variety.Value.CropId);
                Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "CVT{0}", i + 1), variety.Key);
                Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "Variety {0}", i + 1), variety.Value.Description);
                Assert.AreEqual(ProductTypeEnum.Variety, variety.Value.ProductType);
            }
        }

        [Test]
        public void CropWithRequiredFieldsOnlyTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "Crop2.xml");
            File.WriteAllText(path, TestData.TestData.Crop2);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Crops);
            Assert.AreEqual(1, taskDocument.Crops.Count);

            var crop = taskDocument.Crops.First();
            Assert.AreEqual("CTP1", crop.Key);
            Assert.AreEqual("Crop 1", crop.Value.Name);

            Assert.IsNotNull(taskDocument.CropVarieties);
            Assert.AreEqual(0, taskDocument.CropVarieties.Count);
        }

        [Test]
        public void CropInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "Crop3.xml");
            File.WriteAllText(path, TestData.TestData.Crop3);
            var ctpPath = Path.Combine(_directory, "CTP00003.xml");
            File.WriteAllText(ctpPath, TestData.TestData.CTP00003);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Crops);
            Assert.AreEqual(2, taskDocument.Crops.Count);

            var crop1 = taskDocument.Crops["CTP1"];
            Assert.AreEqual("Crop 1", crop1.Name);

            var crop2 = taskDocument.Crops["CTP2"];
            Assert.AreEqual("Crop 2", crop2.Name);

            Assert.IsNotNull(taskDocument.CropVarieties);
            Assert.AreEqual(4, taskDocument.CropVarieties.Count);
            for (int i = 0; i < taskDocument.CropVarieties.Count; i++)
            {
                var variety = taskDocument.CropVarieties.ElementAt(i);
                Assert.That(variety.Value.CropId, Is.EqualTo(crop1.Id.ReferenceId).Or.EqualTo(crop2.Id.ReferenceId));
                Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "CVT{0}", i + 1), variety.Key);
                Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "Variety {0}", i + 1), variety.Value.Description);
                Assert.AreEqual(ProductTypeEnum.Variety, variety.Value.ProductType);
            }

        }

        [Test]
        public void CropWithMissingRequiredInfoTest()
        {
            var crops = new List<String>
            {
                TestData.TestData.Crop4,
                TestData.TestData.Crop5,
                TestData.TestData.Crop6,
            };

            for (int i = 0; i < crops.Count; i++)
            {
                // Setup
                var taskDocument = new TaskDataDocument();
                var path = Path.Combine(_directory, String.Format("crop{0}.xml", i));
                File.WriteAllText(path, crops[i]);

                // Act
                var result = taskDocument.LoadFromFile(path);

                // Verify
                Assert.IsTrue(result);
                Assert.IsNotNull(taskDocument.Crops);
                Assert.AreEqual(0, taskDocument.Crops.Count);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, true);
        }
        
    }
}
