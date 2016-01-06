using AgGateway.ADAPT.ApplicationDataModel;
using AgGateway.ADAPT.Representation.UnitSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal static class GuidancePatternLoader
    {
        internal static Dictionary<string, GuidancePattern> LoadGuidancePatterns(XmlNodeList inputNodes)
        {
            var patterns = new Dictionary<string, GuidancePattern>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlNode inputNode in inputNodes)
            {
                string patternId;
                var pattern = LoadGuidancePattern(inputNode, out patternId);
                if (pattern == null)
                    return null;

                patterns.Add(patternId, pattern);
            }

            return patterns;
        }

        private static GuidancePattern LoadGuidancePattern(XmlNode inputNode, out string patternId)
        {
            patternId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(patternId))
                return null;

            GuidancePattern pattern = GetPattern(inputNode);
            if (pattern == null)
                return null;

            pattern.Description = inputNode.GetXmlNodeValue(@"B");

            pattern.PropagationDirection = GetPropagationDirection(inputNode.GetXmlNodeValue("@E"));
            pattern.BoundingPolygon = ShapeLoader.LoadPolygon(inputNode.SelectNodes("PLN[0]"));
            pattern.Extension = GetExtension(inputNode.GetXmlNodeValue("@F"));
            pattern.GpsSource = GetGpsSource(inputNode);
            pattern.OriginalEpsgCode = inputNode.GetXmlNodeValue("@M");

            return pattern;
        }

        private static GuidancePattern GetPattern(XmlNode inputNode)
        {
            int patternType;
            if (inputNode.GetXmlNodeValue("@C").ParseValue(out patternType) == false)
                return null;

            var lineNode = inputNode.SelectSingleNode("LSG");
            if (lineNode == null)
                return null;

            switch (patternType)
            {
                case 1:
                    return LoadAbLinePattern(inputNode, lineNode);

                case 2:
                    return LoadAPlusPattern(inputNode, lineNode);

                case 3:
                    return LoadAbCurvePattern(inputNode, lineNode);

                case 4:
                    return LoadCenterPivotPattern(lineNode);

                case 5:
                    return LoadSpiralPattern(lineNode);

                default:
                    return null;
            }
        }

        private static AbLine LoadAbLinePattern(XmlNode inputNode, XmlNode lineNode)
        {
            var pattern = new AbLine { GuidancePatternType = GuidancePatternTypeEnum.AbLine };

            var pointNodes = lineNode.SelectNodes("PNT");
            if (pointNodes.Count != 2)
                return null;

            var aPointNode = pointNodes[0];
            var bPointNode = pointNodes[1];

            pattern.A = ShapeLoader.LoadPoint(aPointNode);
            pattern.B = ShapeLoader.LoadPoint(bPointNode);
            if (pattern.A == null || pattern.B == null)
                return null;

            pattern.Heading = GetHeading(inputNode.GetXmlNodeValue("@G"));
            return pattern;
        }

        private static APlus LoadAPlusPattern(XmlNode inputNode, XmlNode lineNode)
        {
            var pattern = new APlus { GuidancePatternType = GuidancePatternTypeEnum.APlus };

            var pointNodes = lineNode.SelectNodes("PNT");
            if (pointNodes.Count != 1)
                return null;

            var aPointNode = pointNodes[0];

            pattern.Point = ShapeLoader.LoadPoint(aPointNode);
            if (pattern.Point == null)
                return null;

            pattern.Heading = GetHeading(inputNode.GetXmlNodeValue("@G"));

            return pattern;
        }

        private static AbCurve LoadAbCurvePattern(XmlNode inputNode, XmlNode lineNode)
        {
            var pattern = new AbCurve { GuidancePatternType = GuidancePatternTypeEnum.AbCurve };

            var pointNodes = lineNode.SelectNodes("PNT");
            if (pointNodes.Count == 0)
                return null;

            var line = ShapeLoader.LoadLine(pointNodes);
            if (line != null)
            {
                pattern.Shape = new List<LineString>();
                pattern.Shape.Add(line);
            }

            pattern.Heading = GetHeading(inputNode.GetXmlNodeValue("@G"));

            return pattern;
        }

        private static CenterPivot LoadCenterPivotPattern(XmlNode lineNode)
        {
            var pattern = new CenterPivot { GuidancePatternType = GuidancePatternTypeEnum.CenterPivot };

            var pointNodes = lineNode.SelectNodes("PNT");
            if (pointNodes.Count < 1 || pointNodes.Count > 3)
                return null;

            pattern.Center = ShapeLoader.LoadPoint(pointNodes[0]);
            if (pattern.Center == null)
                return null;

            pattern.StartPoint = pointNodes.Count > 1 ? ShapeLoader.LoadPoint(pointNodes[1]) : null;
            pattern.EndPoint = pointNodes.Count > 2 ? ShapeLoader.LoadPoint(pointNodes[2]) : null;

            return pattern;
        }

        private static Spiral LoadSpiralPattern(XmlNode lineNode)
        {
            var pattern = new Spiral { GuidancePatternType = GuidancePatternTypeEnum.Spiral };

            var pointNodes = lineNode.SelectNodes("PNT");
            if (pointNodes.Count == 0)
                return null;

            pattern.Shape = ShapeLoader.LoadLine(pointNodes);

            return pattern;
        }

        private static PropagationDirectionEnum GetPropagationDirection(string propagationValue)
        {
            if (string.IsNullOrEmpty(propagationValue))
                return PropagationDirectionEnum.BothDirections;

            if (string.Equals(propagationValue, "1", StringComparison.OrdinalIgnoreCase))
                return PropagationDirectionEnum.BothDirections;

            if (string.Equals(propagationValue, "2", StringComparison.OrdinalIgnoreCase))
                return PropagationDirectionEnum.LeftOnly;

            if (string.Equals(propagationValue, "3", StringComparison.OrdinalIgnoreCase))
                return PropagationDirectionEnum.RightOnly;

            return PropagationDirectionEnum.NoPropagation;
        }

        private static GuidanceExtensionEnum GetExtension(string extensionValue)
        {
            if (string.IsNullOrEmpty(extensionValue))
                return GuidanceExtensionEnum.FromBothPoints;

            if (string.Equals(extensionValue, "1", StringComparison.OrdinalIgnoreCase))
                return GuidanceExtensionEnum.FromBothPoints;

            if (string.Equals(extensionValue, "2", StringComparison.OrdinalIgnoreCase))
                return GuidanceExtensionEnum.FromA;

            if (string.Equals(extensionValue, "3", StringComparison.OrdinalIgnoreCase))
                return GuidanceExtensionEnum.FromB;

            return GuidanceExtensionEnum.None;
        }

        private static GpsSource GetGpsSource(XmlNode inputNode)
        {
            var gpsSource = new GpsSource();

            gpsSource.SourceType = GetSourceType(inputNode.GetXmlNodeValue("@I"));
            gpsSource.HorizontalAccuracy = GetAccuracy(inputNode.GetXmlNodeValue("@J"));
            gpsSource.VerticalAccuracy = GetAccuracy(inputNode.GetXmlNodeValue("@K"));
            return gpsSource;
        }

        private static GpsSourceEnum GetSourceType(string gpsType)
        {
            int sourceType;
            if (gpsType.ParseValue(out sourceType) == true)
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

        private static NumericRepresentationValue GetAccuracy(string accuracyValue)
        {
            double accuracy;
            if (accuracyValue.ParseValue(out accuracy) == false ||
                accuracy < 0 || accuracy > 65)
                return null;

            var accuracyUnitOfMeasure = UnitSystemManager.GetUnitOfMeasure("m");
            var numericValue = new NumericValue(accuracyUnitOfMeasure, accuracy);
            var numericRepresentation = new NumericRepresentation
            {
                DecimalDigits = 1,
                MaxValue = new NumericValue(accuracyUnitOfMeasure, 65),
                MinValue = new NumericValue(accuracyUnitOfMeasure, 0),
            };
            return new NumericRepresentationValue(numericRepresentation, numericValue.UnitOfMeasure, numericValue);
        }

        private static double GetHeading(string headingValue)
        {
            double heading;
            if (headingValue.ParseValue(out heading) == false)
                heading = 0;
            return heading;
        }
    }
}
