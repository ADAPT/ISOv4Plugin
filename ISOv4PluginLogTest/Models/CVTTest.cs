using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class CVTTest
    {
        private CVT _cvt;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _cvt = new CVT();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenCVTWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _cvt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<CVT"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenCVTWhenWriteXmlThenAIsWritten()
        {
            _cvt.A = "H";
            _cvt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"H\""));
        }

        [Test]
        public void GivenCVTWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            _cvt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
        }

        [Test]
        public void GivenCVTWhenWriteXmlThenBIsWritten()
        {
            _cvt.B = "B";
            _cvt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"B\""));
        }

        [Test]
        public void GivenCVTWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            _cvt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("B="));
        }
    }
}
