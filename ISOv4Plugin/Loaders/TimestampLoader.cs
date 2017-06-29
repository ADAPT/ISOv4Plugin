using System;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public static class TimestampLoader
    {
        public static TimeScope Load(XmlNode inputNode)
        {
            // Required attributes
            var startTimeValue = inputNode.GetXmlNodeValue("@A");
            var typeValue = inputNode.GetXmlNodeValue("@D");
            if (string.IsNullOrEmpty(startTimeValue) ||
                string.IsNullOrEmpty(typeValue))
                return null;

            var timeStamp1 = ParseDateTime(startTimeValue);
            if (timeStamp1 == null)
                return null;

            var timeStamp2 = ParseDateTime(inputNode.GetXmlNodeValue("@B"));
            if (timeStamp2 == null)
            {
                var duration = ParseDuration(inputNode.GetXmlNodeValue("@C"));
                if (duration.HasValue)
                    timeStamp2 = timeStamp1.Value.Add(duration.Value);
            }

            return new TimeScope
            {
                TimeStamp1 = timeStamp1,
                TimeStamp2 = timeStamp2,
                DateContext = typeValue == "1" ? DateContextEnum.ProposedStart : DateContextEnum.ActualStart,
                Duration = timeStamp2.GetValueOrDefault() - timeStamp2.GetValueOrDefault()
            };
        }

        private static DateTime? ParseDateTime(string timeValue)
        {
            if (string.IsNullOrEmpty(timeValue))
                return null;
            try
            {
                return XmlConvert.ToDateTime(timeValue, XmlDateTimeSerializationMode.RoundtripKind);
            }
            catch (FormatException)
            {
            }
            return null;
        }

        private static TimeSpan? ParseDuration(string durationValue)
        {
            long duration;
            if (!durationValue.ParseValue(out duration) || duration < 0)
                return null;

            return TimeSpan.FromSeconds(duration);
        }
    }
}
