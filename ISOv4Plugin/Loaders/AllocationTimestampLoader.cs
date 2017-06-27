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
            var timeScope = TimestampLoader.Load(timeStampNode);
            if (timeScope == null)
                return null;

            timeScope.Location1 = LoadLocation(timeStampNode.SelectSingleNode("PTN"));
            timeScope.Location2 = timeScope.Location2;

            return timeScope;
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
