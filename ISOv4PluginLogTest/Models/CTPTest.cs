using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class CTPTest
    {
        private CTP _ctp;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _ctp = new CTP();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenCTPWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _ctp.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<CTP"));
        }

        [Test]
        public void GivenCTPWhenWriteXmlThenAIsWritten()
        {
            _ctp.A = "H";
            _ctp.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"H\""));
        }

        [Test]
        public void GivenCTPWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            _ctp.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
        }

        [Test]
        public void GivenCTPWhenWriteXmlThenBIsWritten()
        {
            _ctp.B = "B";
            _ctp.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"B\""));
        }

        [Test]
        public void GivenCTPWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            _ctp.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("B="));
        }
    }
}
