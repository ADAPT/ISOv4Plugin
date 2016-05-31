using System;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public static class AllocationTimestampLoader
    {
        public static TimeScope Load(XmlNode inputNode)
        {
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

            var location = LoadLocation(timeStampNode.SelectSingleNode("PTN"));

            return new TimeScope
            {
                TimeStamp1 = timeStamp1,
                TimeStamp2 = timeStamp2,
                Location1 = location,
                Location2 = location,
                DateContext = typeValue == "1" ? DateContextEnum.ProposedStart : DateContextEnum.ActualStart,
                Duration = timeStamp1.GetValueOrDefault() - timeStamp2.GetValueOrDefault(),
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

        private static Location LoadLocation(XmlNode inputNode)
        {
            if (inputNode == null)
                return null;

            var location = new Location
            {
                Position = GetPosition(inputNode),
                GpsSource = GetGpsSource(inputNode)
            };

            if (location.Position == null)
                return null;

            return location;
        }

        private static Point GetPosition(XmlNode inputNode)
        {
            double latitude, longitude;
            if (inputNode.GetXmlNodeValue("@A").ParseValue(out latitude) == false ||
                inputNode.GetXmlNodeValue("@B").ParseValue(out longitude) == false)
                return null;

            return new Point
            {
                X = longitude,
                Y = latitude
            };
        }

        private static GpsSource GetGpsSource(XmlNode inputNode)
        {
            var gpsSource = new GpsSource
            {
                SourceType = GetSourceType(inputNode.GetXmlNodeValue("@D"))
            };

            int satelliteCount;
            if (inputNode.GetXmlNodeValue("@G").ParseValue(out satelliteCount))
                gpsSource.NumberOfSatellites = satelliteCount;

            gpsSource.GpsUtcTime = GetGpsTime(inputNode);

            return gpsSource;
        }

        private static readonly DateTime GpsBaseDate = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime GetGpsTime(XmlNode inputNode)
        {
            var gpsTime = inputNode.GetXmlNodeValue("@H");
            var gpsDate = inputNode.GetXmlNodeValue("@I");
            if (string.IsNullOrEmpty(gpsDate))
                return DateTime.MinValue;

            int utcDays;
            if (!gpsDate.ParseValue(out utcDays))
                return DateTime.MinValue;

            long utcMilliseconds = 0;
            if (!string.IsNullOrEmpty(gpsTime))
            {
                if (!gpsTime.ParseValue(out utcMilliseconds))
                    utcMilliseconds = 0;
            }

            return GpsBaseDate.Add(TimeSpan.FromDays(utcDays).Add(TimeSpan.FromMilliseconds(utcMilliseconds)));
        }

        private static GpsSourceEnum GetSourceType(string gpsType)
        {
            int sourceType;
            if (gpsType.ParseValue(out sourceType))
            {
                switch (sourceType)
                {
                    case 1:
                        return GpsSourceEnum.GNSSfix;

                    case 2:
                        return GpsSourceEnum.DGNSSfix;

                    case 3:
                        return GpsSourceEnum.PreciseGNSS;

                    case 4:
                        return GpsSourceEnum.RTKFixedInteger;

                    case 5:
                        return GpsSourceEnum.RTKFloat;

                    case 6:
                        return GpsSourceEnum.EstDRmode;

                    case 7:
                        return GpsSourceEnum.ManualInput;

                    case 8:
                        return GpsSourceEnum.SimulateMode;

                    case 16:
                        return GpsSourceEnum.DesktopGeneratedData;

                    case 17:
                        return GpsSourceEnum.Other;
                }
            }

            return GpsSourceEnum.Unknown;
        }
    }
}
