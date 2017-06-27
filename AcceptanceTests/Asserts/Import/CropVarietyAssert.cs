using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class CropVarietyAssert
    {
        public static void AreEqual(XmlNodeList cropVarietyNodes, List<CropVarietyProduct> cropVarieties, Dictionary<string, List<UniqueId>> linkList)
        {
            for (var i = 0; i < cropVarieties.Count(); ++i)
            {
                AreEqual(cropVarietyNodes[i], cropVarieties[i], linkList);
            }
        }

        private static void AreEqual(XmlNode cropVarietyNode, CropVarietyProduct cropVariety, Dictionary<string, List<UniqueId>> linkList)
        {
            UniqueIdAssert.AreEqual(linkList, cropVarietyNode.GetXmlAttribute("A"), cropVariety.Id.UniqueIds);
            Assert.AreEqual(cropVarietyNode.GetXmlAttribute("A"), cropVariety.Id.FindIsoId());
            Assert.AreEqual(cropVarietyNode.GetXmlAttribute("B"), cropVariety.Description);
        }
    }
}