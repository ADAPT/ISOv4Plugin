using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class TLGTest
    {
        private TLG _tlg;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _tlg = new TLG();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenTLGWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _tlg.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<TLG"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenTLGWhenWriteXmlThenAIsWritten()
        {
            _tlg.A = "AAAAAAAAA";
            _tlg.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"AAAAAAAAA\""));
        }

        [Test]
        public void GivenTLGWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            _tlg.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
        }
    }
}
