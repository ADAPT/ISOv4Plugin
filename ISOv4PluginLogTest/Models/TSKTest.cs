using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class TSKTest
    {
        private TSK _tsk;

        [SetUp]
        public void Setup()
        {
            _tsk = new TSK();
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("<TSK"));
            Assert.True(result.Contains("</TSK>"));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenAIsWritten()
        {
            _tsk.A = "H";
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("A=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            var result = _tsk.WriteXML();
            Assert.False(result.Contains("A="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenBIsWritten()
        {
            _tsk.B = "H";
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("B=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            var result = _tsk.WriteXML();
            Assert.False(result.Contains("B="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenCIsWritten()
        {
            _tsk.C = "H";
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("C=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenCIsNotWritten()
        {
            var result = _tsk.WriteXML();
            Assert.False(result.Contains("C="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenDIsWritten()
        {
            _tsk.D = "H";
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("D=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenDIsNotWritten()
        {
            var result = _tsk.WriteXML();
            Assert.False(result.Contains("D="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenEIsWritten()
        {
            _tsk.E = "H";
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("E=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenEIsNotWritten()
        {
            var result = _tsk.WriteXML();
            Assert.False(result.Contains("E="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenFIsWritten()
        {
            _tsk.F = "H";
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("F=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenFIsNotWritten()
        {
            var result = _tsk.WriteXML();
            Assert.False(result.Contains("F="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenGISWritten()
        {
            _tsk.G = TSKG.Item1;
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("G=\"1\""));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenHIsWritten()
        {
            _tsk.H = 3;
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("H=\"3\""));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenIIsWritten()
        {
            _tsk.I = 7;
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("I=\"7\""));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenJIsWritten()
        {
            _tsk.J = 13;
            var result = _tsk.WriteXML();
            Assert.True(result.Contains("J=\"13\""));
        }

        [Test]
        public void GivenTSKWhenWriteThenItemsAreWritten()
        {
            var mockWriter = new Mock<IWriter>();
            _tsk.Items = new object[] {mockWriter.Object, mockWriter.Object};
            var result = _tsk.WriteXML();
            mockWriter.Verify(x => x.WriteXML(), Times.Exactly(2));
        }
    }
}
