using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class TLGTest
    {
        private TLG _tlg;

        [SetUp]
        public void Setup()
        {
            _tlg = new TLG();
        }

        [Test]
        public void GivenTLGWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _tlg.WriteXML();
            Assert.True(result.Contains("<TLG"));
            Assert.True(result.Contains("</TLG>"));
        }

        [Test]
        public void GivenTLGWhenWriteXmlThenAIsWritten()
        {
            _tlg.A = "AAAAAAAAA";
            var result = _tlg.WriteXML();
            Assert.True(result.Contains("A=\"AAAAAAAAA\""));
        }

        [Test]
        public void GivenTLGWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            var result = _tlg.WriteXML();
            Assert.False(result.Contains("A="));
        }
    }
}
