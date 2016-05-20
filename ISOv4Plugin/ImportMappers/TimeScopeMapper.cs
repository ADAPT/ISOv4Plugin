using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers
{
    public interface ITimeScopeMapper
    {
        List<TimeScope> Map(IEnumerable<TIM> tims, Catalog catalog);
    }

    public class TimeScopeMapper : ITimeScopeMapper
    {
        public List<TimeScope> Map(IEnumerable<TIM> tims, Catalog catalog)
        {
            return tims == null ? null : tims.Select(x => Map(x, catalog)).ToList();
        }

        private TimeScope Map(TIM tim, Catalog catalog)
        {
            var stamp1 = new DateWithContext { DateContext = DateContextEnum.ActualStart};
            if (tim.A.HasValue)
                stamp1.TimeStamp = tim.A.Value;

            var stamp2 = new DateWithContext {DateContext = DateContextEnum.ActualEnd};
            if (tim.B.HasValue)
                stamp2.TimeStamp = tim.B.Value;


            var timeScope = new TimeScope {Stamp1 = stamp1, Stamp2 = stamp2};
            
            if(catalog.TimeScopes == null)
                catalog.TimeScopes = new List<TimeScope>();
            catalog.TimeScopes.Add(timeScope);
            return timeScope;
        }
    }
}
