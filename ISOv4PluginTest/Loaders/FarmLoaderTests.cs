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
    public class FarmLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadFarmInfoTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Farm1);

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
            Assert.AreEqual("Street", farm.Value.ContactInfo.AddressLine1);
            Assert.AreEqual("City", farm.Value.ContactInfo.City);
            Assert.AreEqual("Country", farm.Value.ContactInfo.Country);
            Assert.AreEqual("PO Box", farm.Value.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode", farm.Value.ContactInfo.PostalCode);
            Assert.AreEqual("State", farm.Value.ContactInfo.StateOrProvince);
            Assert.IsEmpty(farm.Value.ContactInfo.Contacts);
            Assert.IsNotNull(farm.Value.GrowerId);
            Assert.AreEqual(taskDocument.Customers["CTR1"].Id.ReferenceId, farm.Value.GrowerId);
        }

        [Test]
        public void FarmUniqueIdIsSetCorrectly()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Farm1);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            var farm = taskDocument.Farms["FRM1"];
            const string expectedSource = "http://dictionary.isobus.net/isobus/";
            const CompoundIdentifierTypeEnum expectedType = CompoundIdentifierTypeEnum.String;

            Assert.AreEqual("FRM1", farm.Id.UniqueIds[0].Id);
            Assert.AreEqual(expectedType, farm.Id.UniqueIds[0].CiTypeEnum);
            Assert.AreEqual(expectedSource, farm.Id.UniqueIds[0].Source);
        }

        [Test]
        public void FarmWithMissingContactInfoTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Farm2);

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
            Assert.IsEmpty(farm.Value.ContactInfo.Contacts);
            Assert.IsNull(farm.Value.GrowerId);
        }

        [Test]
        public void FarmWithMissingCustomerTest()
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
            Assert.IsEmpty(farm.Value.ContactInfo.Contacts);
            Assert.IsNull(farm.Value.GrowerId);
        }

        [Test]
        public void FarmInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Farm4);
            var frmPath = Path.Combine(_directory, "FRM00004.xml");
            File.WriteAllText(frmPath, TestData.TestData.FRM00004);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Farms);
            Assert.AreEqual(2, taskDocument.Farms.Count);

            var farm = taskDocument.Farms["FRM1"];

            Assert.AreEqual("Farm1", farm.Description);
            Assert.IsNotNull(farm.ContactInfo);
            Assert.AreEqual("Street1", farm.ContactInfo.AddressLine1);
            Assert.AreEqual("City1", farm.ContactInfo.City);
            Assert.AreEqual("Country1", farm.ContactInfo.Country);
            Assert.AreEqual("PO Box1", farm.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode1", farm.ContactInfo.PostalCode);
            Assert.AreEqual("State1", farm.ContactInfo.StateOrProvince);
            Assert.IsEmpty(farm.ContactInfo.Contacts);
            Assert.AreEqual(taskDocument.Customers["CTR1"].Id.ReferenceId, farm.GrowerId.Value);

            farm = taskDocument.Farms["FRM2"];

            Assert.AreEqual("Farm2", farm.Description);
            Assert.IsNotNull(farm.ContactInfo);
            Assert.AreEqual("Street2", farm.ContactInfo.AddressLine1);
            Assert.AreEqual("City2", farm.ContactInfo.City);
            Assert.AreEqual("Country2", farm.ContactInfo.Country);
            Assert.AreEqual("PO Box2", farm.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode2", farm.ContactInfo.PostalCode);
            Assert.AreEqual("State2", farm.ContactInfo.StateOrProvince);
            Assert.IsEmpty(farm.ContactInfo.Contacts);
            Assert.AreEqual(taskDocument.Customers["CTR2"].Id.ReferenceId, farm.GrowerId.Value);
        }

        [Test]
        public void FarmWithMissingRequiredInfoTest()
        {
            var farms = new List<string>
            {
                TestData.TestData.Farm5,
                TestData.TestData.Farm6,
                TestData.TestData.Farm7,
            };

            for (int i = 0; i < farms.Count; i++)
            {
                // Setup
                var taskDocument = new TaskDataDocument();
                var path = Path.Combine(_directory, String.Format("farm{0}.xml", i));
                File.WriteAllText(path, farms[i]);

                // Act
                var result = taskDocument.LoadFromFile(path);

                // Verify
                Assert.IsTrue(result);
                Assert.IsNotNull(taskDocument.Farms);
                Assert.AreEqual(0, taskDocument.Farms.Count); 
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
