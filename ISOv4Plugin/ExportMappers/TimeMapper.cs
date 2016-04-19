using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface ITimeMapper
    {
        IEnumerable<TIM> Map(IEnumerable<TimeScope> timeScopes);
    }

    public class TimeMapper : ITimeMapper
    {
        public IEnumerable<TIM> Map(IEnumerable<TimeScope> timeScopes)
        {
            if (timeScopes == null)
                return null;

            return timeScopes.Select(Map);
        }

        private TIM Map(TimeScope timeScope)
        {
            var endTime = FindActualEndTime(timeScope);
            return new TIM
            {
                A = FindActualStartTime(timeScope),
                B = endTime,
                BSpecified = endTime != DateTime.MinValue,
                D = TIMD.Item4
            };
        }

        private DateTime FindActualEndTime(TimeScope timeScope)
        {
            if (timeScope.Stamp2 == null || timeScope.Stamp2.DateContext != DateContextEnum.ActualEnd)
                return DateTime.MinValue;

            return timeScope.Stamp2.TimeStamp;
        }

        private DateTime FindActualStartTime(TimeScope timeScope)
        {
            if (timeScope.Stamp1 == null || timeScope.Stamp1.DateContext != DateContextEnum.ActualStart)
                return DateTime.MinValue;

            return timeScope.Stamp1.TimeStamp;
        }
    }
}
