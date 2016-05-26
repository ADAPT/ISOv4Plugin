using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class GuidancePatternWriter : BaseWriter
    {
        private static Dictionary<GpsSourceEnum, string> _SourceMapping = new Dictionary<GpsSourceEnum, string>()
        {
            {GpsSourceEnum.GNSSfix, "1" },
            {GpsSourceEnum.DGNSSfix, "2" },
            {GpsSourceEnum.PreciseGNSS, "3" },
            {GpsSourceEnum.RTKFixedInteger, "4" },
            {GpsSourceEnum.RTKFloat, "5" },
            {GpsSourceEnum.EstDRmode, "6" },
            {GpsSourceEnum.ManualInput, "7" },
            {GpsSourceEnum.SimulateMode, "8" },
            {GpsSourceEnum.DesktopGeneratedData, "16" },
            {GpsSourceEnum.Other, "17" }
        };

        public GuidancePatternWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "GPN")
        {
        }

        public void Write(XmlWriter writer, GuidancePattern guidancePattern)
        {
            if (guidancePattern == null)
                return;

            WriteGuidancePattern(writer, guidancePattern);
        }

        private void WriteGuidancePattern(XmlWriter writer, GuidancePattern guidancePattern)
        {
            var patternType = ValidateAndGetPatternType(guidancePattern);
            // Bail out for inconsistent pattern type.
            if (string.IsNullOrEmpty(patternType))
                return;

            var guidancePatternId = guidancePattern.Id.FindIsoId() ?? GenerateId();
            TaskWriter.Ids.Add(guidancePatternId, guidancePattern.Id);

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", guidancePatternId);
            writer.WriteXmlAttribute("B", guidancePattern.Description);
            writer.WriteAttributeString("C", patternType);
            writer.WriteXmlAttribute("D", GetPropagationDirection(guidancePattern.PropagationDirection));
            writer.WriteXmlAttribute("E", GetExtension(guidancePattern.Extension));
            writer.WriteXmlAttribute("G", GetHeading(guidancePattern));
            writer.WriteXmlAttribute("M", guidancePattern.OriginalEpsgCode);

            WriteGpsData(writer, guidancePattern.GpsSource);
            WritePattern(writer, guidancePattern);

            WriteBoundary(writer, guidancePattern.BoundingPolygon);

            writer.WriteEndElement();
        }

        private static string ValidateAndGetPatternType(GuidancePattern guidancePattern)
        {
            switch (guidancePattern.GuidancePatternType)
            {
                case GuidancePatternTypeEnum.AbLine:
                    {
                        var abLine = guidancePattern as AbLine;
                        if (abLine == null || abLine.A == null || abLine.B == null)
                            return null;
                    }
                    return "1";

                case GuidancePatternTypeEnum.APlus:
                    {
                        var aPlus = guidancePattern as APlus;
                        if (aPlus == null || aPlus.Point == null)
                            return null;
                        return "2";
                    }

                case GuidancePatternTypeEnum.AbCurve:
                    {
                        var abCurve = guidancePattern as AbCurve;
                        if (abCurve == null || abCurve.Shape == null || abCurve.Shape.Count < 1)
                            return null;
                        return "3";
                    }

                case GuidancePatternTypeEnum.CenterPivot:
                    {
                        var pivot = guidancePattern as CenterPivot;
                        if (pivot == null || pivot.Center == null)
                            return null;
                        return "4";
                    }

                case GuidancePatternTypeEnum.Spiral:
                    {
                        var spiral = guidancePattern as Spiral;
                        if (spiral == null || spiral.Shape == null)
                            return null;
                        return "5";
                    }
            }
            return null;
        }

        private static string GetPropagationDirection(PropagationDirectionEnum propagationDirection)
        {
            switch (propagationDirection)
            {
                case PropagationDirectionEnum.BothDirections:
                    return null;

                case PropagationDirectionEnum.LeftOnly:
                    return "2";

                case PropagationDirectionEnum.RightOnly:
                    return "3";

                case PropagationDirectionEnum.NoPropagation:
                    return "4";
            }
            return null;
        }

        private static string GetExtension(GuidanceExtensionEnum extension)
        {
            switch (extension)
            {
                case GuidanceExtensionEnum.FromA:
                    return "2";

                case GuidanceExtensionEnum.FromB:
                    return "3";

                case GuidanceExtensionEnum.FromBothPoints:
                    return "1";

                case GuidanceExtensionEnum.None:
                    return "4";
            }

            return null;
        }

        private static string GetHeading(GuidancePattern guidancePattern)
        {
            double? heading = null;
            switch (guidancePattern.GuidancePatternType)
            {
                case GuidancePatternTypeEnum.AbCurve:
                    heading = (guidancePattern as AbCurve).Heading;
                    break;

                case GuidancePatternTypeEnum.AbLine:
                    heading = (guidancePattern as AbLine).Heading;
                    break;

                case GuidancePatternTypeEnum.APlus:
                    heading = (guidancePattern as APlus).Heading;
                    break;
            }

            if (!heading.HasValue)
                return null;

            return heading.Value.ToString(CultureInfo.InvariantCulture);
        }

        private static string WriteGpsData(XmlWriter writer, GpsSource gpsSource)
        {
            if (gpsSource == null)
                return null;

            if (_SourceMapping.ContainsKey(gpsSource.SourceType))
                writer.WriteXmlAttribute("I", _SourceMapping[gpsSource.SourceType]);

            writer.WriteXmlAttribute("J", GetAccuracy(gpsSource.HorizontalAccuracy));
            writer.WriteXmlAttribute("K", GetAccuracy(gpsSource.VerticalAccuracy));

            return null;
        }

        private static string GetAccuracy(NumericRepresentationValue accuracyValue)
        {
            if (accuracyValue == null || accuracyValue.Value == null)
                return null;

            var accuracy = accuracyValue.Value.ConvertToUnit(InternalUnitSystemManager.Instance.UnitOfMeasures["m"]);

            return accuracy.ToString(CultureInfo.InvariantCulture);
        }

        private static void WritePattern(XmlWriter writer, GuidancePattern guidancePattern)
        {
            switch (guidancePattern.GuidancePatternType)
            {
                case GuidancePatternTypeEnum.AbCurve:
                    WriteAbCurve(writer, guidancePattern as AbCurve);
                    break;

                case GuidancePatternTypeEnum.AbLine:
                    WriteAbLine(writer, guidancePattern as AbLine);
                    break;

                case GuidancePatternTypeEnum.APlus:
                    WriteAplus(writer, guidancePattern as APlus);
                    break;

                case GuidancePatternTypeEnum.CenterPivot:
                    WritePivot(writer, guidancePattern as CenterPivot);
                    break;

                case GuidancePatternTypeEnum.Spiral:
                    WriteSpiral(writer, guidancePattern as Spiral);
                    break;
            }
        }

        private static void WriteAbCurve(XmlWriter writer, AbCurve abCurve)
        {
            ShapeWriter.WriteLine(writer, abCurve.Shape[0], "5");
        }

        private static void WriteAbLine(XmlWriter writer, AbLine abLine)
        {
            var line = new LineString { Points = new List<Point>() };
            line.Points.Add(abLine.A);
            line.Points.Add(abLine.B);

            ShapeWriter.WriteLine(writer, line, "5");
        }

        private static void WriteAplus(XmlWriter writer, APlus aPlus)
        {
            var line = new LineString { Points = new List<Point>() };
            line.Points.Add(aPlus.Point);

            ShapeWriter.WriteLine(writer, line, "5");
        }

        private static void WritePivot(XmlWriter writer, CenterPivot centerPivot)
        {
            var line = new LineString { Points = new List<Point>() };
            line.Points.Add(centerPivot.Center);

            if (centerPivot.StartPoint != null)
            {
                line.Points.Add(centerPivot.StartPoint);
                if (centerPivot.EndPoint != null)
                    line.Points.Add(centerPivot.EndPoint);
            }

            ShapeWriter.WriteLine(writer, line, "5");
        }

        private static void WriteSpiral(XmlWriter writer, Spiral spiral)
        {
            ShapeWriter.WriteLine(writer, spiral.Shape, "5");
        }

        private static void WriteBoundary(XmlWriter writer, MultiPolygon boundary)
        {
            if (boundary == null || boundary.Polygons == null || boundary.Polygons.Count == 0)
                return;

            // Guidance pattern only supports a single polygon-based boundary
            var polygon = boundary.Polygons[0];
            ShapeWriter.WritePolygon(writer, polygon);
        }
    }
}