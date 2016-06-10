using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class LinkListWriterTest
    {
        private string _exportPath;
        private Dictionary<string, CompoundIdentifier> _ids;
        private string _taskdataPath;

        [SetUp]
        public void Setup()
        {
            _exportPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _taskdataPath = Path.Combine(_exportPath, "TASKDATA");
            Directory.CreateDirectory(_taskdataPath);
            _ids = new Dictionary<string, CompoundIdentifier>();
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenFileIsCreated()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            Assert.IsTrue(File.Exists(linkListFilename));
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenVersionMajorAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual("4", iso11783LinkList.Attributes["VersionMajor"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenVersionMinorAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual("0", iso11783LinkList.Attributes["VersionMinor"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenManagementSoftwareManufacturerAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual("AgGateway", iso11783LinkList.Attributes["ManagementSoftwareManufacturer"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenManagementSoftwareVersionAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual("1.0", iso11783LinkList.Attributes["ManagementSoftwareVersion"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenDataTransferOriginAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual(((int)ISO11783_TaskDataDataTransferOrigin.Item1).ToString(CultureInfo.InvariantCulture), iso11783LinkList.Attributes["DataTransferOrigin"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenTaskControllerManufacturerAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual("", iso11783LinkList.Attributes["TaskControllerManufacturer"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenTaskControllerVersionAdded()
        {
            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var iso11783LinkList = linkListDocument.DocumentElement;

            Assert.IsNotNull(iso11783LinkList);
            Assert.AreEqual("", iso11783LinkList.Attributes["TaskControllerVersion"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenLgp1Added()
        {
            var uniqueId = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                CiTypeEnum = CompoundIdentifierTypeEnum.UUID,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId> { uniqueId } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);

            var lgps = linkListDocument.DocumentElement.SelectNodes("LGP");

            Assert.AreEqual("LGP1", lgps[0].Attributes["A"].Value);
            Assert.AreEqual("1", lgps[0].Attributes["B"].Value);
        }

        [Test]
        public void GivenPathAndIdsWhenWriteThenLgp2Added()
        {
            var uniqueId = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                CiTypeEnum = CompoundIdentifierTypeEnum.UUID,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId> { uniqueId } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);

            var lgps = linkListDocument.DocumentElement.SelectNodes("LGP");

            Assert.AreEqual("LGP2", lgps[1].Attributes["A"].Value);
            Assert.AreEqual("2", lgps[1].Attributes["B"].Value);
        }

        [Test]
        public void GivenPathAndIdWithTypeUuidWhenWriteThenLstAddedToLGP1()
        {
            var uniqueId = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                CiTypeEnum = CompoundIdentifierTypeEnum.UUID,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId>{ uniqueId } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var lsts = linkListDocument.DocumentElement.SelectNodes("LGP")[0].ChildNodes;

            Assert.AreEqual("TSK5", lsts[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId.Id, lsts[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId.Source, lsts[0].Attributes["C"].Value);
        }

        [Test]
        public void GivenPathAndIdWithTypeLongIntWhenWriteThenLstAddedToLGP2()
        {
            var uniqueId = new UniqueId
            {
                Id = "123",
                CiTypeEnum = CompoundIdentifierTypeEnum.LongInt,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId>{ uniqueId } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var lsts = linkListDocument.DocumentElement.SelectNodes("LGP")[1].ChildNodes;

            Assert.AreEqual("TSK5", lsts[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId.Id, lsts[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId.Source, lsts[0].Attributes["C"].Value);
        }

        [Test]
        public void GivenPathAndIdWithTypeStringWhenWriteThenLstAddedToLGP2()
        {
            var uniqueId = new UniqueId
            {
                Id = "bob",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId>{ uniqueId } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var lsts = linkListDocument.DocumentElement.SelectNodes("LGP")[1].ChildNodes;

            Assert.AreEqual("TSK5", lsts[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId.Id, lsts[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId.Source, lsts[0].Attributes["C"].Value);
        }

        [Test]
        public void GivenPathAndIdWithTypeUuidAndStringWhenWriteThenLstAddedToCorrectGroup()
        {
            var uniqueId1 = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                CiTypeEnum = CompoundIdentifierTypeEnum.UUID,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var uniqueId2 = new UniqueId
            {
                Id = "456",
                CiTypeEnum = CompoundIdentifierTypeEnum.LongInt,
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId> { uniqueId1, uniqueId2 } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var group1Links = linkListDocument.DocumentElement.SelectNodes("LGP")[0].ChildNodes;
            var group2Links = linkListDocument.DocumentElement.SelectNodes("LGP")[1].ChildNodes;

            Assert.AreEqual("TSK5", group1Links[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId1.Id, group1Links[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId1.Source, group1Links[0].Attributes["C"].Value);
            Assert.AreEqual("TSK5", group2Links[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId2.Id, group2Links[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId2.Source, group2Links[0].Attributes["C"].Value);
        }

        [Test]
        public void GivenPathAndIdsWithMultipleUniqueIdsAndMultipleSourcesWhenWriteThenLstAdded()
        {
            var uniqueId1 = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var uniqueId2 = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                Source = "urn:epc:id:sgln:0000000.00000.2",
            };
            var id = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId> { uniqueId1, uniqueId2 } };
            _ids.Add("TSK5", id);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(_taskdataPath, _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var linkGroup1Lists = linkListDocument.DocumentElement.SelectNodes("LGP")[0].ChildNodes;
            var linkGroup3Lists = linkListDocument.DocumentElement.SelectNodes("LGP")[2].ChildNodes;

            Assert.AreEqual("TSK5", linkGroup1Lists[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId1.Id, linkGroup1Lists[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId1.Source, linkGroup1Lists[0].Attributes["C"].Value);
            Assert.AreEqual("TSK5", linkGroup3Lists[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId2.Id, linkGroup3Lists[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId2.Source, linkGroup3Lists[0].Attributes["C"].Value);
        }

        [Test]
        public void GivenPathAndIdWithMultipleIdsWhenWriteThenLstsAdded()
        {
            var uniqueId1 = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id1 = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId>{ uniqueId1 } };
            _ids.Add("TSK5", id1);

            var uniqueId2 = new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                Source = "urn:epc:id:sgln:0000000.00000.1",
            };
            var id2 = new CompoundIdentifier(0) { UniqueIds = new List<UniqueId>{ uniqueId2 } };
            _ids.Add("TSK6", id2);

            var linkListFilename = Path.Combine(_taskdataPath, "LINKLIST.XML");

            LinkListWriter.Write(Path.GetDirectoryName(linkListFilename), _ids);

            var linkListDocument = new XmlDocument();
            linkListDocument.Load(linkListFilename);
            var lsts = linkListDocument.DocumentElement.ChildNodes[0].ChildNodes;

            Assert.AreEqual("TSK5", lsts[0].Attributes["A"].Value);
            Assert.AreEqual(uniqueId1.Id, lsts[0].Attributes["B"].Value);
            Assert.AreEqual(uniqueId1.Source, lsts[0].Attributes["C"].Value);

            Assert.AreEqual("TSK6", lsts[1].Attributes["A"].Value);
            Assert.AreEqual(uniqueId2.Id, lsts[1].Attributes["B"].Value);
            Assert.AreEqual(uniqueId2.Source, lsts[1].Attributes["C"].Value);
        }
        
        [TearDown]
        public void TearDown()
        {
            if(Directory.Exists(_exportPath))
                Directory.Delete(_exportPath, true);
        }
    }
}
