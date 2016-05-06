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
            var timeScope = new TimeScope
            {
                Stamp1 = new DateWithContext { TimeStamp = tim.A, DateContext = DateContextEnum.ActualStart },
                Stamp2 = new DateWithContext { TimeStamp = tim.B, DateContext = DateContextEnum.ActualEnd },
            };

            if(catalog.TimeScopes == null)
                catalog.TimeScopes = new List<TimeScope>();
            catalog.TimeScopes.Add(timeScope);
            return timeScope;
        }
    }
}
