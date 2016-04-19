using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class CTRTest
    {
        private CTR _ctr;

        [SetUp]
        public void Setup()
        {
            _ctr = new CTR();
        }

        [Test]
        public void GivenCTRWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _ctr.WriteXML();
            Assert.True(result.Contains("<CTR"));
            Assert.True(result.Contains("</CTR>"));
        }

        [Test]
        public void GivenCTRWhenWriteXmlThenAIsWritten()
        {
            _ctr.A = "H";
            var result = _ctr.WriteXML();
            Assert.True(result.Contains("A=\"H\""));
        }

        [Test]
        public void GivenCTRWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            var result = _ctr.WriteXML();
            Assert.False(result.Contains("A="));
        }

        [Test]
        public void GivenCTRWhenWriteXmlThenBIsWritten()
        {
            _ctr.B = "B";
            var result = _ctr.WriteXML();
            Assert.True(result.Contains("B=\"B\""));
        }

        [Test]
        public void GivenCTRWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            var result = _ctr.WriteXML();
            Assert.False(result.Contains("B="));
        }
    }
}
