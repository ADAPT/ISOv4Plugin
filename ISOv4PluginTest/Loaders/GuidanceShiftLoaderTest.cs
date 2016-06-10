using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class GuidanceShiftLoaderTest
    {
        private TaskDataDocument _taskDataDocument;
        private XmlDocument _xmlDoc;
        private XmlElement _gstNode;
        private XmlAttribute _a;
        private XmlAttribute _b;

        [SetUp]
        public void Setup()
        {
            _taskDataDocument = new TaskDataDocument();
            _xmlDoc = new XmlDocument();
            _gstNode = _xmlDoc.CreateElement("GST");

            _a = _xmlDoc.CreateAttribute("A");
            _a.Value = "bob";
            _gstNode.Attributes.Append(_a);

            _b = _xmlDoc.CreateAttribute("B");
            _b.Value = "rob";
            _gstNode.Attributes.Append(_b);

            _xmlDoc.AppendChild(_gstNode);
        }

        [Test]
        public void GivenInputNodeWithAttributeAWhenLoadThenGuidanceGroupIdIsMapped()
        {
            _a.Value = "GGP1";

            var guidanceGroup = new GuidanceGroup();
            var guidanceGroupDescriptor = new GuidanceGroupDescriptor(guidanceGroup, null);

            _taskDataDocument.GuidanceGroups.Add(_a.Value, guidanceGroupDescriptor);

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);
            
            Assert.AreEqual(guidanceGroupDescriptor.Group.Id.ReferenceId, result.GuidanceGroupId);
        }

        [Test]
        public void GivenInputNodeWithAttributeBWhenLoadThenGuidancePatternIdIsMapped()
        {
            _b.Value = "GPN2";

            var guidancePatterns = new Dictionary<string, GuidancePattern>
            {
                {"GPN2", new AbLine()}
            };

            var guidanceGroup = new GuidanceGroup();
            var guidanceGroupDescriptor = new GuidanceGroupDescriptor(guidanceGroup, guidancePatterns);
            _taskDataDocument.GuidanceGroups.Add(_a.Value, guidanceGroupDescriptor);

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);
            
            Assert.AreEqual(guidancePatterns.First().Value.Id.ReferenceId, result.GuidancePatterId);
        }

        [Test]
        public void GivenInputNodeWithAttributeCWhenLoadThenEastShiftIsMapped()
        {
            var c = _xmlDoc.CreateAttribute("C");
            c.Value = "100";
            _gstNode.Attributes.Append(c);

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);
            
            Assert.AreEqual(c.Value, result.EastShift.Value.Value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void GivenInputNodeWithAttributeDWhenLoadThenNorthShiftIsMapped()
        {
            var d = _xmlDoc.CreateAttribute("D");
            d.Value = "145";
            _gstNode.Attributes.Append(d);

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);
            
            Assert.AreEqual(d.Value, result.NorthShift.Value.Value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void GivenInputNodeWithAttributeEWhenLoadThenPropagationOffsetIsMapped()
        {
            var e = _xmlDoc.CreateAttribute("E");
            e.Value = "65";
            _gstNode.Attributes.Append(e);

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);
            
            Assert.AreEqual(e.Value, result.PropagationOffset.Value.Value.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void GivenInputNodeWithEmptyAttributeAWhenLoadThenNull()
        {
            _a.Value = "";

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);

            Assert.IsNull(result);
        }

        [Test]
        public void GivenInputNodeWithEmptyAttributeBWhenLoadThenNull()
        {
            _b.Value = "";

            var result = GuidanceShiftLoader.Load(_xmlDoc.GetElementsByTagName("GST"), _taskDataDocument);

            Assert.IsNull(result);
        }
    }
}
