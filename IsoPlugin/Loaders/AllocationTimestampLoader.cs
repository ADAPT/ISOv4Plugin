using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using System;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal static class AllocationTimestampLoader
    {
        internal static TimeScope Load(XmlNode inputNode)
        {
            Point location;
            return Load(inputNode, out location);
        }

        internal static TimeScope Load(XmlNode inputNode, out Point location)
        {
            location = null;

            var timeStampNode = inputNode.SelectSingleNode("ASP");
            if (timeStampNode == null)
                return null;

            // Required attributes
            var startTimeValue = timeStampNode.GetXmlNodeValue("@A");
            var typeValue = timeStampNode.GetXmlNodeValue("@D");
            if (string.IsNullOrEmpty(startTimeValue) ||
                string.IsNullOrEmpty(typeValue))
                return null;

            var timeStamp1 = ParseDateTime(startTimeValue);
            if (timeStamp1 == null)
                return null;

            var timeStamp2 = ParseDateTime(timeStampNode.GetXmlNodeValue("@B"));
            if (timeStamp2 == null)
            {
                var duration = ParseDuration(timeStampNode.GetXmlNodeValue("@C"));
                if (duration.HasValue)
                    timeStamp2 = timeStamp1.Value.Add(duration.Value);
            }

            location = ShapeLoader.LoadPoint(timeStampNode.SelectSingleNode("PTN"));

            return new TimeScope
            {
                Stamp1 = GetDateWithContext(timeStamp1, typeValue, true),
                Stamp2 = GetDateWithContext(timeStamp2, typeValue, false)
            };
        }

        private static readonly string[] DATE_TIME_PARSE_FORMATS = { "o", "s" };
        private static DateTime? ParseDateTime(string timeValue)
        {
            DateTime timeStamp;
            if (!DateTime.TryParseExact(timeValue, DATE_TIME_PARSE_FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out timeStamp))
                return null;

            return timeStamp;
        }

        private static TimeSpan? ParseDuration(string durationValue)
        {
            long duration;
            if (!durationValue.ParseValue(out duration) || duration < 0)
                return null;

            return TimeSpan.FromSeconds(duration);
        }

        private static DateWithContext GetDateWithContext(DateTime? timeStamp, string typeValue, bool isStart)
        {
            if (!timeStamp.HasValue)
                return null;

            bool isPlanned = string.Equals(typeValue, "1", StringComparison.OrdinalIgnoreCase);

            return new DateWithContext
            {
                TimeStamp = timeStamp.Value,
                DateContext = isPlanned ?
                    (isStart ? DateContextEnum.ProposedStart : DateContextEnum.ProposedEnd) :
                    (isStart ? DateContextEnum.ActualStart : DateContextEnum.ActualEnd)
            };
        }
    }
}
