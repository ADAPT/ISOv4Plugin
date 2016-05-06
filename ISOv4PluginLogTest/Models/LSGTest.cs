using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class LSGTest
    {
        private LSG _lsg;
        private Mock<IWriter> _mockInteriorObject;

        [SetUp]
        public void Setup()
        {
            _mockInteriorObject = new Mock<IWriter>();
            _lsg = new LSG();
        }

        [Test]
        public void GivenLSGWhenWriteXmlThenAttributeAIsInlcuded()
        {
            _lsg.A = LSGA.Item2;

            var result = _lsg.WriteXML();

            Assert.True(result.Contains("A=\"2\""));
        }

        [Test]
        public void GivenLSGWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _lsg.WriteXML();
            Assert.True(result.Contains("<LSG"));
            Assert.True(result.Contains("</LSG>"));
        }

        [Test]
        public void GivenLsgWithItemsWhenWriteXmlThenItemsAreIncluded()
        {
            _lsg.Items = new object[] { _mockInteriorObject.Object, _mockInteriorObject.Object };

            var result = _lsg.WriteXML();
            _mockInteriorObject.Verify(x => x.WriteXML(), Times.Exactly(2));
        }
    }
}
