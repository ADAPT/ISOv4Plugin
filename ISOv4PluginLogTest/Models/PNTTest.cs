using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PNTTest
    {
        private PNT _pnt;

        [SetUp]
        public void Setup()
        {
            _pnt = new PNT();
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _pnt.WriteXML();
            Assert.True(result.Contains("<PNT"));
            Assert.True(result.Contains("</PNT>"));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenAIsWritten()
        {
            _pnt.A = PNTA.Item2;
            var result = _pnt.WriteXML();
            Assert.True(result.Contains("A=\"2\""));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenCIsWritten()
        {
            _pnt.C = 93.93m;
            var result = _pnt.WriteXML();
            Assert.True(result.Contains("C=\"93.93\""));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenDIsWritten()
        {
            _pnt.D = 2.22m;
            var result = _pnt.WriteXML();
            Assert.True(result.Contains("D=\"2.22\""));
        }

        [Test]
        public void GivenPNTWhenWriteXmlThenEIsWritten()
        {
            _pnt.E = 6;
            var result = _pnt.WriteXML();
            Assert.True(result.Contains("E=\"6\""));
        }
    }
}
