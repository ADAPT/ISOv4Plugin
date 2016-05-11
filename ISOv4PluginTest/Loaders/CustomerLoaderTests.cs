using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class CustomerLoaderTests
    {
        private string _directory;

        [SetUp]
        public void Setup()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [Test]
        public void LoadCustomerInfoTest()
        {
            // Setup
            var taskDocument = new  TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Customer1);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Customers);
            Assert.AreEqual(1, taskDocument.Customers.Count);
            var customer = taskDocument.Customers.First();
            Assert.AreEqual("CTR1", customer.Key);
            Assert.AreEqual("Last, First", customer.Value.Name);
            Assert.IsNotNull(customer.Value.ContactInfo);
            Assert.AreEqual("Street", customer.Value.ContactInfo.AddressLine1);
            Assert.AreEqual("City", customer.Value.ContactInfo.City);
            Assert.AreEqual("Country", customer.Value.ContactInfo.Country);
            Assert.AreEqual("PO Box", customer.Value.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode", customer.Value.ContactInfo.PostalCode);
            Assert.AreEqual("State", customer.Value.ContactInfo.StateOrProvince);
            Assert.IsNotNull(customer.Value.ContactInfo.Contacts);
            Assert.AreEqual(4, customer.Value.ContactInfo.Contacts.Count);
            Assert.AreEqual("Phone", customer.Value.ContactInfo.Contacts[0].Number);
            Assert.AreEqual(ContactTypeEnum.FixedPhone, customer.Value.ContactInfo.Contacts[0].Type);
            Assert.AreEqual("Mobile", customer.Value.ContactInfo.Contacts[1].Number);
            Assert.AreEqual(ContactTypeEnum.MobilePhone, customer.Value.ContactInfo.Contacts[1].Type);
            Assert.AreEqual("Fax", customer.Value.ContactInfo.Contacts[2].Number);
            Assert.AreEqual(ContactTypeEnum.Fax, customer.Value.ContactInfo.Contacts[2].Type);
            Assert.AreEqual("Email", customer.Value.ContactInfo.Contacts[3].Number);
            Assert.AreEqual(ContactTypeEnum.Email, customer.Value.ContactInfo.Contacts[3].Type);
        }

        [Test]
        public void CustomerUniqueIdIsSetCorrectly()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Customer1);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            var customer = taskDocument.Customers["CTR1"];
            const string expectedSource = "http://dictionary.isobus.net/isobus/";
            const CompoundIdentifierTypeEnum expectedType = CompoundIdentifierTypeEnum.String;

            Assert.AreEqual("CTR1", customer.Id.UniqueIds[0].Id);
            Assert.AreEqual(expectedType, customer.Id.UniqueIds[0].CiTypeEnum);
            Assert.AreEqual(expectedSource, customer.Id.UniqueIds[0].Source);
        }

        [Test]
        public void CustomerWithMissingPhonesTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Customer2);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Customers);
            Assert.AreEqual(1, taskDocument.Customers.Count);
            var customer = taskDocument.Customers.First();
            Assert.AreEqual("CTR1", customer.Key);
            Assert.AreEqual("Last, First", customer.Value.Name);
            Assert.IsNotNull(customer.Value.ContactInfo);
            Assert.AreEqual("Street", customer.Value.ContactInfo.AddressLine1);
            Assert.AreEqual("City", customer.Value.ContactInfo.City);
            Assert.AreEqual("Country", customer.Value.ContactInfo.Country);
            Assert.AreEqual("PO Box", customer.Value.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode", customer.Value.ContactInfo.PostalCode);
            Assert.AreEqual("State", customer.Value.ContactInfo.StateOrProvince);
            Assert.IsNotNull(customer.Value.ContactInfo.Contacts);
            Assert.AreEqual(0, customer.Value.ContactInfo.Contacts.Count);
        }

        [Test]
        public void CustomerWithMissingContactInfoTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Customer3);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Customers);
            Assert.AreEqual(1, taskDocument.Customers.Count);
            var customer = taskDocument.Customers.First();
            Assert.AreEqual("CTR1", customer.Key);
            Assert.AreEqual("Last, First", customer.Value.Name);
            Assert.IsNotNull(customer.Value.ContactInfo);
            Assert.IsNull(customer.Value.ContactInfo.AddressLine1);
            Assert.IsNull(customer.Value.ContactInfo.City);
            Assert.IsNull(customer.Value.ContactInfo.Country);
            Assert.IsNull(customer.Value.ContactInfo.PoBoxNumber);
            Assert.IsNull(customer.Value.ContactInfo.PostalCode);
            Assert.IsNull(customer.Value.ContactInfo.StateOrProvince);
            Assert.IsNotNull(customer.Value.ContactInfo.Contacts);
            Assert.AreEqual(0, customer.Value.ContactInfo.Contacts.Count);
        }

        [Test]
        public void CustomerWithRequiredFieldsOnlyTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Customer4);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Customers);
            Assert.AreEqual(1, taskDocument.Customers.Count);
            var customer = taskDocument.Customers.First();
            Assert.AreEqual("CTR1", customer.Key);
            Assert.AreEqual("Last", customer.Value.Name);
            Assert.IsNotNull(customer.Value.ContactInfo);
            Assert.IsNull(customer.Value.ContactInfo.AddressLine1);
            Assert.IsNull(customer.Value.ContactInfo.City);
            Assert.IsNull(customer.Value.ContactInfo.Country);
            Assert.IsNull(customer.Value.ContactInfo.PoBoxNumber);
            Assert.IsNull(customer.Value.ContactInfo.PostalCode);
            Assert.IsNull(customer.Value.ContactInfo.StateOrProvince);
            Assert.IsNotNull(customer.Value.ContactInfo.Contacts);
            Assert.AreEqual(0, customer.Value.ContactInfo.Contacts.Count);
        }

        [Test]
        public void CustomerInExternalFileTest()
        {
            // Setup
            var taskDocument = new TaskDataDocument();
            var path = Path.Combine(_directory, "test.xml");
            File.WriteAllText(path, TestData.TestData.Customer5);
            File.WriteAllText(Path.Combine(_directory, "CTR00005.xml"), TestData.TestData.CTR00005);

            // Act
            var result = taskDocument.LoadFromFile(path);

            // Verify
            Assert.IsTrue(result);
            Assert.IsNotNull(taskDocument.Customers);
            Assert.AreEqual(2, taskDocument.Customers.Count);

            var customer = taskDocument.Customers["CTR1"];

            Assert.AreEqual("Last1, First1", customer.Name);
            Assert.IsNotNull(customer.ContactInfo);
            Assert.AreEqual("Street1", customer.ContactInfo.AddressLine1);
            Assert.AreEqual("City1", customer.ContactInfo.City);
            Assert.AreEqual("Country1", customer.ContactInfo.Country);
            Assert.AreEqual("PO Box1", customer.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode1", customer.ContactInfo.PostalCode);
            Assert.AreEqual("State1", customer.ContactInfo.StateOrProvince);
            Assert.IsNotNull(customer.ContactInfo.Contacts);
            Assert.AreEqual(4, customer.ContactInfo.Contacts.Count);
            Assert.AreEqual("Phone1", customer.ContactInfo.Contacts[0].Number);
            Assert.AreEqual(ContactTypeEnum.FixedPhone, customer.ContactInfo.Contacts[0].Type);
            Assert.AreEqual("Mobile1", customer.ContactInfo.Contacts[1].Number);
            Assert.AreEqual(ContactTypeEnum.MobilePhone, customer.ContactInfo.Contacts[1].Type);
            Assert.AreEqual("Fax1", customer.ContactInfo.Contacts[2].Number);
            Assert.AreEqual(ContactTypeEnum.Fax, customer.ContactInfo.Contacts[2].Type);
            Assert.AreEqual("Email1", customer.ContactInfo.Contacts[3].Number);
            Assert.AreEqual(ContactTypeEnum.Email, customer.ContactInfo.Contacts[3].Type);

            customer = taskDocument.Customers["CTR2"];

            Assert.AreEqual("Last2, First2", customer.Name);
            Assert.IsNotNull(customer.ContactInfo);
            Assert.AreEqual("Street2", customer.ContactInfo.AddressLine1);
            Assert.AreEqual("City2", customer.ContactInfo.City);
            Assert.AreEqual("Country2", customer.ContactInfo.Country);
            Assert.AreEqual("PO Box2", customer.ContactInfo.PoBoxNumber);
            Assert.AreEqual("PostalCode2", customer.ContactInfo.PostalCode);
            Assert.AreEqual("State2", customer.ContactInfo.StateOrProvince);
            Assert.IsNotNull(customer.ContactInfo.Contacts);
            Assert.AreEqual(4, customer.ContactInfo.Contacts.Count);
            Assert.AreEqual("Phone2", customer.ContactInfo.Contacts[0].Number);
            Assert.AreEqual(ContactTypeEnum.FixedPhone, customer.ContactInfo.Contacts[0].Type);
            Assert.AreEqual("Mobile2", customer.ContactInfo.Contacts[1].Number);
            Assert.AreEqual(ContactTypeEnum.MobilePhone, customer.ContactInfo.Contacts[1].Type);
            Assert.AreEqual("Fax2", customer.ContactInfo.Contacts[2].Number);
            Assert.AreEqual(ContactTypeEnum.Fax, customer.ContactInfo.Contacts[2].Type);
            Assert.AreEqual("Email2", customer.ContactInfo.Contacts[3].Number);
            Assert.AreEqual(ContactTypeEnum.Email, customer.ContactInfo.Contacts[3].Type);
        }

        [Test]
        public void CustomerWithMissingRequiredInfoTest()
        {
            var customers = new List<string>
            {
                TestData.TestData.Customer6,
                TestData.TestData.Customer7,
                TestData.TestData.Customer8,
            };

            for (int i = 0; i < customers.Count; i++)
            {
                var path = Path.Combine(_directory, String.Format("customer{0}.xml", i));
                File.WriteAllText(path, customers[i]);

                // Setup
                var taskDocument = new TaskDataDocument();

                // Act
                var result = taskDocument.LoadFromFile(path);

                // Verify
                Assert.IsTrue(result);
                Assert.IsNotNull(taskDocument.Customers);
                Assert.AreEqual(0, taskDocument.Customers.Count);
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
