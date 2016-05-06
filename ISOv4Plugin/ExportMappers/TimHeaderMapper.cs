using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface ITimHeaderMapper
    {
        TIMHeader Map(IEnumerable<Meter> meters);
    }

    public class TimHeaderMapper : ITimHeaderMapper
    {
        private readonly IPtnHeaderMapper _ptnHeaderMapper;
        private readonly IDlvHeaderMapper _dlvHeaderMapper;

        public TimHeaderMapper() : this(new PtnHeaderMapper(), new DlvHeaderMapper())
        {
            
        }

        public TimHeaderMapper(IPtnHeaderMapper ptnHeaderMapper, IDlvHeaderMapper dlvHeaderMapper)
        {
            _ptnHeaderMapper = ptnHeaderMapper;
            _dlvHeaderMapper = dlvHeaderMapper;
        }

        public TIMHeader Map(IEnumerable<Meter> meters)
        {
            return new TIMHeader
            {
                Start = new HeaderProperty {State = HeaderPropertyState.IsEmpty},
                Stop = new HeaderProperty{ State = HeaderPropertyState.IsNull},
                Duration = new HeaderProperty{ State = HeaderPropertyState.IsNull},
                Type = new HeaderProperty{ State = HeaderPropertyState.HasValue, Value = (int)TIMD.Item4},
                PtnHeader = _ptnHeaderMapper.Map(),
                DLVs = _dlvHeaderMapper.Map(meters).ToList()
            };
        }
    }
}
