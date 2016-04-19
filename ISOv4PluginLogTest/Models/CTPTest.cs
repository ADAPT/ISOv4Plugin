using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class CTPTest
    {
        private CTP _ctp;

        [SetUp]
        public void Setup()
        {
            _ctp = new CTP();
        }

        [Test]
        public void GivenCTPWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _ctp.WriteXML();
            Assert.True(result.Contains("<CTP"));
            Assert.True(result.Contains("</CTP>"));
        }

        [Test]
        public void GivenCTPWhenWriteXmlThenAIsWritten()
        {
            _ctp.A = "H";
            var result = _ctp.WriteXML();
            Assert.True(result.Contains("A=\"H\""));
        }

        [Test]
        public void GivenCTPWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            var result = _ctp.WriteXML();
            Assert.False(result.Contains("A="));
        }

        [Test]
        public void GivenCTPWhenWriteXmlThenBIsWritten()
        {
            _ctp.B = "B";
            var result = _ctp.WriteXML();
            Assert.True(result.Contains("B=\"B\""));
        }

        [Test]
        public void GivenCTPWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            var result = _ctp.WriteXML();
            Assert.False(result.Contains("B="));
        }
    }
}
