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
            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.GpsUtcDate.State);
        }

        [Test]
        public void WhenMapThenGpsUtcTimeIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.GpsUtcTime.State);
        }

        [Test]
        public void WhenMapThenHDOPIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsNull, result.HDOP.State);
        }

        [Test]
        public void WhenMapThenNumberOfSatellitesIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsNull, result.NumberOfSatellites.State);
        }

        [Test]
        public void WhenMapThenPDOPIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsNull, result.PDOP.State);
        }

        [Test]
        public void WhenMapThenPositionStatusIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsNull, result.PositionStatus.State);
        }

        [Test]
        public void WhenMapThenPositionEastIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.PositionEast.State);
        }

        [Test]
        public void WhenMapThenPositionNorthIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.PositionNorth.State);
        }

        [Test]
        public void WhenMapThenPositionUpIsMapped()
        {
            var result = _ptnHeaderMapper.Map();
            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.PositionUp.State);
        }
    }
}
