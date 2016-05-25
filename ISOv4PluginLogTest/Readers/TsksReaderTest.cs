using System.IO;
using System.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Readers;
using Moq;
using NUnit.Framework;
using TestUtilities;

namespace ISOv4PluginLogTest.Readers
{
    [TestFixture]
    public class TsksReaderTest
    {
        private XPathNodeIterator _iterator;
        private TsksReader _tsksReader;
        private Mock<ITaskdataTimReader> _timReaderMock;
        private Mock<ITlgReader> _tlgReaderMock;
        private Mock<IGrdReader> _grdReaderMock;
        private XPathNodeIterator _children;

        [SetUp]
        public void Setup()
        {
            var cardPath = DataCardUtility.WriteDataCard("agco_c100_tc___jd_sprayer_900");
            var taskDataPath = Path.Combine(cardPath, "TASKDATA", "TASKDATA.XML");
            var xDocument = new XPathDocument(taskDataPath);
            var navigator = xDocument.CreateNavigator(); 
            var taskDataNode = navigator.SelectSingleNode("ISO11783_TaskData");
            _children = taskDataNode.SelectChildren(XPathNodeType.Element);
            _iterator = _children.Current.Select("./TSK");

            _timReaderMock = new Mock<ITaskdataTimReader>();
            _tlgReaderMock = new Mock<ITlgReader>();
            _grdReaderMock = new Mock<IGrdReader>();
            _tsksReader = new TsksReader(_timReaderMock.Object, _tlgReaderMock.Object, _grdReaderMock.Object);
        }

        [Test]
        public void GivenIteratorWithNoItemsWhenReadThenEmptyList()
        {
            _iterator = _children.Current.Select("./lkjzkljzl");

            var result = _tsksReader.Read(_iterator);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GivenIteratorWhenReadThenAllTasksWithTlgsAreRead()
        {
            var result = _tsksReader.Read(_iterator);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GivenIteratorWhenReadThenAIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual("TSK4", result.A);
        }

        [Test]
        public void GivenIteratorWhenReadThenBIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual("PostSprayMod", result.B);
        }

        [Test]
        public void GivenIteratorWhenReadThenCIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual("CTR2", result.C);
        }

        [Test]
        public void GivenIteratorWhenReadThenDIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual("FRM2", result.D);
        }

        [Test]
        public void GivenIteratorWhenReadThenEIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual("PFD2", result.E);
        }

        [Test]
        public void GivenIteratorWithoutRelatedValueWhenReadThenDefaultIsUsed()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual(null, result.F);
        }

        [Test]
        public void GivenIteratorWhenReadThenGIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual(TSKG.Item3, result.G);
        }

        [Test]
        public void GivenIteratorWhenReadThenHIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual(1, result.H);
        }

        [Test]
        public void GivenIteratorWhenReadThenIIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual(2, result.I);
        }

        [Test]
        public void GivenIteratorWhenReadThenJIsPopulated()
        {
            var result = _tsksReader.Read(_iterator).First();
            Assert.AreEqual(3, result.J);
        }



        [Test]
        public void GivenIteratorWhenReadThenItemsArePopulated()
        {
            var cardPath = DataCardUtility.WriteDataCard("KV");
            var taskDataPath = Path.Combine(cardPath, "TASKDATA", "TASKDATA.XML");
            var xDocument = new XPathDocument(taskDataPath);
            var navigator = xDocument.CreateNavigator();
            var taskDataNode = navigator.SelectSingleNode("ISO11783_TaskData");
            var children = taskDataNode.SelectChildren(XPathNodeType.Element);
            _iterator = children.Current.Select("./TSK");

            _tsksReader.Read(_iterator);
            _tlgReaderMock.Verify(x => x.Read(It.IsAny<XPathNodeIterator>()));
            _timReaderMock.Verify(x => x.Read(It.IsAny<XPathNodeIterator>()));
            _grdReaderMock.Verify(x => x.Read(It.IsAny<XPathNodeIterator>()));
        }
    }
}
