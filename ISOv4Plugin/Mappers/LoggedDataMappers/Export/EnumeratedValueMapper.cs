/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IEnumeratedValueMapper
    {
        UInt32? Map(EnumeratedWorkingData currentMeter, List<WorkingData> meters, SpatialRecord spatialRecord);
    }

    public class EnumeratedValueMapper : IEnumeratedValueMapper
    {
        private readonly IEnumeratedMeterFactory _enumeratedMeterFactory;
        private readonly IRepresentationMapper _representationMapper;

        public EnumeratedValueMapper() : this (new EnumeratedMeterFactory(), new RepresentationMapper())
        {
            
        }

        public EnumeratedValueMapper(IEnumeratedMeterFactory enumeratedMeterFactory, IRepresentationMapper representationMapper)
        {
            _enumeratedMeterFactory = enumeratedMeterFactory;
            _representationMapper = representationMapper;
        }

        public UInt32? Map(EnumeratedWorkingData currentMeter, List<WorkingData> meters, SpatialRecord spatialRecord)
        {
            var ddi = _representationMapper.Map(currentMeter.Representation);
            if (ddi == null)
            {
                return null;
            }

            var creator = _enumeratedMeterFactory.GetMeterCreator(ddi.GetValueOrDefault());
            //The intent of the meters parameter below is to send in those meters that reconcile to a single DLV.
            //Since we are not exporting condensed DDIS at this time, just passing in the collection.
            return creator.GetMetersValue(meters, spatialRecord); 
        }
    }
}
