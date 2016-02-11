using AgGateway.ADAPT.Plugins;
using NUnit.Framework;
using System.Linq;

namespace IsoPluginTest
{
    [TestFixture]
    public class FarmLoaderTests
    {
        [Test]
        public void LoadFarmInfoTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Farm\Farm1.xml");

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
            Assert.IsNull(farm.Value.ContactInfo.Contacts);
            Assert.IsNotNull(farm.Value.GrowerId);
            Assert.AreEqual(taskDocument.Customers["CTR1"].Id.ReferenceId, farm.Value.GrowerId);
        }

        [Test]
        public void FarmWithMissingContactInfoTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Farm\Farm2.xml");

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
        public void FarmWithMissingCustomerTest()
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
        public void FarmInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(@"TestData\Farm\Farm4.xml");

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
            Assert.IsNull(farm.ContactInfo.Contacts);
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
            Assert.IsNull(farm.ContactInfo.Contacts);
            Assert.AreEqual(taskDocument.Customers["CTR2"].Id.ReferenceId, farm.GrowerId.Value);
        }

        [TestCase(@"TestData\Farm\Farm5.xml")]
        [TestCase(@"TestData\Farm\Farm6.xml")]
        [TestCase(@"TestData\Farm\Farm7.xml")]
        public void FarmWithMissingRequiredInfoTest(string testFileName)
        {
            // Setup
            var taskDocument = new TaskDataDocument();

            // Act
            var result = taskDocument.LoadFromFile(testFileName);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Farms);
            Assert.AreEqual(0, taskDocument.Farms.Count);
        }
    }
}
