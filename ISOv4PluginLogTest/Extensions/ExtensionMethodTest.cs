using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Extensions
{
    [TestFixture]
    public class ExtensionMethodTest
    {

        [Test]
        public void GivenCompoundIdentifierWithIsoIdWhenFindIsoIdThenReturnsIsoId()
        {
            
            var compoundIdentifier = new CompoundIdentifier(1);

            const string isoId = "CTR1";
            var uid = new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = isoId,
                Source = UniqueIdMapper.IsoSource
            };

            compoundIdentifier.UniqueIds = new List<UniqueId>{uid};

            var result = compoundIdentifier.FindIsoId();

            Assert.AreEqual(isoId, result);
        }

        [Test]
        public void GivenCompoundIdentifierWithNoStringIdWhenFindIsoIdThenReturnsNull()
        {
            var compoundIdentifier = CompoundIdentifierFactory.Instance.Create();

            var result = compoundIdentifier.FindIsoId();

            Assert.IsNull(result);
        }

        [Test]
        public void GivenCompoundIdentifierWithMulipleStringIdWhenFindIsoIdThenReturnsIsoId()
        {
            var compoundIdentifier = CompoundIdentifierFactory.Instance.Create();

            const string isoId = "CTR1";
            var uid1 = new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = isoId,
                Source = UniqueIdMapper.IsoSource
            };
            
            var uid2 = new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = Guid.NewGuid().ToString(),
                Source = UniqueIdMapper.IsoSource
            };

            compoundIdentifier.UniqueIds.Add(uid2);
            compoundIdentifier.UniqueIds.Add(uid1);

            var result = compoundIdentifier.FindIsoId();

            Assert.AreEqual(isoId, result);
        }

        [Test]
        public void GivenCompoundIdWithStringIdWhenFindIsoLongIdThenPullsFromIsoId()
        {
            var compoundIdentifier = CompoundIdentifierFactory.Instance.Create();

            const string isoId = "CTR2";
            var uid1 = new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = isoId,
                Source = UniqueIdMapper.IsoSource
            };
            compoundIdentifier.UniqueIds.Add(uid1);

            var result = compoundIdentifier.FindIntIsoId();

            Assert.AreEqual(2, result);
        }

        [Test]
        public void GivenCompoundIdWithNullIdWhenFindIsoLongIdThenIsZero()
        {
            var compoundIdentifier = CompoundIdentifierFactory.Instance.Create();

            var result = compoundIdentifier.FindIntIsoId();

            Assert.AreEqual(-1, result);
        }


        [Test]
        public void GivenCompoundIdWithStringIdAndLargeNumberWhenFindIsoIntIdThenPullsFromIsoId()
        {
            var compoundIdentifier = CompoundIdentifierFactory.Instance.Create();

            const string isoId = "CTR-123";
            var uid1 = new UniqueId
            {
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                Id = isoId,
                Source = UniqueIdMapper.IsoSource
            };
            compoundIdentifier.UniqueIds.Add(uid1);

            var result = compoundIdentifier.FindIntIsoId();

            Assert.AreEqual(123, result);
        }

        [Test]
        public void GivenCompoundIdWithMultipleSourcesWhenFindIsoIdThenIsoSourceIsSelected()
        {
            var compoundIdentifier = CompoundIdentifierFactory.Instance.Create();
            var uid1 = new UniqueId
            {
                Id = "DLV1",
                Source = "otherSource",
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                SourceType = IdSourceTypeEnum.URI,
            };
            var uid2 = new UniqueId
            {
                Id = "DLV2",
                Source = UniqueIdMapper.IsoSource,
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
                SourceType = IdSourceTypeEnum.URI,
            };
            compoundIdentifier.UniqueIds.Add(uid1);
            compoundIdentifier.UniqueIds.Add(uid2);

            var result = compoundIdentifier.FindIsoId();

            Assert.AreEqual("DLV2", result);
        }

        [Test]
        public void GivenOperationDataWithMaxDepthThreeWhenGetAllSectionsThenAllSections()
        {
            var operationData = new OperationData { MaxDepth = 3 };

            var depth0Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth1Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth2Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth3Sections = new List<DeviceElementUse> { new DeviceElementUse() };

            operationData.GetDeviceElementUses = depth =>
            {
                if (depth == 0)
                    return depth0Sections;
                if (depth == 1)
                    return depth1Sections;
                if (depth == 2)
                    return depth2Sections;
                if (depth == 3)
                    return depth3Sections;

                return null;
            };

            var result = operationData.GetAllSections();

            Assert.AreEqual(4, result.Count);
            Assert.Contains(depth0Sections.First(), result);
            Assert.Contains(depth1Sections.First(), result);
            Assert.Contains(depth2Sections.First(), result);
            Assert.Contains(depth3Sections.First(), result);
        }

        [Test]
        public void GivenOperationDataWithMaxDepthTwoWhenGetAllSectionsThenAllSections()
        {
            var operationData = new OperationData { MaxDepth = 2 };

            var depth0Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth1Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth2Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth3Sections = new List<DeviceElementUse> { new DeviceElementUse() };

            operationData.GetDeviceElementUses = depth =>
            {
                if (depth == 0)
                    return depth0Sections;
                if (depth == 1)
                    return depth1Sections;
                if (depth == 2)
                    return depth2Sections;
                if (depth == 3)
                    return depth3Sections;

                return null;
            };

            var result = operationData.GetAllSections();

            Assert.AreEqual(3, result.Count);
            Assert.Contains(depth0Sections.First(), result);
            Assert.Contains(depth1Sections.First(), result);
            Assert.Contains(depth2Sections.First(), result);
        }

        [Test]
        public void GivenOperationDataWithMaxDepthOneWhenGetAllSectionsThenAllSections()
        {
            var operationData = new OperationData { MaxDepth = 1 };

            var depth0Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth1Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth2Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth3Sections = new List<DeviceElementUse> { new DeviceElementUse() };

            operationData.GetDeviceElementUses = depth =>
            {
                if (depth == 0)
                    return depth0Sections;
                if (depth == 1)
                    return depth1Sections;
                if (depth == 2)
                    return depth2Sections;
                if (depth == 3)
                    return depth3Sections;

                return null;
            };

            var result = operationData.GetAllSections();

            Assert.AreEqual(2, result.Count);
            Assert.Contains(depth0Sections.First(), result);
            Assert.Contains(depth1Sections.First(), result);
        }

        [Test]
        public void GivenOperationDataWithMaxDepthZeroWhenGetAllSectionsThenAllSections()
        {
            var operationData = new OperationData { MaxDepth = 0 };

            var depth0Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth1Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth2Sections = new List<DeviceElementUse> { new DeviceElementUse() };
            var depth3Sections = new List<DeviceElementUse> { new DeviceElementUse() };

            operationData.GetDeviceElementUses = depth =>
            {
                if (depth == 0)
                    return depth0Sections;
                if (depth == 1)
                    return depth1Sections;
                if (depth == 2)
                    return depth2Sections;
                if (depth == 3)
                    return depth3Sections;

                return null;
            };

            var result = operationData.GetAllSections();

            Assert.AreEqual(1, result.Count);
            Assert.Contains(depth0Sections.First(), result);
        }
            
        [Test]
        public void GivenNullGetSectionsWhenGetAllSectionsThenEmptyList()
        {
            var operationData = new OperationData {GetDeviceElementUses = null};
            var result = operationData.GetAllSections();

            Assert.AreEqual(0, result.Count);
        }
    }
}
