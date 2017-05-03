using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class DlvHeaderMapperTest
    {
        private WorkingData _meter;
        private List<WorkingData> _meters; 
        private DlvHeaderMapper _dlvHeaderMapper;

        private Mock<IRepresentationMapper> _representationMapperMock;


        [SetUp]
        public void Setup()
        {
            _meter = new NumericWorkingData();
            _meters = new List<WorkingData>();

            _representationMapperMock = new Mock<IRepresentationMapper>();
            _dlvHeaderMapper = new DlvHeaderMapper(_representationMapperMock.Object);
        }

        [Test]
        public void GivenMetersWhenMapThenDlvForEachMeter()
        {
            _meters.Add(new NumericWorkingData());
            _meters.Add(new NumericWorkingData());
            _meters.Add(new NumericWorkingData());
            _meters.Add(new NumericWorkingData());

            var result = Map();
            Assert.AreEqual(_meters.Count, result.Count());
        }

        [Test]
        public void GivenMeterWhenMapThenProcessDataDdiIsMapped()
        {
            _meters = new List<WorkingData> { _meter };

            _representationMapperMock.Setup(x => x.Map(_meter.Representation)).Returns(5);

            var result = MapSingle();

            Assert.AreEqual(5.ToString(), result.A);
        }

        [Test]
        public void GivenMetersWhenMapThenMetersAreSorted()
        {
            _meter.Id.UniqueIds.Add(new UniqueId
            {
                Id = "DLV3",
                IdType = IdTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
           });
            _meter.Representation = RepresentationInstanceList.vrYieldMass.ToModelRepresentation();
            _representationMapperMock.Setup(x => x.Map(_meter.Representation)).Returns(5);
            _meters.Add(_meter);

            var meter1 = new NumericWorkingData();
            meter1.Id.UniqueIds.Add(new UniqueId
            {
                Id = "DLV1",
                IdType = IdTypeEnum.String,
                Source = UniqueIdMapper.IsoSource
            });
            meter1.Representation = RepresentationInstanceList.vrYieldMass.ToModelRepresentation();
            _representationMapperMock.Setup(x => x.Map(meter1.Representation)).Returns(8);
            _meters.Add(meter1);

            var result = Map().ToList();
            Assert.AreEqual(8.ToString(), result[0].A);
            Assert.AreEqual(5.ToString(), result[1].A);
        }

        [Test]
        public void GivenNullWhenMapThenReturnsNull()
        {
            _meters = null;

            var result = Map();

            Assert.IsNull(result);
        }

        private DLV MapSingle()
        {
            return Map().First();
        }

        private IEnumerable<DLV> Map()
        {
            return _dlvHeaderMapper.Map(_meters);
        }
    }
}
