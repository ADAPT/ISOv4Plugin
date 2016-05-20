using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IDlvHeaderMapper
    {
        IEnumerable<DLV> Map(IEnumerable<Meter> meters);
    }

    public class DlvHeaderMapper : IDlvHeaderMapper
    {
        private readonly IRepresentationMapper _representationMapper;

        public DlvHeaderMapper() : this(new RepresentationMapper())
        {
            
        }

        public DlvHeaderMapper(IRepresentationMapper representationMapper)
        {
            _representationMapper = representationMapper;
        }

        public IEnumerable<DLV> Map(IEnumerable<Meter> meters)
        {
            if (meters == null)
                return null;

            var sortedMeters = meters.OrderBy(x => x.Id.FindIntIsoId());

            return sortedMeters.Select(Map);
        }

        public DLV Map(Meter meter)
        {
            var representation = _representationMapper.Map(meter.Representation);

            var dlv = new DLV();
            if (representation == null)
            {
                // to do:  add state 
                dlv.A = null;
            }
            else
            {
                dlv.A = representation.ToString();
            }

            return dlv;
            //return new DLV
            //{

            //    ProcessDataDDI = new HeaderProperty
            //    {
            //        State = representation == null ? HeaderPropertyState.IsNull : HeaderPropertyState.HasValue,
            //        Value = representation
            //    }
            //};
        }
    }
}
