using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class PrescriptionLoaderTest
    {
        private GRD _grd;
        private string _dataPath;

        [SetUp]
        public void Setup()
        {
            _dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_dataPath);

            _grd = new GRD
            {
                A = 123,
                B = 234,
                C = 345,
                D = 456,
                E = 567,
                F = 678,
                G = "Hold The Door",
                H = 789,
                I = 8,
                J = 1,
            };
        }

        [Test]
        public void GivenTaskDocumentWithPrescriptionWhenLoadThenUniqueIdIsSet()
        {
            TaskDataDocument taskDocument = CreateTaskDocument();

            var result = PrescriptionLoader.Load(taskDocument).First();

            Assert.AreEqual(1, result.Id.UniqueIds.Count);
            Assert.AreEqual("TSK0", result.Id.UniqueIds.First().Id);
            Assert.AreEqual(IdTypeEnum.String, result.Id.UniqueIds.First().IdType);
            Assert.AreEqual("http://dictionary.isobus.net/isobus/", result.Id.UniqueIds.First().Source);

        }

        private TaskDataDocument CreateTaskDocument()
        {
            var taskDataDocument = new TaskDataDocument();

            var xmlFile = Path.Combine(_dataPath, "data.xml");
            var fileStream = new FileStream(xmlFile, FileMode.CreateNew);
            using (var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
            {
                xmlWriter.WriteStartElement("ISO11783_TaskData");
                xmlWriter.WriteStartElement("TSK");
                xmlWriter.WriteAttributeString("A", "TSK0");

                xmlWriter.WriteStartElement("GRD");

                xmlWriter.WriteAttributeString("A", _grd.A.ToString());
                xmlWriter.WriteAttributeString("B", _grd.B.ToString());
                xmlWriter.WriteAttributeString("C", _grd.C.ToString());
                xmlWriter.WriteAttributeString("D", _grd.D.ToString());
                xmlWriter.WriteAttributeString("E", _grd.E.ToString());
                xmlWriter.WriteAttributeString("F", _grd.F.ToString());
                xmlWriter.WriteAttributeString("G", _grd.G);
                xmlWriter.WriteAttributeString("H", _grd.H.ToString());
                xmlWriter.WriteAttributeString("I", _grd.I.ToString());
                xmlWriter.WriteAttributeString("J", _grd.J.ToString());

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
              
                xmlWriter.Flush();
                xmlWriter.Close();
            }

            fileStream.Position = 0;
            fileStream.Flush();
            fileStream.Close();

            taskDataDocument.LoadFromFile(xmlFile);
            return taskDataDocument;
        }

        [TearDown]
        public void Teardown()
        {
            if(Directory.Exists(_dataPath))
                Directory.Delete(_dataPath, true);
        }
    }
}
