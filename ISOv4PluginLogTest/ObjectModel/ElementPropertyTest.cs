using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ObjectModel
{
    //[TestFixture]
    //public class ElementPropertyTest
    //{
    
    //    private XPathNavigator CreateNavigator(string attributeName, string attributeValue)
    //    {
    //        var memStream = new MemoryStream();
    //        using (var xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
    //        {
    //            xmlWriter.WriteStartElement("TestElement");
    //            xmlWriter.WriteAttributeString(attributeName, attributeValue);
    //            xmlWriter.WriteEndElement();
    //            xmlWriter.Flush();
    //            xmlWriter.Close();
    //        }

    //        memStream.Position = 0;
    //        var xpathDoc = new XPathDocument(memStream);
    //        return xpathDoc.CreateNavigator().SelectSingleNode("TestElement");
    //    }

    //    [Test]
    //    public void GivenNullNavigatorWhenCreatedThenStateIsNull()
    //    {
    //        var result = new HeaderProperty(null, "");
    //        Assert.AreEqual(HeaderPropertyState.IsNull, result.State);
    //    }

    //    [Test]
    //    public void GivenNavigatorWithEmptyValueWhenCreatedThenStateIsEmpty()
    //    {
    //        var navigator = CreateNavigator("Test", "");
    //        var result = new HeaderProperty(navigator, "Test");

    //        Assert.AreEqual(HeaderPropertyState.IsEmpty, result.State);
    //    }

    //    [Test]
    //    public void GivenNavigatorWithEmptyValueWhenCreatedThenStateHasValue()
    //    {
    //        var navigator = CreateNavigator("Test", "what");
    //        var result = new HeaderProperty(navigator, "Test");

    //        Assert.AreEqual(HeaderPropertyState.HasValue, result.State);
    //    }

    //    [Test]
    //    public void GivenNavigatorWithValueWhenCreatedThenValueIsSet()
    //    {
    //        var navigator = CreateNavigator("Test", "Hello");
    //        var result = new HeaderProperty(navigator, "Test");

    //        Assert.AreEqual("Hello", result.Value);
    //    }

    //    [Test]
    //    public void GivenNullNavigatorWhenCreatedThenValueIsNull()
    //    {
    //        var result = new HeaderProperty(null, "");
    //        Assert.IsNull(result.Value);
    //    }

    //}
}
