using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class CTRTest
    {
        private CTR _ctr;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _ctr = new CTR();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenCTRWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _ctr.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<CTR"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenCTRWhenWriteXmlThenAIsWritten()
        {
            _ctr.A = "H";
            _ctr.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"H\""));
        }

        [Test]
        public void GivenCTRWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            _ctr.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
        }

        [Test]
        public void GivenCTRWhenWriteXmlThenBIsWritten()
        {
            _ctr.B = "B";
            _ctr.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"B\""));
        }

        [Test]
        public void GivenCTRWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            _ctr.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("B="));
        }
    }
}
