using AgGateway.ADAPT.ApplicationDataModel;
using AgGateway.ADAPT.Plugins;
using NUnit.Framework;
using System.Globalization;
using System.Linq;

namespace IsoPluginTest
{
    [TestFixture]
    public class TaskControllerPluginCropLoaderTests
    {
        [Test]
        public void LoadCropAndVarietiesTest()
        {
            var unit = UnitFactory.Instance.GetUnitByDdi(1);
            // Setup
            var taskDocument = new  TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Crop\Crop1.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Crop\Crop2.xml");

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

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Crop\Crop3.xml");

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

        [TestCase(@"TestData\Crop\Crop4.xml")]
        [TestCase(@"TestData\Crop\Crop5.xml")]
        [TestCase(@"TestData\Crop\Crop6.xml")]
        public void CropWithMissingRequiredInfoTest(string testFileName)
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(testFileName);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Crops);
            Assert.AreEqual(0, taskDocument.Crops.Count);
        }
    }
}
