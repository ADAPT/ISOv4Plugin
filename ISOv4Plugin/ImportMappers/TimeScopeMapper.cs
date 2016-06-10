using System;
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
            DateTime? stamp1 = null;
            if (tim.A.HasValue)
                stamp1 = tim.A.Value;

            DateTime? stamp2 = null;
            if (tim.B.HasValue)
                stamp2 = tim.B.Value;

            var timeScope = new TimeScope {TimeStamp1 = stamp1, TimeStamp2 = stamp2, DateContext = DateContextEnum.ActualStart};
            
            if(catalog.TimeScopes == null)
                catalog.TimeScopes = new List<TimeScope>();
            catalog.TimeScopes.Add(timeScope);
            return timeScope;
        }
    }
}
