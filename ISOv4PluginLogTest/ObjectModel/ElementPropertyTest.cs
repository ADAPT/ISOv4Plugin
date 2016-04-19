using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ObjectModel
{
    [TestFixture]
    public class ElementPropertyTest
    {
        [Test]
        public void GivenNullXattributeWhenCreatedThenStateIsNull()
        {
            var result = new HeaderProperty(null);
            Assert.AreEqual(HeaderPropertyState.IsNull, result.State);
        }

        [Test]
        public void GivenXattributeWithEmptyValueWhenCreatedThenStateIsEmpty()
        {
            var attribute = new XAttribute("Test", "");

            var result = new HeaderProperty(attribute);

            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.State);
        }

        [Test]
        public void GivenXattributeWithEmptyValueWhenCreatedThenStateHasValue()
        {
            var attribute = new XAttribute("Test", "what");

            var result = new HeaderProperty(attribute);

            Assert.AreEqual(HeaderPropertyState.HasValue, result.State);
        }

        [Test]
        public void GivenXattributeWithValueWhenCreatedThenValueIsSet()
        {
            var attribute = new XAttribute("Test", "Hello");

            var result = new HeaderProperty(attribute);

            Assert.AreEqual(attribute.Value, result.Value);
        }

        [Test]
        public void GivenNullXattributeWhenCreatedThenValueIsNull()
        {
            var result = new HeaderProperty(null);
            Assert.IsNull(result.Value);
        }

    }
}
