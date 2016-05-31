using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Representation
{
    [TestFixture]
    public class RepresentationMapperTest
    {
        private RepresentationMapper _representationMapper;

        [SetUp]
        public void Setup()
        {
            _representationMapper = new RepresentationMapper();
        }

        [Test]
        public void GivenDdiWhenMapThenNumericRepresentationIsReturned()
        {
            var result = _representationMapper.Map(183);
            Assert.AreEqual(RepresentationInstanceList.vrYieldMass.DomainId, result.Code);
        }

        [Test]
        public void GivenDdiWhenGetUnitThenPullsFromIsoUomList()
        {
            var result = _representationMapper.GetUnitForDdi(7);
            var expected = IsoUnitOfMeasureList.Mappings.Single(m => m.Unit == "mg/m²").AdaptCode;

            Assert.AreEqual(expected, result.Code);
        }

        [Test]
        public void GivenDdiWithEnumeratedRepresentationWhenGetUnitThenReturnsNull()
        {
            var result = _representationMapper.GetUnitForDdi(161); //Condensed working state

            Assert.IsNull(result);
        }

        [Test]
        public void GivenDdiWithoutARepresentationWhenMapThenIsNumericRepresentationWithDdiCode()
        {
            var result = _representationMapper.Map(67);
            Assert.AreEqual(67.ToString(), result.Code);
        }

        [Test]
        public void GivenDdiWithoutARepresentationWhenMapThenIsNumericRepresentationWithCodeSourceDdi()
        {
            var result = _representationMapper.Map(67);
            Assert.AreEqual(RepresentationCodeSourceEnum.ISO11783_DDI, result.CodeSource);
        }

        [TearDown]
        public void TearDown()
        {
            _representationMapper = null;
        }
    }
}
