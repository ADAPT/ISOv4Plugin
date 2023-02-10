/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IGuidancePatternMapper
    {
        IEnumerable<ISOGuidancePattern> ExportGuidancePatterns(IEnumerable<GuidancePattern> adaptPolygons);
        ISOGuidancePattern ExportGuidancePattern(GuidancePattern adaptGuidancePattern);
        IEnumerable<GuidancePattern> ImportGuidancePatterns(IEnumerable<ISOGuidancePattern> isoPolygons);
        GuidancePattern ImportGuidancePattern(ISOGuidancePattern isoGuidancePattern);
    }

    public class GuidancePatternMapper : BaseMapper, IGuidancePatternMapper
    {
        public GuidancePatternMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "GPN")
        {
        }

        #region Export

        public IEnumerable<ISOGuidancePattern> ExportGuidancePatterns(IEnumerable<GuidancePattern> adaptGuidancePatterns)
        {
            List<ISOGuidancePattern> GuidancePatterns = new List<ISOGuidancePattern>();
            foreach (GuidancePattern adaptGuidancePattern in adaptGuidancePatterns)
            {
                ISOGuidancePattern GuidancePattern = ExportGuidancePattern(adaptGuidancePattern);
                GuidancePatterns.Add(GuidancePattern);
            }
            return GuidancePatterns;
        }

        public ISOGuidancePattern ExportGuidancePattern(GuidancePattern adaptGuidancePattern)
        {
            ISOGuidancePattern gpn = new ISOGuidancePattern();

            //ID
            string gpnID = adaptGuidancePattern.Id.FindIsoId() ?? GenerateId();
            gpn.GuidancePatternId = gpnID;
            ExportIDs(adaptGuidancePattern.Id, gpnID);

            gpn.GuidancePatternDesignator = adaptGuidancePattern.Description;
            gpn.GuidancePatternType = ExportGuidancePatternType(adaptGuidancePattern.GuidancePatternType);
            gpn.PropagationDirection = ExportPropagationDirection(adaptGuidancePattern.PropagationDirection);
            gpn.Extension = ExportExtension(adaptGuidancePattern.Extension);
            gpn.Heading = ExportHeading(adaptGuidancePattern);
            if (adaptGuidancePattern.GpsSource != null)
            {
                gpn.GNSSMethod = ExportGNSSMethod(adaptGuidancePattern.GpsSource.SourceType);
                if (adaptGuidancePattern.GpsSource.HorizontalAccuracy != null)
                {
                    gpn.HorizontalAccuracy = (decimal)adaptGuidancePattern.GpsSource.HorizontalAccuracy.AsConvertedDouble("m").Value;
                }
                if (adaptGuidancePattern.GpsSource.VerticalAccuracy != null)
                {
                    gpn.VerticalAccuracy = (decimal)adaptGuidancePattern.GpsSource.VerticalAccuracy.AsConvertedDouble("m").Value;
                }
            }
            gpn.OriginalSRID = adaptGuidancePattern.OriginalEpsgCode;
            gpn.NumberOfSwathsLeft = (uint?)adaptGuidancePattern.NumbersOfSwathsLeft;
            gpn.NumberOfSwathsRight = (uint?)adaptGuidancePattern.NumbersOfSwathsRight;

            //Pattern
            LineStringMapper lineStringMapper = new LineStringMapper(TaskDataMapper);
            gpn.LineString = lineStringMapper.ExportGuidancePattern(adaptGuidancePattern);

            if (adaptGuidancePattern is PivotGuidancePattern pivot &&
                pivot.DefinitionMethod == PivotGuidanceDefinitionEnum.PivotGuidancePatternCenterRadius &&
                pivot.Radius != null)
            {
                gpn.Radius = (uint)pivot.Radius.AsConvertedInt("mm").Value;
                gpn.GuidancePatternOptions = ISOGuidancePatternOption.FullCircle;
            }

            //Boundary
            if (adaptGuidancePattern.BoundingPolygon != null)
            {
                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                gpn.BoundaryPolygons = polygonMapper.ExportMultipolygon(adaptGuidancePattern.BoundingPolygon, ISOPolygonType.Other).ToList();
            }

            return gpn;
        }

        private static ISOGuidancePatternType ExportGuidancePatternType(GuidancePatternTypeEnum enumeration)
        {
            switch (enumeration)
            {
                case GuidancePatternTypeEnum.AbLine:
                    return ISOGuidancePatternType.AB;
                case GuidancePatternTypeEnum.AbCurve:
                    return ISOGuidancePatternType.Curve;
                case GuidancePatternTypeEnum.APlus:
                    return ISOGuidancePatternType.APlus;
                case GuidancePatternTypeEnum.CenterPivot:
                    return ISOGuidancePatternType.Pivot;
                case GuidancePatternTypeEnum.Spiral:
                    return ISOGuidancePatternType.Spiral;
            }
            return ISOGuidancePatternType.AB;
        }

        private static ISOGuidancePatternGNSSMethod ExportGNSSMethod(GpsSourceEnum enumeration)
        {
            switch (enumeration)
            {
                case GpsSourceEnum.GNSSfix:
                    return ISOGuidancePatternGNSSMethod.GNSSFix;
                case GpsSourceEnum.DGNSSfix:
                    return ISOGuidancePatternGNSSMethod.DGNSSFix;
                case GpsSourceEnum.PreciseGNSS:
                    return ISOGuidancePatternGNSSMethod.PreciseGNSS;
                case GpsSourceEnum.RTKFixedInteger:
                    return ISOGuidancePatternGNSSMethod.RTKFixedInteger;
                case GpsSourceEnum.RTKFloat:
                    return ISOGuidancePatternGNSSMethod.RTKFloat;
                case GpsSourceEnum.EstDRmode:
                    return ISOGuidancePatternGNSSMethod.EstDRMode;
                case GpsSourceEnum.ManualInput:
                    return ISOGuidancePatternGNSSMethod.ManualInput;
                case GpsSourceEnum.SimulateMode:
                    return ISOGuidancePatternGNSSMethod.SimulateMode;
                case GpsSourceEnum.DesktopGeneratedData:
                    return ISOGuidancePatternGNSSMethod.DesktopGeneratedData;
                case GpsSourceEnum.Other:
                    return ISOGuidancePatternGNSSMethod.Other;
            }
            return ISOGuidancePatternGNSSMethod.Other;
        }

        private static ISOGuidancePatternPropagationDirection ExportPropagationDirection(PropagationDirectionEnum propagationDirection)
        {
            switch (propagationDirection)
            {
                case PropagationDirectionEnum.BothDirections:
                    return ISOGuidancePatternPropagationDirection.BothDirections;

                case PropagationDirectionEnum.LeftOnly:
                    return ISOGuidancePatternPropagationDirection.LeftDirectionOnly;

                case PropagationDirectionEnum.RightOnly:
                    return ISOGuidancePatternPropagationDirection.RightDirectionOnly;

                case PropagationDirectionEnum.NoPropagation:
                    return ISOGuidancePatternPropagationDirection.NoPropagation;
            }
            return ISOGuidancePatternPropagationDirection.NoPropagation;
        }

        private static ISOGuidancePatternExtension ExportExtension(GuidanceExtensionEnum extension)
        {
            switch (extension)
            {
                case GuidanceExtensionEnum.FromBothPoints:
                    return ISOGuidancePatternExtension.FromBothFirstAndLast;
                case GuidanceExtensionEnum.FromA:
                    return ISOGuidancePatternExtension.FromAOnly;
                case GuidanceExtensionEnum.FromB:
                    return ISOGuidancePatternExtension.FromBOnly;
                case GuidanceExtensionEnum.None:
                    return ISOGuidancePatternExtension.NoExtensions;
            }

            return ISOGuidancePatternExtension.NoExtensions;
        }

        private static decimal? ExportHeading(GuidancePattern guidancePattern)
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
            if (heading.HasValue)
            {
                return Convert.ToDecimal(heading);
            }
            return null;
        }




        #endregion Export

        #region Import

        public IEnumerable<GuidancePattern> ImportGuidancePatterns(IEnumerable<ISOGuidancePattern> isoGuidancePatterns)
        {
            //Import patterns
            List<GuidancePattern> adaptGuidancePatterns = new List<GuidancePattern>();
            foreach (ISOGuidancePattern isoGuidancePattern in isoGuidancePatterns)
            {
                GuidancePattern adaptGuidancePattern = ImportGuidancePattern(isoGuidancePattern);
                adaptGuidancePatterns.Add(adaptGuidancePattern);
            }

            //Add the patterns to the Catalog
            if (adaptGuidancePatterns.Any())
            {
                if (DataModel.Catalog.GuidancePatterns == null)
                {
                    DataModel.Catalog.GuidancePatterns = new List<GuidancePattern>();
                }
                DataModel.Catalog.GuidancePatterns.AddRange(adaptGuidancePatterns);
            }

            return adaptGuidancePatterns;
        }

        public GuidancePattern ImportGuidancePattern(ISOGuidancePattern isoGuidancePattern)
        {
            GuidancePattern pattern = null;
            LineStringMapper lineStringMapper = new LineStringMapper(TaskDataMapper);
            PointMapper pointMapper = new PointMapper(TaskDataMapper);
            var isoLineString = isoGuidancePattern.LineString ?? new ISOLineString();
            switch (isoGuidancePattern.GuidancePatternType)
            {
                case ISOGuidancePatternType.AB:
                    pattern = new AbLine();
                    AbLine abLine = pattern as AbLine;
                    abLine.A = pointMapper.ImportPoint(isoLineString.Points.First());
                    abLine.B = pointMapper.ImportPoint(isoLineString.Points.Last());
                    break;
                case ISOGuidancePatternType.APlus:
                    pattern = new APlus();
                    APlus aPlus = pattern as APlus;
                    aPlus.Point = pointMapper.ImportPoint(isoLineString.Points.First());
                    break;
                case ISOGuidancePatternType.Curve:
                    pattern = new AbCurve();
                    AbCurve abCurve = pattern as AbCurve;
                    abCurve.Shape = new List<LineString>() { lineStringMapper.ImportLineString(isoLineString) }; //As with export, we only have 1 linestring.
                    break;
                case ISOGuidancePatternType.Pivot:
                    pattern = new PivotGuidancePattern();
                    PivotGuidancePattern pivot = pattern as PivotGuidancePattern;
                    pivot.Center = pointMapper.ImportPoint(isoLineString.Points.First());
                    pivot.Radius = isoGuidancePattern.Radius.HasValue ? ((int)isoGuidancePattern.Radius).AsNumericRepresentationValue("mm") : null;
                    if (isoLineString.Points.Count == 1)
                    {
                        pivot.DefinitionMethod = PivotGuidanceDefinitionEnum.PivotGuidancePatternCenterRadius;
                    }
                    else if (isoLineString.Points.Count == 3)
                    {
                        pivot.DefinitionMethod = PivotGuidanceDefinitionEnum.PivotGuidancePatternStartEndCenter;
                        pivot.StartPoint = pointMapper.ImportPoint(isoLineString.Points[1]);
                        pivot.EndPoint = pointMapper.ImportPoint(isoLineString.Points[2]);
                    }
                    break;
                case ISOGuidancePatternType.Spiral:
                    pattern = new Spiral();
                    Spiral spiral = pattern as Spiral;
                    spiral.Shape = lineStringMapper.ImportLineString(isoLineString);
                    break;
            }

            //ID
            ImportIDs(pattern.Id, isoGuidancePattern.GuidancePatternId);

            pattern.Description = isoGuidancePattern.GuidancePatternDesignator;
            pattern.GuidancePatternType = ImportGuidancePatternType(isoGuidancePattern.GuidancePatternType);
            pattern.PropagationDirection = ImportPropagationDirection(isoGuidancePattern.PropagationDirection);
            pattern.Extension = ImportExtension(isoGuidancePattern.Extension);

            //Heading
            if (isoGuidancePattern.Heading.HasValue)
            {
                double heading = Convert.ToDouble(isoGuidancePattern.Heading.Value);
                if (pattern is AbLine)
                {
                    (pattern as AbLine).Heading = heading;
                }
                if (pattern is AbCurve)
                {
                    (pattern as AbCurve).Heading = heading;
                }
                if (pattern is APlus)
                {
                    (pattern as APlus).Heading = heading;
                }
            }

            pattern.GpsSource = new GpsSource();
            pattern.GpsSource.SourceType = ImportGNSSMethod(isoGuidancePattern.GNSSMethod);
            pattern.GpsSource.HorizontalAccuracy = GetAccuracy(isoGuidancePattern.HorizontalAccuracy);
            pattern.GpsSource.VerticalAccuracy = GetAccuracy(isoGuidancePattern.VerticalAccuracy);

            pattern.NumbersOfSwathsLeft = (int?)(isoGuidancePattern.NumberOfSwathsLeft);
            pattern.NumbersOfSwathsRight = (int?)(isoGuidancePattern.NumberOfSwathsRight);
            pattern.OriginalEpsgCode = isoGuidancePattern.OriginalSRID;

            if (isoLineString.LineStringWidth.HasValue)
            {
                pattern.SwathWidth = ((int)isoLineString.LineStringWidth.Value).AsNumericRepresentationValue("mm");
            }

            return pattern;
        }

        private static NumericRepresentationValue GetAccuracy(decimal? accuracyValue)
        {
            if (!accuracyValue.HasValue || accuracyValue < 0m || accuracyValue > 65m)
                return null;
            double accuracy = Convert.ToDouble(accuracyValue.Value);

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

        private GuidancePatternTypeEnum ImportGuidancePatternType(ISOGuidancePatternType gpnType)
        {
            switch (gpnType)
            {
                case ISOGuidancePatternType.AB:
                    return GuidancePatternTypeEnum.AbLine;
                case ISOGuidancePatternType.Curve:
                    return GuidancePatternTypeEnum.AbCurve;
                case ISOGuidancePatternType.APlus:
                    return GuidancePatternTypeEnum.APlus;
                case ISOGuidancePatternType.Pivot:
                    return GuidancePatternTypeEnum.CenterPivot;
                case ISOGuidancePatternType.Spiral:
                    return GuidancePatternTypeEnum.Spiral;
            }
            return GuidancePatternTypeEnum.AbLine;
        }

        private static GpsSourceEnum ImportGNSSMethod(ISOGuidancePatternGNSSMethod? enumeration)
        {
            switch (enumeration)
            {
                case ISOGuidancePatternGNSSMethod.GNSSFix:
                    return GpsSourceEnum.GNSSfix;
                case ISOGuidancePatternGNSSMethod.DGNSSFix:
                    return GpsSourceEnum.DGNSSfix;
                case ISOGuidancePatternGNSSMethod.PreciseGNSS:
                    return GpsSourceEnum.PreciseGNSS;
                case ISOGuidancePatternGNSSMethod.RTKFixedInteger:
                    return GpsSourceEnum.RTKFixedInteger;
                case ISOGuidancePatternGNSSMethod.RTKFloat:
                    return GpsSourceEnum.RTKFloat;
                case ISOGuidancePatternGNSSMethod.EstDRMode:
                    return  GpsSourceEnum.EstDRmode;
                case ISOGuidancePatternGNSSMethod.ManualInput:
                    return GpsSourceEnum.ManualInput;
                case ISOGuidancePatternGNSSMethod.SimulateMode:
                    return GpsSourceEnum.SimulateMode;
                case ISOGuidancePatternGNSSMethod.DesktopGeneratedData:
                    return GpsSourceEnum.DesktopGeneratedData;
                case ISOGuidancePatternGNSSMethod.Other:
                    return GpsSourceEnum.Other;
            }
            return GpsSourceEnum.Other;
        }

        private static PropagationDirectionEnum ImportPropagationDirection(ISOGuidancePatternPropagationDirection? propagationDirection)
        {
            switch (propagationDirection)
            {
                case ISOGuidancePatternPropagationDirection.BothDirections:
                    return PropagationDirectionEnum.BothDirections;
                case ISOGuidancePatternPropagationDirection.LeftDirectionOnly:
                    return PropagationDirectionEnum.LeftOnly;
                case ISOGuidancePatternPropagationDirection.RightDirectionOnly:
                    return PropagationDirectionEnum.RightOnly;
                case ISOGuidancePatternPropagationDirection.NoPropagation:
                    return PropagationDirectionEnum.NoPropagation;
            }
            return PropagationDirectionEnum.BothDirections;
        }

        private static GuidanceExtensionEnum ImportExtension(ISOGuidancePatternExtension? extension)
        {
            switch (extension)
            {
                case ISOGuidancePatternExtension.FromBothFirstAndLast:
                    return GuidanceExtensionEnum.FromBothPoints;
                case ISOGuidancePatternExtension.FromAOnly:
                    return GuidanceExtensionEnum.FromA;
                case ISOGuidancePatternExtension.FromBOnly:
                    return GuidanceExtensionEnum.FromB;
                case ISOGuidancePatternExtension.NoExtensions:
                    return GuidanceExtensionEnum.None;
            }
            return GuidanceExtensionEnum.None;
        }

        #endregion Import
    }
}
