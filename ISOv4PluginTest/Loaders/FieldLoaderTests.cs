using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class FieldLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadFieldTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field1);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            //File.WriteAllText(path, TestData.TestData.Field2);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field3);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Farm3);

            // Act
            var result = taskDocument.LoadFromFile(path);

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
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field5);

            // Act
            var result = taskDocument.LoadFromFile(path);

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

            Assert.AreEqual(1, taskDocument.Farms.Count);
            var farm = taskDocument.Farms.First();
            Assert.AreEqual(farm.Value.Id.ReferenceId, field.FarmId);
        }

        [Test]
        public void FieldInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field6);
            var pfdPath = Path.Combine(_directory, "PFD00006.xml");
            File.WriteAllText(pfdPath, TestData.TestData.PFD00006);

            // Act
            var result = taskDocument.LoadFromFile(path);

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

        [Test]
        public void FieldUniqueIdIsSetCorrectly()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field6);
            var pfdPath = Path.Combine(_directory, "PFD00006.xml");
            File.WriteAllText(pfdPath, TestData.TestData.PFD00006);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Fields);
            Assert.AreEqual(2, taskDocument.Fields.Count);

            const string expectedSource = "http://dictionary.isobus.net/isobus/";
            const CompoundIdentifierTypeEnum expectedType = CompoundIdentifierTypeEnum.String;

            var field = taskDocument.Fields["PFD1"];
            Assert.AreEqual("PFD1", field.Id.UniqueIds[0].Id);
            Assert.AreEqual(expectedType, field.Id.UniqueIds[0].CiTypeEnum);
            Assert.AreEqual(expectedSource, field.Id.UniqueIds[0].Source);

            field = taskDocument.Fields["PFD2"];
            Assert.AreEqual("PFD2", field.Id.UniqueIds[0].Id);
            Assert.AreEqual(expectedType, field.Id.UniqueIds[0].CiTypeEnum);
            Assert.AreEqual(expectedSource, field.Id.UniqueIds[0].Source);
        }

        [Test]
        public void FieldWithExteriorBoundary()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field11);

            // Act
            taskDocument.LoadFromFile(path);

            // Verify
            var field = taskDocument.Fields["PFD1"];
            var fieldBoundary = taskDocument.FieldBoundaries[0];

            Assert.AreEqual(field.Id.ReferenceId, fieldBoundary.FieldId);
            Assert.AreEqual(5, fieldBoundary.SpatialData.Polygons[0].ExteriorRing.Points.Count);
        }

        [Test]
        public void FieldWithExteriorAndInteriorBoundary()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Field12);

            // Act
            taskDocument.LoadFromFile(path);

            // Verify
            var field = taskDocument.Fields["PFD1"];
            var fieldBoundary = taskDocument.FieldBoundaries[0];

            Assert.AreEqual(field.Id.ReferenceId, fieldBoundary.FieldId);
            Assert.AreEqual(11, fieldBoundary.SpatialData.Polygons[0].ExteriorRing.Points.Count);
            Assert.AreEqual(1, fieldBoundary.SpatialData.Polygons[0].InteriorRings.Count);
            Assert.AreEqual(12, fieldBoundary.SpatialData.Polygons[0].InteriorRings[0].Points.Count);
        }

        [Test]
        public void FieldWithMissingRequiredInfoTest()
        {
            var fields = new List<string>
            {
                TestData.TestData.Field7,
                TestData.TestData.Field8,
                TestData.TestData.Field9,
                TestData.TestData.Field10,
            };

            for (int i = 0; i < fields.Count; i++)
            {
                // Setup
                var taskDocument = new TaskDataDocument();
                var path = Path.Combine(_directory, String.Format("field{0}.xml", i));
                File.WriteAllText(path, fields[i]);

                // Act
                var result = taskDocument.LoadFromFile(path);

                // Verify
                Assert.IsTrue(result);
                Assert.IsNotNull(taskDocument.Fields);
                Assert.AreEqual(0, taskDocument.Fields.Count);
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
