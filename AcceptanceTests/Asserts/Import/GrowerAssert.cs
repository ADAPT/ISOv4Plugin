using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class GrowerAssert
    {
        public static void AreEqual(XmlNodeList ctrNodes, List<Grower> growers, Dictionary<string, List<UniqueId>> linkList)
        {
            for (var i = 0; i < ctrNodes.Count; i++)
            {
                AreEqual(ctrNodes[i], growers[i], linkList);
            }
        }

        private static void AreEqual(XmlNode ctrNode, Grower grower, Dictionary<string, List<UniqueId>> linkList)
        {
            UniqueIdAssert.AreEqual(linkList, ctrNode.GetXmlAttribute("A"), grower.Id.UniqueIds);
            Assert.AreEqual(ctrNode.GetXmlAttribute("A"), grower.Id.FindIsoId());
            CheckName(ctrNode, grower);
            Assert.AreEqual(ctrNode.GetXmlAttribute("D"), grower.ContactInfo.AddressLine1);
            Assert.AreEqual(ctrNode.GetXmlAttribute("E"), grower.ContactInfo.PoBoxNumber);
            Assert.AreEqual(ctrNode.GetXmlAttribute("F"), grower.ContactInfo.PostalCode);
            Assert.AreEqual(ctrNode.GetXmlAttribute("G"), grower.ContactInfo.City);
            Assert.AreEqual(ctrNode.GetXmlAttribute("H"), grower.ContactInfo.StateOrProvince);
            Assert.AreEqual(ctrNode.GetXmlAttribute("I"), grower.ContactInfo.Country);
            CheckContacts(ctrNode, grower.ContactInfo.Contacts);
        }

        private static void CheckName(XmlNode ctrNode, Grower grower)
        {
            var bNode = ctrNode.GetXmlAttribute("B");
            var cNode = ctrNode.GetXmlAttribute("C");
            if (!String.IsNullOrWhiteSpace(cNode))
                Assert.AreEqual(bNode + ", " + cNode, grower.Name);
            else
                Assert.AreEqual(bNode, grower.Name);
        }

        private static void CheckContacts(XmlNode ctrNode, List<Contact> contacts)
        {
            var jnode = ctrNode.GetXmlAttribute("J");
            var knode = ctrNode.GetXmlAttribute("K");
            var lnode = ctrNode.GetXmlAttribute("L");
            var mnode = ctrNode.GetXmlAttribute("M");

            if (!string.IsNullOrEmpty(jnode))
            {
                var fixedPhoneContact = contacts.Single(x => x.Type == ContactTypeEnum.FixedPhone);
                Assert.AreEqual(jnode, fixedPhoneContact.Number);
            }
            if (!string.IsNullOrEmpty(knode))
            {
                var mobilePhoneContact = contacts.Single(x => x.Type == ContactTypeEnum.MobilePhone);
                Assert.AreEqual(knode, mobilePhoneContact.Number);
            }
            if (!string.IsNullOrEmpty(lnode))
            {
                var faxContact = contacts.Single(x => x.Type == ContactTypeEnum.Fax);
                Assert.AreEqual(lnode, faxContact.Number);
            }
            if (!string.IsNullOrEmpty(mnode))
            {
                var emailContact = contacts.Single(x => x.Type == ContactTypeEnum.Email);
                Assert.AreEqual(mnode, emailContact.Number);
            }
        }
    }
}