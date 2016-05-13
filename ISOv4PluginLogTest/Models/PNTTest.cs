using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PNTTest
    {
        private PNT _pnt;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _pnt = new PNT();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _pnt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<PNT"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenAIsWritten()
        {
            _pnt.A = PNTA.Item2;
            _pnt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"2\""));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenCIsWritten()
        {
            _pnt.C = 93.93m;
            _pnt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("C=\"93.93\""));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenDIsWritten()
        {
            _pnt.D = 2.22m;
            _pnt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("D=\"2.22\""));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenEIsWritten()
        {
            _pnt.E = 6;
            _pnt.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("E=\"6\""));
        }
    }
}
