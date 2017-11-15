/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.UnitSystem;

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
            ExportUniqueIDs(adaptGuidancePattern.Id, gpnID);
            TaskDataMapper.ISOIdMap.Add(adaptGuidancePattern.Id.ReferenceId, gpnID);

            gpn.GuidancePatternDesignator = adaptGuidancePattern.Description;
            gpn.GuidancePatternType = ExportGuidancePatternType(adaptGuidancePattern.GuidancePatternType);
            gpn.PropagationDirection = ExportPropagationDirection(adaptGuidancePattern.PropagationDirection);
            gpn.Extension = ExportExtension(adaptGuidancePattern.Extension);
            gpn.Heading = ExportHeading(adaptGuidancePattern);

            //if (adaptGuidancePattern is PivotGuidancePattern)
            //{
            //    //gpn.Radius = ?//TODO
            //    //gpn.GuidancePatternOptions = ?//TODO
            //}

            gpn.GNSSMethod = ExportGNSSMethod(adaptGuidancePattern.GpsSource.SourceType);
            gpn.HorizontalAccuracy = (decimal)adaptGuidancePattern.GpsSource.HorizontalAccuracy.AsConvertedDouble("m");
            gpn.VerticalAccuracy = (decimal)adaptGuidancePattern.GpsSource.VerticalAccuracy.AsConvertedDouble("m"); 
            gpn.OriginalSRID = adaptGuidancePattern.OriginalEpsgCode;
            gpn.NumberOfSwathsLeft = adaptGuidancePattern.NumbersOfSwathsLeft;
            gpn.NumberOfSwathsRight = adaptGuidancePattern.NumbersOfSwathsRight;

            //Pattern
            LineStringMapper lineStringMapper = new LineStringMapper(TaskDataMapper);
            switch (adaptGuidancePattern.GuidancePatternType)
            {
                case GuidancePatternTypeEnum.AbCurve:
                    AbCurve curve = adaptGuidancePattern as AbCurve;
                    gpn.LineString = lineStringMapper.ExportLineString(curve.Shape[0], ISOLineStringType.GuidancePattern); //Only first linestring used.
                    break;

                case GuidancePatternTypeEnum.AbLine:
                    AbLine abLine = adaptGuidancePattern as AbLine;
                    LineString line = new LineString { Points = new List<Point>() };
                    line.Points.Add(abLine.A);
                    line.Points.Add(abLine.B);
                    gpn.LineString = lineStringMapper.ExportLineString(line, ISOLineStringType.GuidancePattern);
                    break;
                case GuidancePatternTypeEnum.APlus:
                    APlus aPlus = adaptGuidancePattern as APlus;
                    LineString aPlusLine = new LineString { Points = new List<Point>() };
                    aPlusLine.Points.Add(aPlus.Point);
                    gpn.LineString = lineStringMapper.ExportLineString(aPlusLine, ISOLineStringType.GuidancePattern);
                    break;
                case GuidancePatternTypeEnum.CenterPivot:
                    PivotGuidancePattern pivot = adaptGuidancePattern as PivotGuidancePattern;
                    LineString pivotLine = new LineString { Points = new List<Point>() };
                    pivotLine.Points.Add(pivot.Center);

                    if (pivot.StartPoint != null)
                    {
                        pivotLine.Points.Add(pivot.StartPoint);
                        if (pivot.EndPoint != null)
                        {
                            pivotLine.Points.Add(pivot.EndPoint);
                        }
                    }
                    gpn.LineString = lineStringMapper.ExportLineString(pivotLine, ISOLineStringType.GuidancePattern);
                    break;
                case GuidancePatternTypeEnum.Spiral:
                    Spiral spiral = adaptGuidancePattern as Spiral;
                    gpn.LineString = lineStringMapper.ExportLineString(spiral.Shape, ISOLineStringType.GuidancePattern);
                    break;
            }

            //Boundary
            if (adaptGuidancePattern.BoundingPolygon != null)
            {
                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                gpn.BoundaryPolygons = polygonMapper.ExportPolygons(adaptGuidancePattern.BoundingPolygon.Polygons, ISOPolygonType.Other).ToList();
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
            switch (isoGuidancePattern.GuidancePatternType)
            {
                case ISOGuidancePatternType.AB:
                    pattern = new AbLine();
                    break;
                case ISOGuidancePatternType.APlus:
                    pattern = new APlus();
                    break;
                case ISOGuidancePatternType.Curve:
                    pattern = new AbCurve();
                    break;
                case ISOGuidancePatternType.Pivot:
                    pattern = new PivotGuidancePattern();
                    break;
                case ISOGuidancePatternType.Spiral:
                    pattern = new Spiral();
                    break;
            }

            //ID
            pattern.Id.UniqueIds.AddRange(ImportUniqueIDs(isoGuidancePattern.GuidancePatternId));
            TaskDataMapper.ADAPTIdMap.Add(isoGuidancePattern.GuidancePatternId, pattern.Id.ReferenceId);

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
