using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PFDTest
    {
        private PFD _pfd;

        [SetUp]
        public void Setup()
        {
            _pfd = new PFD();
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenStartAndEndTagsWritten()
        {
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("<PFD"));
            Assert.True(result.Contains("</PFD>"));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenAIsWritten()
        {
            _pfd.A = "frank";
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("A=\"frank\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenCIsWritten()
        {
            _pfd.C = "fiona";
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("C=\"fiona\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenDIsWritten()
        {
            _pfd.D = 2222222222;
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("D=\"2222222222\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenEIsWritten()
        {
            _pfd.E = "lip";
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("E=\"lip\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenFIsWritten()
        {
            _pfd.F = "ian";
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("F=\"ian\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenGIsWritten()
        {
            _pfd.G = "v";
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("G=\"v\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenIIsWritten()
        {
            _pfd.I = "kev";
            var result = _pfd.WriteXML();
            Assert.True(result.Contains("I=\"kev\""));
        }

        [Test]
        public void GivenPfdWhenWriteXmlThenItemsAreWritten()
        {
            var mockWriter = new Mock<IWriter>();
            _pfd.Items = new object[]{mockWriter.Object, mockWriter.Object};

            _pfd.WriteXML();
            mockWriter.Verify(x => x.WriteXML(), Times.Exactly(2));
        }

        [Test]
        public void GivenPfdWithNoDefinedValuesWhenWriteXmlThenNoValuesWritten()
        {
            var result = _pfd.WriteXML();
            Assert.False(result.Contains("A="));
            Assert.False(result.Contains("C="));
            Assert.False(result.Contains("E="));
            Assert.False(result.Contains("F="));
            Assert.False(result.Contains("G="));
            Assert.False(result.Contains("H="));
            Assert.False(result.Contains("I="));
        }
    }
}
