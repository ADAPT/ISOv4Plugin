using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PLNTest
    {
        private PLN _pln;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _pln = new PLN();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenPlnWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _pln.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<PLN"));
            Assert.True(_output.ToString().Contains("/"));
        }
        
        [Test]
        public void GivenPlnWhenWriteXmlThenPlnAIsWritten()
        {
            _pln.A = PLNA.Item8;
            _pln.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"8\""));
        }
        
        [Test]
        public void GivenPlnWhenWriteXmlThenPlnBIsWritten()
        {
            _pln.B = "Wilma";
            _pln.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"Wilma\""));
        }
    }
}
