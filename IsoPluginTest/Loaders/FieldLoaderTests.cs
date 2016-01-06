using AgGateway.ADAPT.Plugins;
using NUnit.Framework;
using System.Linq;

namespace IsoPluginTest
{
    [TestFixture]
    public class TaskControllerPluginFieldLoaderTests
    {
        [Test]
        public void LoadFieldTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Field\Field1.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Fields);
            Assert.AreEqual(2, taskDocument.Fields.Count);

            var field = taskDocument.Fields["PFD1"];
            Assert.AreEqual("Field 1", field.Description);
            Assert.AreEqual(12345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);

            field = taskDocument.Fields["PFD2"];
            Assert.AreEqual("Field 2", field.Description);
            Assert.AreEqual(22345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);

            Assert.AreEqual(1, taskDocument.CropZones.Count);
            var cropZone = taskDocument.CropZones.First();
            Assert.AreEqual(field.Id.ReferenceId, cropZone.Value.FieldId);

            Assert.AreEqual(1, taskDocument.Crops.Count);
            var crop = taskDocument.Crops.First();
            Assert.AreEqual(crop.Value.Id.ReferenceId, cropZone.Value.CropId);

            Assert.AreEqual(1, taskDocument.Farms.Count);
            var farm = taskDocument.Farms.First();
            Assert.AreEqual(farm.Value.Id.ReferenceId, field.FarmId);
        }

        [Test, Ignore("In progress")]
        public void FieldWithMissingBoundaryTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Field\Field2.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Farms);
            Assert.AreEqual(1, taskDocument.Farms.Count);
            var farm = taskDocument.Farms.First();
            Assert.AreEqual("FRM1", farm.Key);
            Assert.AreEqual("Farm", farm.Value.Description);
            Assert.IsNotNull(farm.Value.ContactInfo);
            Assert.IsNull(farm.Value.ContactInfo.AddressLine1);
            Assert.IsNull(farm.Value.ContactInfo.City);
            Assert.IsNull(farm.Value.ContactInfo.Country);
            Assert.IsNull(farm.Value.ContactInfo.PoBoxNumber);
            Assert.IsNull(farm.Value.ContactInfo.PostalCode);
            Assert.IsNull(farm.Value.ContactInfo.StateOrProvince);
            Assert.IsNull(farm.Value.ContactInfo.Contacts);
            Assert.IsNull(farm.Value.GrowerId);
        }

        [Test]
        public void FarmWithMissingFarmTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Field\Field3.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Fields);
            Assert.AreEqual(2, taskDocument.Fields.Count);

            var field = taskDocument.Fields["PFD1"];
            Assert.AreEqual("Field 1", field.Description);
            Assert.AreEqual(12345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);
            Assert.IsNull(field.FarmId);

            field = taskDocument.Fields["PFD2"];
            Assert.AreEqual("Field 2", field.Description);
            Assert.AreEqual(22345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);
            Assert.IsNull(field.FarmId);

            Assert.AreEqual(1, taskDocument.CropZones.Count);
            var cropZone = taskDocument.CropZones.First();
            Assert.AreEqual(field.Id.ReferenceId, cropZone.Value.FieldId);

            Assert.AreEqual(1, taskDocument.Crops.Count);
            var crop = taskDocument.Crops.First();
            Assert.AreEqual(crop.Value.Id.ReferenceId, cropZone.Value.CropId);

            Assert.AreEqual(1, taskDocument.Farms.Count);
        }

        [Test, Ignore("In progress")]
        public void FarmWithMissingGuidanceTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Farm\Farm3.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Farms);
            Assert.AreEqual(1, taskDocument.Farms.Count);
            var farm = taskDocument.Farms.First();
            Assert.AreEqual("FRM1", farm.Key);
            Assert.AreEqual("Farm", farm.Value.Description);
            Assert.IsNotNull(farm.Value.ContactInfo);
            Assert.IsNull(farm.Value.ContactInfo.AddressLine1);
            Assert.IsNull(farm.Value.ContactInfo.City);
            Assert.IsNull(farm.Value.ContactInfo.Country);
            Assert.IsNull(farm.Value.ContactInfo.PoBoxNumber);
            Assert.IsNull(farm.Value.ContactInfo.PostalCode);
            Assert.IsNull(farm.Value.ContactInfo.StateOrProvince);
            Assert.IsNull(farm.Value.ContactInfo.Contacts);
            Assert.IsNull(farm.Value.GrowerId);
        }

        [Test]
        public void FarmWithMissingCropZoneTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Field\Field5.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Fields);
            Assert.AreEqual(1, taskDocument.Fields.Count);

            var field = taskDocument.Fields["PFD1"];
            Assert.AreEqual("Field 1", field.Description);
            Assert.AreEqual(12345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);

            Assert.AreEqual(0, taskDocument.CropZones.Count);

            Assert.AreEqual(1, taskDocument.Crops.Count);
            var crop = taskDocument.Crops.First();

            Assert.AreEqual(1, taskDocument.Farms.Count);
            var farm = taskDocument.Farms.First();
            Assert.AreEqual(farm.Value.Id.ReferenceId, field.FarmId);
        }

        [Test]
        public void FieldInExternalFileTest()
        {
            // Setup
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Field\Field6.xml");

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Fields);
            Assert.AreEqual(2, taskDocument.Fields.Count);

            var field = taskDocument.Fields["PFD1"];
            Assert.AreEqual("Field 1", field.Description);
            Assert.AreEqual(12345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);

            field = taskDocument.Fields["PFD2"];
            Assert.AreEqual("Field 2", field.Description);
            Assert.AreEqual(22345, field.Area.Value.Value);
            Assert.AreEqual("m2", field.Area.Value.UnitOfMeasure.Code);

            Assert.AreEqual(1, taskDocument.CropZones.Count);
            var cropZone = taskDocument.CropZones.First();
            Assert.AreEqual(field.Id.ReferenceId, cropZone.Value.FieldId);

            Assert.AreEqual(1, taskDocument.Crops.Count);
            var crop = taskDocument.Crops.First();
            Assert.AreEqual(crop.Value.Id.ReferenceId, cropZone.Value.CropId);

            Assert.AreEqual(1, taskDocument.Farms.Count);
            var farm = taskDocument.Farms.First();
            Assert.AreEqual(farm.Value.Id.ReferenceId, field.FarmId);
        }

        [TestCase(@"TestData\Field\Field7.xml")]
        [TestCase(@"TestData\Field\Field8.xml")]
        [TestCase(@"TestData\Field\Field9.xml")]
        [TestCase(@"TestData\Field\Field10.xml")]
        public void FieldWithMissingRequiredInfoTest(string testFileName)
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(testFileName);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Fields);
            Assert.AreEqual(0, taskDocument.Fields.Count);
        }
        
    }
}
