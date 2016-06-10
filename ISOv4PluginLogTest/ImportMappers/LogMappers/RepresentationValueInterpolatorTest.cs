using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class RepresentationValueInterpolatorTest
    {
        private RepresentationValueInterpolator _interpolator;
        private NumericWorkingData _numericMeter;
        private EnumeratedWorkingData _enumeratedMeter;

        [SetUp]
        public void Setup()
        {
            _interpolator = new RepresentationValueInterpolator();

            _numericMeter = new NumericWorkingData
            {
                Representation = RepresentationInstanceList.vrAvgHarvestMoisture.ToModelRepresentation(),
                DeviceElementUseId = 1,
                UnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("prcnt")
            };

            _enumeratedMeter = new ISOEnumeratedMeter
            {
                Representation = RepresentationInstanceList.dtSectionControlMasterState.ToModelRepresentation(),
                ValueCodes = new List<int> { 1, 2, 3 },
                DeviceElementUseId = 1,
                GetEnumeratedValue = (sv, im) => new EnumeratedValue { Value = new AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember { Code = 3 } }
            };
        }

        [Test]
        public void GivenNumericRepresentationValueWhenInterpolateThenRepresentationValue()
        {
            var representationValue = new NumericRepresentationValue(RepresentationInstanceList.vrReportedFieldArea.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("m2"), 1.0));

            _interpolator.SetMostRecentMeterValue(_numericMeter, representationValue);   
            var result = _interpolator.Interpolate(_numericMeter);

            Assert.IsInstanceOf<NumericRepresentationValue>(result);
        }

        [Test]
        public void GivenEnumeratedRepresentationValueWhenInterpolateThenRepresentationValue()
        {
            var previousEnumeratedValue = new EnumeratedValue
            {
                Representation = RepresentationInstanceList.dtSkyCondition.ToModelRepresentation()
            };

            var enumMember = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember();
            previousEnumeratedValue.Value = enumMember;
            previousEnumeratedValue.Code = enumMember.Code;

            _interpolator.SetMostRecentMeterValue(_enumeratedMeter, previousEnumeratedValue);

            var result = _interpolator.Interpolate(_enumeratedMeter);

            Assert.IsInstanceOf<EnumeratedValue>(result);
        }

        [Test]
        public void GivenNumericRepresentationValueOfTotalTypeWhenInterpolateThenRepresentationValueIsZero()
        {
            var previousRepresentationValue = new NumericRepresentationValue(RepresentationInstanceList.vrYieldMass.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("m2"), 1.0));
            _interpolator.SetMostRecentMeterValue(_numericMeter, previousRepresentationValue);

            var result = _interpolator.Interpolate(_numericMeter);

            var numericRepresentationValue = result as NumericRepresentationValue;
            Assert.AreEqual(0, numericRepresentationValue.Value.Value);
        }

        [Test]
        public void GivenNumericRepresentationValueOfRateTypeWhenInterpolateThenRepresentationValueIsSameAsPrevious()
        {
            var previousRepresentationValue = new NumericRepresentationValue(RepresentationInstanceList.vrYieldMass.ToModelRepresentation(), new NumericValue(UnitSystemManager.GetUnitOfMeasure("m2"), 1.0));
            _interpolator.SetMostRecentMeterValue(_numericMeter, previousRepresentationValue);

            var result = _interpolator.Interpolate(_numericMeter);

            var numericRepresentationValue = result as NumericRepresentationValue;
            Assert.AreEqual(previousRepresentationValue.Value.Value, numericRepresentationValue.Value.Value);
        }

        [Test]
        public void GivenEnumeratedValueWhenInterpolateThenValueIsSameAsPrevious()
        {
            var previousEnumeratedValue = new EnumeratedValue
            {
                Representation = RepresentationInstanceList.dtSkyCondition.ToModelRepresentation()
            };

            var enumMember = DefinedTypeEnumerationInstanceList.dtiSunny.ToModelEnumMember();
            previousEnumeratedValue.Value = enumMember;
            previousEnumeratedValue.Code = enumMember.Code;

            _interpolator.SetMostRecentMeterValue(_enumeratedMeter, previousEnumeratedValue);
            var result = _interpolator.Interpolate(_enumeratedMeter);

            var enumeratedRepresentation = result as EnumeratedValue;
            Assert.AreEqual(previousEnumeratedValue.Representation, enumeratedRepresentation.Representation);
            Assert.AreEqual(previousEnumeratedValue.Code, enumeratedRepresentation.Code);
        }
    }
}
