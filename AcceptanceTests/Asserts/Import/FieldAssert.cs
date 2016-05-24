using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class FieldAssert
    {
        public static void AreEqual(XmlNodeList fieldNodes, List<Field> fields, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            for (int i = 0; i < fieldNodes.Count; i++)
            {
                AreEqual(fieldNodes[i], fields[i], catalog, linkList);
            }
        }

        private static void AreEqual(XmlNode fieldNode, Field field, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            if (fieldNode.GetXmlAttribute("A") == null || fieldNode.GetXmlAttribute("C") == null || fieldNode.GetXmlAttribute("D") == null)
                return;

            UniqueIdAssert.AreEqual(linkList, fieldNode.GetXmlAttribute("A"), field.Id.UniqueIds);
            Assert.AreEqual(fieldNode.GetXmlAttribute("A"), field.Id.FindIsoId());
            Assert.AreEqual(fieldNode.GetXmlAttribute("C"), field.Description);

            CheckArea(fieldNode, field);
            CheckFarm(fieldNode, field, catalog);
            CheckBoundary(fieldNode, field, catalog);
            CheckGuidanceGroups(fieldNode, field, catalog, linkList);
            CheckCropZone(fieldNode, field, catalog);
        }

        private static void CheckCropZone(XmlNode fieldNode, Field field, Catalog catalog)
        {
            var cropId = fieldNode.GetXmlAttribute("G");
            if(String.IsNullOrEmpty(cropId))
               return;

            var cropZone = catalog.CropZones.Single(cz => cz.FieldId == field.Id.ReferenceId);
            var crop = catalog.Crops.Single(c => c.Id.ReferenceId == cropZone.CropId);

            Assert.AreEqual(cropId, crop.Id.FindIsoId());
            Assert.AreEqual(field.Id.ReferenceId, cropZone.FieldId);
            Assert.AreEqual(field.Description, cropZone.Description);
            Assert.AreEqual(field.Area.Value.Value, cropZone.Area.Value.Value);
            
            if(field.GuidanceGroupIds != null)
                Assert.AreEqual(field.GuidanceGroupIds, cropZone.GuidanceGroupIds);
        }

        private static void CheckArea(XmlNode fieldNode, Field field)
        {
            long areaValue;
            if (fieldNode.GetXmlAttribute("D").ParseValue(out areaValue) == false || areaValue < 0)
                return;

            Assert.AreEqual(areaValue, field.Area.Value.Value);
            Assert.AreEqual(RepresentationInstanceList.vrReportedFieldArea.ToModelRepresentation().Code,
                field.Area.Representation.Code);
            Assert.AreEqual(new CompositeUnitOfMeasure("m2").ToModelUom().Code, field.Area.Value.UnitOfMeasure.Code);
            Assert.AreEqual(new CompositeUnitOfMeasure("m2").ToModelUom().Code, field.Area.UserProvidedUnitOfMeasure.Code);
        }

        private static void CheckFarm(XmlNode fieldNode, Field field, Catalog catalog)
        {
            var farmId = fieldNode.GetXmlAttribute("F");
            if (String.IsNullOrEmpty(farmId))
                return;

            var farm = catalog.Farms.Single(f => f.Id.ReferenceId == field.FarmId);
            Assert.AreEqual(farmId, farm.Id.FindIsoId());
        }

        private static void CheckBoundary(XmlNode fieldNode, Field field, Catalog catalog)
        {
            var polygons = fieldNode.SelectNodes("PLN");
            if(polygons.Count == 0)
                return;

            var boundary = catalog.FieldBoundaries.Single(b => b.Id.ReferenceId == field.ActiveBoundaryId);
            MultiPolygonAssert.AreEqual(polygons, boundary.SpatialData);
        }

        private static void CheckGuidanceGroups(XmlNode fieldNode, Field field, Catalog catalog, Dictionary<string, List<UniqueId>> linkList)
        {
            var ggps = fieldNode.SelectNodes("GGP");
            if(ggps.Count == 0)
                return;

            var guidanceGroups = catalog.GuidanceGroups.Where(gg => field.GuidanceGroupIds.Contains(gg.Id.ReferenceId));
            GuidanceGroupAssert.AreEqual(ggps, guidanceGroups, catalog, linkList);
        }
    }
}