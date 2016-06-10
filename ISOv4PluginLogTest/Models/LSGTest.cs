using System.Text;
using System.Xml;
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
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _mockInteriorObject = new Mock<IWriter>();
            _lsg = new LSG();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output, new XmlWriterSettings{ConformanceLevel = ConformanceLevel.Fragment});
        }

        [Test]
        public void GivenLSGWhenWriteXmlThenAttributeAIsInlcuded()
        {
            _lsg.A = LSGA.Item2;
            _lsg.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"2\""));
        }

        [Test]
        public void GivenLSGWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _lsg.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<LSG"));
            Assert.True(_output.ToString().Contains("/"));
        }
    }
}
