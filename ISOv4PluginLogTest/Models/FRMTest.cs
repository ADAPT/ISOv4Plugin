using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class FRMTest
    {
        private FRM _frm;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _frm = new FRM();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<FRM"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenAIsWritten()
        {
            _frm.A = "AAAAAAAAA";
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"AAAAAAAAA\""));
        }

        [Test]
        public void GivenFRMWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenBIsWritten()
        {
            _frm.B = "B";
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"B\""));
        }

        [Test]
        public void GivenFRMWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("B="));
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenIIsWritten()
        {
            _frm.I = "Q";
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("I=\"Q\""));
        }

        [Test]
        public void GivenFRMWithoutAWhenWriteXmlThenIIsNotWritten()
        {
            _frm.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("I="));
        }
    }
}
