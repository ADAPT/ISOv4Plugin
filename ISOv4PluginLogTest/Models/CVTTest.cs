using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class CVTTest
    {
        private CVT _cvt;

        [SetUp]
        public void Setup()
        {
            _cvt = new CVT();
        }

        [Test]
        public void GivenCVTWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _cvt.WriteXML();
            Assert.True(result.Contains("<CVT"));
            Assert.True(result.Contains("</CVT>"));
        }

        [Test]
        public void GivenCVTWhenWriteXmlThenAIsWritten()
        {
            _cvt.A = "H";
            var result = _cvt.WriteXML();
            Assert.True(result.Contains("A=\"H\""));
        }

        [Test]
        public void GivenCVTWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            var result = _cvt.WriteXML();
            Assert.False(result.Contains("A="));
        }

        [Test]
        public void GivenCVTWhenWriteXmlThenBIsWritten()
        {
            _cvt.B = "B";
            var result = _cvt.WriteXML();
            Assert.True(result.Contains("B=\"B\""));
        }

        [Test]
        public void GivenCVTWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            var result = _cvt.WriteXML();
            Assert.False(result.Contains("B="));
        }
    }
}
