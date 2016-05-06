using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.UnitSystem;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Representation
{
    [TestFixture]
    public class UomLookupTest
    {
        [Test]
        public void GivenAdaptUomWhenGetIsoMappingThenIsoUomExists()
        {
            var uom = UnitSystemManager.GetUnitOfMeasure("l1ha-1");
            var isoUom = IsoUnitOfMeasureList.Mappings.SingleOrDefault(x => x.AdaptCode == "l1ha-1");

            Assert.IsNotNull(uom);
            Assert.IsNotNull(isoUom);
            Assert.AreEqual("l/ha", isoUom.Unit.ToLower());
        }

        [Test]
        public void GivenIsoUomLookupThenAdaptUomMillimetersIsMapped()
        {
            var uom = UnitSystemManager.GetUnitOfMeasure("mm");
            var isoMapping = IsoUnitOfMeasureList.Mappings.Single(x => x.Unit == "mm");

            Assert.AreEqual(uom.Code, isoMapping.AdaptCode);
        }

        [Test]
        public void GivenIsoUomLookupThenAdaptUomKgHaIsMapped()
        {
            var uom = UnitSystemManager.GetUnitOfMeasure("kg1ha-1");
            var isoUom = IsoUnitOfMeasureList.Mappings.SingleOrDefault(x => x.AdaptCode == "kg1ha-1");

            Assert.IsNotNull(uom);
            Assert.IsNotNull(isoUom);
            Assert.AreEqual("kg/ha", isoUom.Unit.ToLower());
        }
    }
}
