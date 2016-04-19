using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class FRMTest
    {
        private FRM _frm;

        [SetUp]
        public void Setup()
        {
            _frm = new FRM();
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _frm.WriteXML();
            Assert.True(result.Contains("<FRM"));
            Assert.True(result.Contains("</FRM>"));
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenAIsWritten()
        {
            _frm.A = "AAAAAAAAA";
            var result = _frm.WriteXML();
            Assert.True(result.Contains("A=\"AAAAAAAAA\""));
        }

        [Test]
        public void GivenFRMWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            var result = _frm.WriteXML();
            Assert.False(result.Contains("A="));
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenBIsWritten()
        {
            _frm.B = "B";
            var result = _frm.WriteXML();
            Assert.True(result.Contains("B=\"B\""));
        }

        [Test]
        public void GivenFRMWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            var result = _frm.WriteXML();
            Assert.False(result.Contains("B="));
        }

        [Test]
        public void GivenFRMWhenWriteXmlThenIIsWritten()
        {
            _frm.I = "Q";
            var result = _frm.WriteXML();
            Assert.True(result.Contains("I=\"Q\""));
        }

        [Test]
        public void GivenFRMWithoutAWhenWriteXmlThenIIsNotWritten()
        {
            var result = _frm.WriteXML();
            Assert.False(result.Contains("I="));
        }
    }
}
