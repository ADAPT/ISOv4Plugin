using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;
using EnumerationMember = AgGateway.ADAPT.ApplicationDataModel.Representations.EnumerationMember;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IRepresentationValueInterpolator
    {
        RepresentationValue Interpolate(Meter meter);
        void SetMostRecentMeterValue(Meter meter, RepresentationValue value);
        void Clear();
    }

    public class RepresentationValueInterpolator : IRepresentationValueInterpolator
    {
        private readonly List<string> _numericRepresentationTotals;
        private Dictionary<Meter, RepresentationValue> _meterToPreviousValue; 

        public RepresentationValueInterpolator()
        {
            _meterToPreviousValue = new Dictionary<Meter, RepresentationValue>();

            _numericRepresentationTotals = new List<string>
            {
                RepresentationInstanceList.vrYieldMass.DomainId,
                RepresentationInstanceList.vrYieldWetMassForage.DomainId,
                RepresentationInstanceList.vrYieldWetMassFrgPerArea.DomainId,
                RepresentationInstanceList.vrTotalQuantityAppliedVolume.DomainId,
            };
        }
        
        public void SetMostRecentMeterValue(Meter meter, RepresentationValue value)
        {
            if (_meterToPreviousValue.ContainsKey(meter))
            {
                _meterToPreviousValue[meter] = value;
            }
            else
            {
                _meterToPreviousValue.Add(meter, value);
            }
        }

        public void Clear()
        {
            _meterToPreviousValue.Clear();
        }

        public RepresentationValue Interpolate(Meter meter)
        {
            if (!_meterToPreviousValue.ContainsKey(meter))
                return null;

            var previousValue = _meterToPreviousValue[meter];

            var numericRepresentationValue = previousValue as NumericRepresentationValue;
            if (numericRepresentationValue != null)
            {
                var newNumericRepresentationValue = numericRepresentationValue.Copy();

                if (numericRepresentationValue.Representation != null && _numericRepresentationTotals.Contains(numericRepresentationValue.Representation.Code))
                {
                    newNumericRepresentationValue.Value.Value = 0;
                }
                
                return newNumericRepresentationValue;
            }

            var enumeratedValue = previousValue as EnumeratedValue;
            if (enumeratedValue != null)
            {
                var newEnumeratedValue = new EnumeratedValue
                {
                    Code = enumeratedValue.Code,
                    Value = new EnumerationMember
                    {
                        Code = enumeratedValue.Value.Code,
                        Value = enumeratedValue.Value.Value
                    },
                    Representation = enumeratedValue.Representation

                };

                return newEnumeratedValue;
            }

            return null;
        }
    }
}
