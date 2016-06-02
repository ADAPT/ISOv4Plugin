using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

            return MapNotNullMeters(meters);
        }

        private IEnumerable<DLV> MapNotNullMeters(IEnumerable<Meter> meters)
        {
            var dlvOrders = meters.Select(x => x.Id.FindIntIsoId()).Distinct().OrderBy(y => y);

            if (dlvOrders.Contains(-1))
            {
                var sortedMeters = meters.OrderBy(x => x.Id.FindIntIsoId());

                foreach (var meter in sortedMeters)
                {
                    yield return Map(meter);
                }
            }
            else
            {
                foreach (var order in dlvOrders)
                {
                    var dlvMeter = meters.Where(x => x.Id.FindIntIsoId() == order).First();
                    yield return Map(dlvMeter);

                }
            }
        }

        public DLV Map(Meter meter)
        {
            var representation = _representationMapper.Map(meter.Representation);

            var dlv = new DLV();
            if (representation == null)
            {
                dlv.A = null;
            }
            else
            {
                if (meter.Representation != null && meter.Representation.Code == "dtRecordingStatus" && meter.SectionId != 0)
                    dlv.A = "161";
                else
                    dlv.A = representation.ToString();
            }

            return dlv;
        }
    }
}
