using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PDTTest
    {
        private PDT _pdt;
        private StringBuilder _output;
        private XmlWriter _xmlWriter;

        [SetUp]
        public void Setup()
        {
            _pdt = new PDT();
            _output = new StringBuilder();
            _xmlWriter = XmlWriter.Create(_output, new XmlWriterSettings{ConformanceLevel = ConformanceLevel.Fragment});
        }

        [Test]
        public void GivenPdtWhenWriteThenStartAndEndTagsAreWritten()
        {
            _pdt.WriteXML(_xmlWriter);
            _xmlWriter.Flush();
            Assert.IsTrue(_output.ToString().Contains("<PDT"));
        }

        [Test]
        public void GivenPdtWhenWriteThenIdIsWritten()
        {
            _pdt.A = "PDT1";
            _pdt.WriteXML(_xmlWriter);
            _xmlWriter.Flush();
            Assert.IsTrue(_output.ToString().Contains("A=\"PDT1\""));
        }

        [Test]
        public void GivenPdtWhenWriteThenProductNameIsWritten()
        {
            _pdt.B = "Some Product Name";
            _pdt.WriteXML(_xmlWriter);
            _xmlWriter.Flush();
            Assert.IsTrue(_output.ToString().Contains("B=\"Some Product Name\""));
        }

        [Test]
        public void GivenPdtWhenWriteThenProductTypeIsWritten()
        {
            _pdt.F = PDTF.Mixture;
            _pdt.WriteXML(_xmlWriter);
            _xmlWriter.Flush();
            Assert.IsTrue(_output.ToString().Contains("F=\"2\""));
        }
    }
}
