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
        TIM Map(IEnumerable<Meter> meters);
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

        public TIM Map(IEnumerable<Meter> meters)
        {
            var tim =  new TIM
            {
                ASpecified = true,
                A = null,
                BSpecified = false,
                B = null,
                CSpecified = false,
                C = null,
                DSpecified = true,
                D = TIMD.Item4,
            };

            // TODO:  This is not mapping the PTN Header?
            var ptn = _ptnHeaderMapper.Map();
            var dlvs = _dlvHeaderMapper.Map(meters).ToList();
            var iWriters = new List<IWriter>();

            if(ptn != null)
                iWriters.Add(ptn);

            if(dlvs != null)
                iWriters.AddRange(dlvs);

            tim.Items = iWriters.ToArray();

            return tim;
        }
    }
}
