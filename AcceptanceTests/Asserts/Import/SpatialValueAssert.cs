using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class SpatialValueAssert
    {
        public static void AreEqual(ISOSpatialRow isoSpatialRow, SpatialRecord adaptSpatialRecord, IEnumerable<Meter> meters)
        {
            foreach (var meter in meters)
            {
                var isoValue = isoSpatialRow.SpatialValues.SingleOrDefault(v => v.Id == meter.Id.FindIntIsoId());

                if(isoValue == null)
                    continue;
                
                if (meter is NumericMeter)
                {
                    var numericMeter = meter as NumericMeter;

                    var numericRepresentationValue = (NumericRepresentationValue)adaptSpatialRecord.GetMeterValue(numericMeter);
                    if (isoValue != null)
                        Assert.AreEqual(isoValue.Value, numericRepresentationValue.Value.Value);
                }
                if (meter is EnumeratedMeter)
                {
                    var isoEnumeratedMeter = meter as ISOEnumeratedMeter;
                    var enumeratedValue = isoEnumeratedMeter.GetEnumeratedValue(isoValue, isoEnumeratedMeter);
                    Assert.AreEqual(enumeratedValue.Representation.Description, meter.Representation.Description);
                }
            }
        }
    }
}