using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Models
{
    [TestFixture]
    public class PLNTest
    {
        private PLN _pln;

        [SetUp]
        public void Setup()
        {
            _pln = new PLN();
        }

        [Test]
        public void GivenPlnWhenWriteXmlThenStartAndEndTagsAreWritten()
        {
            var result = _pln.WriteXML();
            Assert.True(result.Contains("<PLN"));
            Assert.True(result.Contains("</PLN>"));
        }
        
        [Test]
        public void GivenPlnWhenWriteXmlThenPlnAIsWritten()
        {
            _pln.A = PLNA.Item8;

            var result = _pln.WriteXML();
            Assert.True(result.Contains("A=\"8\""));
        }
        
        [Test]
        public void GivenPlnWhenWriteXmlThenPlnBIsWritten()
        {
            _pln.B = "Wilma";

            var result = _pln.WriteXML();
            Assert.True(result.Contains("B=\"Wilma\""));
        }

        [Test]
        public void GivenPlnWithItemsWhenWriteXmlThenItemsAreIncluded()
        {
            var mockWriter = new Mock<IWriter>();
            _pln.Items = new object[] { mockWriter.Object, mockWriter.Object };

            var result = _pln.WriteXML();
            mockWriter.Verify(x => x.WriteXML(), Times.Exactly(2));
        }
    }
}
