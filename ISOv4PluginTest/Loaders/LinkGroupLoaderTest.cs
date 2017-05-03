using System;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Loaders;
using NUnit.Framework;

namespace ISOv4PluginTest.Loaders
{
    [TestFixture]
    public class LinkGroupLoaderTest
    {
        private XmlDocument _doc;
        private XmlElement _lgp;

        [SetUp]
        public void Setup()
        {
            _doc = new XmlDocument();
            _lgp = _doc.CreateElement("LGP");
            _doc.AppendChild(_lgp);
        }

        [Test]
        public void GivenLinkGroupWithoutAttributeAWhenLoadThenEmpty()
        {
            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(0, linkIds.Count);
        }

        [Test]
        public void GivenLinkGroupWithoutAttributeBWhenLoadThenEmpty()
        {
            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "LGP1";
            _lgp.Attributes.Append(attributeA);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(0, linkIds.Count);
        }

        [Test]
        public void GivenLinkGroupWithoutAttributeCWithGroupType2WhenLoadThenEmpty()
        {
            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "LGP1";
            _lgp.Attributes.Append(attributeA);

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = "2";
            _lgp.Attributes.Append(attributeB);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.IsEmpty(linkIds);
        }

        [Test]
        public void GivenLinkWithoutAttributeAWhenLoadThenEmpty()
        {
            AddLgpAttributes();

            var lnk = _doc.CreateElement("LNK");
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(0, linkIds.Count);
        }

        [Test]
        public void GivenLinkWithoutAttributeBWhenLoadThenEmpty()
        {
            AddLgpAttributes();

            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "RTR9";

            var lnk = _doc.CreateElement("LNK");
            lnk.Attributes.Append(attributeA);
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(0, linkIds.Count);
        }

        [Test]
        public void GivenLinkWhenLoadThenIdIsMapped()
        {
            AddLgpAttributes();

            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "FRM1";

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = Guid.NewGuid().ToString();

            var lnk = _doc.CreateElement("LNK");
            lnk.Attributes.Append(attributeA);
            lnk.Attributes.Append(attributeB);
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(attributeB.Value, linkIds[attributeA.Value].First().Id);
        }

        [Test]
        public void GivenLinkWhenLoadThenSourceIsMapped()
        {
            AddLgpAttributes();

            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "CRT3";

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = Guid.NewGuid().ToString();

            var attributeC = _doc.CreateAttribute("C");
            attributeC.Value = "http://www.johndeere.com";

            var lnk = _doc.CreateElement("LNK");
            lnk.Attributes.Append(attributeA);
            lnk.Attributes.Append(attributeB);
            lnk.Attributes.Append(attributeC);
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(attributeC.Value, linkIds[attributeA.Value].First().Source);
        }

        [Test]
        public void GivenLinkInGroupType1WhenLoadThenCiTypeIsUuid()
        {
            AddLgpAttributes();

            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "CRT3";

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = Guid.NewGuid().ToString();

            var lnk = _doc.CreateElement("LNK");
            lnk.Attributes.Append(attributeA);
            lnk.Attributes.Append(attributeB);
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(IdTypeEnum.UUID, linkIds[attributeA.Value].First().IdType);
        }

        [Test]
        public void GivenLinkWithStringIdInGroupType2WhenLoadThenCiTypeIsString()
        {
            AddLgpAttributes("2");

            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "CRT3";

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = "CRT4";

            var attributeC = _doc.CreateAttribute("C");
            attributeC.Value = "http://www.johndeere.com";

            var lnk = _doc.CreateElement("LNK");
            lnk.Attributes.Append(attributeA);
            lnk.Attributes.Append(attributeB);
            lnk.Attributes.Append(attributeC);
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(IdTypeEnum.String, linkIds[attributeA.Value].First().IdType);
        }

        [Test]
        public void GivenLinkWithIntIdInGroupType2WhenLoadThenCiTypeIsLongInt()
        {
            AddLgpAttributes("2");

            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "CRT3";

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = "8754";

            var attributeC = _doc.CreateAttribute("C");
            attributeC.Value = "http://www.johndeere.com";

            var lnk = _doc.CreateElement("LNK");
            lnk.Attributes.Append(attributeA);
            lnk.Attributes.Append(attributeB);
            lnk.Attributes.Append(attributeC);
            _lgp.AppendChild(lnk);

            var linkIds = LinkGroupLoader.Load(_doc.ChildNodes);

            Assert.AreEqual(IdTypeEnum.LongInt, linkIds[attributeA.Value].First().IdType);
        }

        private void AddLgpAttributes(string b = "1")
        {
            var attributeA = _doc.CreateAttribute("A");
            attributeA.Value = "LGP1";
            _lgp.Attributes.Append(attributeA);

            var attributeB = _doc.CreateAttribute("B");
            attributeB.Value = b;
            _lgp.Attributes.Append(attributeB);

            var attributeC = _doc.CreateAttribute("C");
            attributeC.Value = "iso";
            _lgp.Attributes.Append(attributeC);
        }
    }
}
