using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PFDTest
    {
        private PFD _pfd;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _pfd = new PFD();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenStartAndEndTagsWritten()
        {
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<PFD"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenAIsWritten()
        {
            _pfd.A = "frank";
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"frank\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenCIsWritten()
        {
            _pfd.C = "fiona";
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("C=\"fiona\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenDIsWritten()
        {
            _pfd.D = 2222222222;
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("D=\"2222222222\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenEIsWritten()
        {
            _pfd.E = "lip";
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("E=\"lip\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenFIsWritten()
        {
            _pfd.F = "ian";
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("F=\"ian\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenGIsWritten()
        {
            _pfd.G = "v";
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("G=\"v\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenIIsWritten()
        {
            _pfd.I = "kev";
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("I=\"kev\""));
        }

        [Test]
        public void GivenPfdWithNoDefinedValuesWhenWriteXmlThenNoValuesWritten()
        {
            _pfd.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
            Assert.False(_output.ToString().Contains("C="));
            Assert.False(_output.ToString().Contains("E="));
            Assert.False(_output.ToString().Contains("F="));
            Assert.False(_output.ToString().Contains("G="));
            Assert.False(_output.ToString().Contains("H="));
            Assert.False(_output.ToString().Contains("I="));
        }
    }
}
