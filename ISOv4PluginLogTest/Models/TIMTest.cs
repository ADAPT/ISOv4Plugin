using System;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class TIMTest
    {
        private TIM _tim;
        private StringBuilder _output;
        private XmlWriter _xmlBuilder;

        [SetUp]
        public void Setup()
        {
            _tim = new TIM();
            _output = new StringBuilder();
            _xmlBuilder = XmlWriter.Create(_output);
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenStartAndEndTagsAreWritten()
        {
            _tim.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("<TIM"));
            Assert.True(_output.ToString().Contains("/"));
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenAIsWritten()
        {
            _tim.ASpecified = true;
            _tim.A = DateTime.Today;

            _tim.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("A=\"" + DateTime.Today.ToString("yyyy-MM-ddThh:mm:ss") + "\""));
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenBIsWritten()
        {
            _tim.BSpecified = true;
            _tim.B = DateTime.Today;

            _tim.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("B=\"" + DateTime.Today.ToString("yyyy-MM-ddThh:mm:ss") + "\""));
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenDIsWritten()
        {
            _tim.DSpecified = true;
            _tim.D = TIMD.Item3;

            _tim.WriteXML(_xmlBuilder);
            _xmlBuilder.Flush();
            Assert.True(_output.ToString().Contains("D=\"3\""));
        }

        // Todo:  add tests for attributes that are not specified
    }
}
