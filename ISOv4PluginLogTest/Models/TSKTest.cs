using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class TSKTest
    {
        private TSK _tsk;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _tsk = new TSK();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<TSK"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenAIsWritten()
        {
            _tsk.A = "H";
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenAIsNotWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("A="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenBIsWritten()
        {
            _tsk.B = "H";
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenBIsNotWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("B="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenCIsWritten()
        {
            _tsk.C = "H";
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("C=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenCIsNotWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("C="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenDIsWritten()
        {
            _tsk.D = "H";
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("D=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenDIsNotWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("D="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenEIsWritten()
        {
            _tsk.E = "H";
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("E=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenEIsNotWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("E="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenFIsWritten()
        {
            _tsk.F = "H";
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("F=\"H\""));
        }

        [Test]
        public void GivenTSKWithoutAWhenWriteXmlThenFIsNotWritten()
        {
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.False(_output.ToString().Contains("F="));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenGISWritten()
        {
            _tsk.G = TSKG.Item1;
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("G=\"1\""));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenHIsWritten()
        {
            _tsk.H = 3;
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("H=\"3\""));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenIIsWritten()
        {
            _tsk.I = 7;
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("I=\"7\""));
        }

        [Test]
        public void GivenTSKWhenWriteXmlThenJIsWritten()
        {
            _tsk.J = 13;
            _tsk.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("J=\"13\""));
        }
    }
}
