using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class FarmAssert
    {
        public static void AreEqual(XmlNodeList farmNodes, List<Farm> farms, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            for (int i = 0; i < farmNodes.Count; i++)
            {
                AreEqual(farmNodes[i], farms[i], catalog, linkList);
            }
        }

        private static void AreEqual(XmlNode farmNode, Farm farm, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            UniqueIdAssert.AreEqual(linkList, farmNode.GetXmlAttribute("A"), farm.Id.UniqueIds);
            Assert.AreEqual(farmNode.GetXmlAttribute("A"), farm.Id.FindIsoId());
            Assert.AreEqual(farmNode.GetXmlAttribute("B"), farm.Description);

            Assert.AreEqual(farmNode.GetXmlAttribute("C"), farm.ContactInfo.AddressLine1);
            Assert.AreEqual(farmNode.GetXmlAttribute("D"), farm.ContactInfo.PoBoxNumber);
            Assert.AreEqual(farmNode.GetXmlAttribute("E"), farm.ContactInfo.PostalCode);
            Assert.AreEqual(farmNode.GetXmlAttribute("F"), farm.ContactInfo.City);
            Assert.AreEqual(farmNode.GetXmlAttribute("G"), farm.ContactInfo.StateOrProvince);
            Assert.AreEqual(farmNode.GetXmlAttribute("H"), farm.ContactInfo.Country);

            var customerId = farmNode.GetXmlAttribute("I");
            if(!String.IsNullOrEmpty(customerId))
            {
                var grower = catalog.Growers.Single(x => x.Id.ReferenceId == farm.GrowerId);
                Assert.AreEqual(customerId, grower.Id.FindIsoId());
            }
        }
    }
}