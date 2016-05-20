using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class PtnHeaderMapperTest
    {
        private PtnHeaderMapper _ptnHeaderMapper;

        [SetUp]
        public void Setup()
        {
            _ptnHeaderMapper = new PtnHeaderMapper();
        }

        [Test]
        public void WhenMapThenGpsUtcDateIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(false, result.ISpecified);
        }

        [Test]
        public void WhenMapThenGpsUtcTimeIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(false, result.HSpecified);
        }

        [Test]
        public void WhenMapThenHDOPIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(false, result.FSpecified);
        }

        [Test]
        public void WhenMapThenNumberOfSatellitesIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(false, result.GSpecified);
        }

        [Test]
        public void WhenMapThenPDOPIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(false, result.ESpecified);
        }

        [Test]
        public void WhenMapThenPositionStatusIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(false, result.DSpecified);
        }

        [Test]
        public void WhenMapThenPositionEastIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(true, result.BSpecified);
            Assert.IsNull(result.B);
        }

        [Test]
        public void WhenMapThenPositionNorthIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(true, result.ASpecified);
            Assert.IsNull(result.A);
        }

        [Test]
        public void WhenMapThenPositionUpIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(true, result.CSpecified);
            Assert.IsNull(result.C);
        }
    }
}
