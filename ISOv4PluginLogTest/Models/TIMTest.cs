using System;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class TIMTest
    {
        private TIM _tim;

        [SetUp]
        public void Setup()
        {
            _tim = new TIM();
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenStartAndEndTagsAreWritten()
        {
            var result = _tim.WriteXML();
            Assert.True(result.Contains("<TIM"));
            Assert.True(result.Contains("</TIM>"));
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenAIsWritten()
        {
            _tim.A = DateTime.Today;

            var result = _tim.WriteXML();
            Assert.True(result.Contains("A=\""+DateTime.Today+"\""));
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenBIsWritten()
        {
            _tim.B = DateTime.Today;

            var result = _tim.WriteXML();
            Assert.True(result.Contains("B=\"" + DateTime.Today + "\""));
        }

        [Test]
        public void GivenTIMWhenWriteXMLThenDIsWritten()
        {
            _tim.D = TIMD.Item3;
            var result = _tim.WriteXML();
            Assert.True(result.Contains("D=\"3\""));
        }
    }
}
