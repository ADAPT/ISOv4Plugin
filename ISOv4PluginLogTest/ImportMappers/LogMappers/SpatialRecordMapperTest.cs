using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using Moq;
using NUnit.Framework;
using NumericRepresentation = AgGateway.ADAPT.ApplicationDataModel.Representations.NumericRepresentation;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class SpatialRecordMapperTest
    {
        private ISOSpatialRow _isoSpatialRow;
        private List<ISOSpatialRow> _isoSpatialRows;
        private List<WorkingData> _meters;
        private Mock<IRepresentationValueInterpolator> _spatialValueInterpolator;
        private SpatialRecordMapper _spatialRecordMapper;

        [SetUp]
        public void Setup()
        {
            _isoSpatialRow = new ISOSpatialRow();
            _isoSpatialRows = new List<ISOSpatialRow> { _isoSpatialRow };
            _meters = new List<WorkingData>();

            _spatialValueInterpolator = new Mock<IRepresentationValueInterpolator>();
            _spatialRecordMapper = new SpatialRecordMapper(_spatialValueInterpolator.Object);
        }

        [Test]
        public void GivenIsoSpatialsRowsWhenMapThenSpatialRecordReturnedForEach()
        {
            _isoSpatialRows.Add(_isoSpatialRow);
            _isoSpatialRows.Add(_isoSpatialRow);
            _isoSpatialRows.Add(_isoSpatialRow);

            var result = Map();

            Assert.AreEqual(_isoSpatialRows.Count, result.Count());
        }

        [Test]
        public void GivenIsoSpatialRowAndMeterWhenMapThenSpatialRecordContainsMeterValue()
        {
            var spatialValue = new SpatialValue
            {
                Id = 0,
                Value = 12.3
            };
            _isoSpatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            _isoSpatialRows.Add(_isoSpatialRow);

            WorkingData meter = new NumericWorkingData
            {
                Representation = RepresentationInstanceList.vrAvgHarvestMoisture.ToModelRepresentation(),
                DeviceElementUseId = 1,
                UnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("prcnt")
            };

            var uniqueId = new UniqueId
            {
                IdType = IdTypeEnum.String,
                Id = "DLV0",
                Source = UniqueIdMapper.IsoSource
            };
            meter.Id.UniqueIds.Add(uniqueId);
            _meters.Add(meter);

            var result = Map();
            var meterValue = result.First().GetMeterValue(meter) as NumericRepresentationValue;

            Assert.AreEqual(12.3, meterValue.Value.Value);
        }

        [Test]
        public void GivenIsoSpatialRowAndEnumeratedMeterWhenMapThenSpatialRecordContainsMeterValue()
        {
            var spatialValue = new SpatialValue
            {
                Id = 0,
                Value = 1
            };
            _isoSpatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            _isoSpatialRows = new List<ISOSpatialRow> { _isoSpatialRow };

            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSectionControlMasterState.ToModelRepresentation(),
                ValueCodes = new List<int> { 1, 2, 3 },
                DeviceElementUseId = 1,
                GetEnumeratedValue = (sv, im) => new EnumeratedValue { Value = new AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember { Code = 3 } },
            };

            var uniqueId = new UniqueId
            {
                IdType = IdTypeEnum.String,
                Id = "DLV0",
                Source = UniqueIdMapper.IsoSource
            };
            meter.Id.UniqueIds.Add(uniqueId);
            _meters.Add(meter);

            var result = Map();
            var meterValue = result.First().GetMeterValue(meter) as EnumeratedValue;

            Assert.AreEqual(3, meterValue.Value.Code);
        }

        [Test]
        public void GivenIsoSpatialRowWithEastPositionWhenMapThenSpatialRecordShapeXIsSet()
        {
            _isoSpatialRow.EastPosition = 900000000;
            _isoSpatialRows.Add(_isoSpatialRow);

            var result = Map();

            var shape = result.First().Geometry as AgGateway.ADAPT.ApplicationDataModel.Shapes.Point;
            Assert.AreEqual(90, shape.X);
        }

        [Test]
        public void GivenIsoSpatialRowWithNorthPositionWhenMapThenSpatialRecordShapeYIsSet()
        {
            _isoSpatialRow.NorthPosition = 500000000;
            _isoSpatialRows.Add(_isoSpatialRow);

            var result = Map();

            var shape = result.First().Geometry as AgGateway.ADAPT.ApplicationDataModel.Shapes.Point;
            Assert.AreEqual(50, shape.Y);
        }

        [Test]
        public void GivenIsoSpatialRowWithElevationWhenMapThenSpatialRecordShapeZIsSet()
        {
            _isoSpatialRow.Elevation = 68512001;
            _isoSpatialRows.Add(_isoSpatialRow);

            var result = Map();

            var shape = result.First().Geometry as AgGateway.ADAPT.ApplicationDataModel.Shapes.Point;
            Assert.AreEqual(68512001, shape.Z);
        }

        [Test]
        public void GivenIsoSpatialRowWithEastPositionAndNorthPositionWhenMapThenSpatialRecordShapeXAndYAreSet()
        {
            _isoSpatialRow.EastPosition = 900000000;
            _isoSpatialRow.NorthPosition = 500000000;
            _isoSpatialRows.Add(_isoSpatialRow);

            var result = Map();

            var shape = result.First().Geometry as AgGateway.ADAPT.ApplicationDataModel.Shapes.Point;
            Assert.AreEqual(90, shape.X);
            Assert.AreEqual(50, shape.Y);

        }

        [Test]
        public void GivenIsoSpatialRowWithTimeStartWhenMapTHenSpatialRecordTimestampIsSet()
        {
            var dateTime = DateTime.Now;
            _isoSpatialRow.TimeStart = dateTime;

            var result = Map();

            Assert.AreEqual(dateTime, result.First().Timestamp);
        }

        [Test]
        public void GivenIsoSpatialRowWhenMapThenTimestampIsSet()
        {
            _isoSpatialRow.TimeStart = DateTime.Now;

            var result = Map().Single();

            Assert.AreEqual(_isoSpatialRow.TimeStart, result.Timestamp);
        }

        [Test]
        public void GivenIsoSpatialRowWithoutMeterWhenMapThenInterpolatorIsCalled()
        {
            var spatialValue = new SpatialValue
            {
                Id = 0,
                Value = 12.3
            };
            _isoSpatialRow.SpatialValues = new List<SpatialValue> { spatialValue };

            var isoSpatialRow2 = new ISOSpatialRow { SpatialValues = new List<SpatialValue>() };

            _isoSpatialRows = new List<ISOSpatialRow> {_isoSpatialRow, isoSpatialRow2};

            var meter = new NumericWorkingData
            {
                Representation = RepresentationInstanceList.vrAvgHarvestMoisture.ToModelRepresentation(),
                DeviceElementUseId = 1,
                UnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("prcnt")
            };

            var uniqueId = new UniqueId
            {
                IdType = IdTypeEnum.String,
                Id = "DLV0"
            };
            meter.Id.UniqueIds.Add(uniqueId);
            _meters.Add(meter);

            Map().ToList();
            _spatialValueInterpolator.Verify(s => s.Interpolate(It.IsAny<WorkingData>()));
        }

        [Test]
        public void GivenIsoSpatialRowsWithoutNumericMeterOnSecondWhenMapThenSecondValueIsInterpolator()
        {
            var spatialValue = new SpatialValue
            {
                Id = 0,
                Value = 12.3
            };
            _isoSpatialRow.SpatialValues = new List<SpatialValue> { spatialValue };

            var isoSpatialRow2 = new ISOSpatialRow { SpatialValues = new List<SpatialValue>() };

            _isoSpatialRows = new List<ISOSpatialRow> {_isoSpatialRow, isoSpatialRow2};

            var meter = new NumericWorkingData
            {
                Representation = RepresentationInstanceList.vrAvgHarvestMoisture.ToModelRepresentation(),
                DeviceElementUseId = 1,
                UnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("prcnt")
            };

            var uniqueId = new UniqueId
            {
                IdType = IdTypeEnum.String,
                Id = "DLV0"
            };
            meter.Id.UniqueIds.Add(uniqueId);
            _meters.Add(meter);

            var numericRepresentation = new NumericRepresentationValue(meter.Representation as NumericRepresentation,
                    meter.UnitOfMeasure, new NumericValue(meter.UnitOfMeasure, 2.3));

            _spatialValueInterpolator.Setup(s => s.Interpolate(meter)).Returns(numericRepresentation);

            var result = Map().ToList();

            Assert.AreEqual(numericRepresentation, result[1].GetMeterValue(meter) as NumericRepresentationValue);
        }

        [Test]
        public void GivenIsoSpatialRowsWithoutEnumeratedMeterOnSecondWhenMapThenSecondValueIsInterpolator()
        {
            var spatialValue = new SpatialValue
            {
                Id = 0,
                Value = 1
            };
            _isoSpatialRow.SpatialValues = new List<SpatialValue> { spatialValue };
            var isoSpatialRow2 = new ISOSpatialRow {SpatialValues = new List<SpatialValue>()};

            _isoSpatialRows = new List<ISOSpatialRow> { _isoSpatialRow, isoSpatialRow2 };

            var meter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSectionControlMasterState.ToModelRepresentation(),
                ValueCodes = new List<int> { 1, 2, 3 },
                DeviceElementUseId = 1,
                GetEnumeratedValue = (sv, im) => new EnumeratedValue { Value = new AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember { Code = 3 } }
            };

            var uniqueId = new UniqueId
            {
                IdType = IdTypeEnum.String,
                Id = "DLV0"
            };
            meter.Id.UniqueIds.Add(uniqueId);
            _meters.Add(meter);


            var enumeratedRepresentation = new EnumeratedValue { Value = new AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember { Code = 3 } };
            _spatialValueInterpolator.Setup(s => s.Interpolate(meter)).Returns(enumeratedRepresentation);

            var result = Map().ToList();

            Assert.AreEqual(enumeratedRepresentation, result[1].GetMeterValue(meter) as EnumeratedValue);
        }

        private IEnumerable<SpatialRecord> Map()
        {
            return _spatialRecordMapper.Map(_isoSpatialRows, _meters);
        }
    }
}
