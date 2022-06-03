/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ILineStringMapper
    {
        IEnumerable<ISOLineString> ExportLineStrings(IEnumerable<LineString> adaptLineStrings, ISOLineStringType lsgType);
        IEnumerable<LineString> ImportLineStrings(IEnumerable<ISOLineString> isoLineStrings);
        IEnumerable<AttributeShape> ImportAttributeLineStrings(IEnumerable<ISOLineString> isoLineStrings);
        IEnumerable<ISOLineString> ExportLinearRings(IEnumerable<LinearRing> adaptLinearRings, ISOLineStringType lsgType);
        IEnumerable<LinearRing> ImportLinearRings(IEnumerable<ISOLineString> isoLineStrings);
        ISOLineString ExportGuidancePattern(GuidancePattern adaptGuidancePattern);
    }

    public class LineStringMapper : BaseMapper, ILineStringMapper
    {
        public LineStringMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "LSG")
        {
        }

        #region Export

        public IEnumerable<ISOLineString> ExportLineStrings(IEnumerable<LineString> adaptLineStrings, ISOLineStringType lsgType)
        {
            List<ISOLineString> lineStrings = new List<ISOLineString>();
            foreach (LineString adaptLineString in adaptLineStrings)
            {
                ISOLineString lineString = ExportLineString(adaptLineString, lsgType);
                lineStrings.Add(lineString);
            }
            return lineStrings;
        }

        public ISOLineString ExportLineString(LineString adaptLineString, ISOLineStringType lsgType)
        {
            ISOLineString lineString = new ISOLineString(TaskDataMapper.Version);
            lineString.LineStringType = lsgType;

            ISOPointType pointType = ISOPointType.Other;
            switch (lsgType)
            {
                case ISOLineStringType.PolygonExterior:
                case ISOLineStringType.PolygonInterior:
                    pointType = ISOPointType.PartfieldReference;
                    break;
                case ISOLineStringType.GuidancePattern:
                    pointType = ISOPointType.GuidancePoint;
                    break;
                case ISOLineStringType.Flag:
                    pointType = ISOPointType.Flag;
                    break;
                case ISOLineStringType.Obstacle:
                    pointType = ISOPointType.Obstacle;
                    break;
            }

            PointMapper pointMapper = new PointMapper(TaskDataMapper);
            lineString.Points = pointMapper.ExportPoints(adaptLineString.Points, pointType).ToList();
            return lineString;
        }

        public ISOLineString ExportGuidancePattern(GuidancePattern adaptGuidancePattern)
        {
            ISOLineString lineString = new ISOLineString(TaskDataMapper.Version);
            lineString.LineStringType = ISOLineStringType.GuidancePattern;

            PointMapper pointMapper = new PointMapper(TaskDataMapper);

            List<Point> adaptPoints;
            switch (adaptGuidancePattern.GuidancePatternType)
            {
                case GuidancePatternTypeEnum.AbCurve:
                    AbCurve curve = adaptGuidancePattern as AbCurve;
                    adaptPoints = curve.Shape[0].Points; //Only first linestring used.
                    break;

                case GuidancePatternTypeEnum.AbLine:
                    AbLine abLine = adaptGuidancePattern as AbLine;
                    adaptPoints = new List<Point>();
                    adaptPoints.Add(abLine.A);
                    adaptPoints.Add(abLine.B);
                    break;
                case GuidancePatternTypeEnum.APlus:
                    APlus aPlus = adaptGuidancePattern as APlus;
                    adaptPoints = new List<Point>();
                    adaptPoints.Add(aPlus.Point);
                    break;
                case GuidancePatternTypeEnum.CenterPivot:
                    PivotGuidancePattern pivot = adaptGuidancePattern as PivotGuidancePattern;
                    adaptPoints = new List<Point>();
                    lineString.Points.Add(pointMapper.ExportPoint(pivot.Center, ISOPointType.GuidanceReferenceCenter));

                    if (pivot.DefinitionMethod == PivotGuidanceDefinitionEnum.PivotGuidancePatternStartEndCenter &&
                        pivot.StartPoint != null &&
                        pivot.EndPoint != null)
                    {
                        adaptPoints.Add(pivot.StartPoint);
                        adaptPoints.Add(pivot.EndPoint);
                    }
                    break;
                case GuidancePatternTypeEnum.Spiral:
                    Spiral spiral = adaptGuidancePattern as Spiral;
                    adaptPoints = spiral.Shape.Points;
                    break;
                default:
                    return null;
            }

            for (int i = 0; i < adaptPoints.Count; i++)
            {
                ISOPointType pointType = i == 0
                    ? ISOPointType.GuidanceReferenceA
                    : (i == adaptPoints.Count - 1
                        ? ISOPointType.GuidanceReferenceB
                        : ISOPointType.GuidancePoint);

                lineString.Points.Add(pointMapper.ExportPoint(adaptPoints[i], pointType));
            }

            return lineString;
        }

        public IEnumerable<ISOLineString> ExportLinearRings(IEnumerable<LinearRing> adaptRings, ISOLineStringType lsgType)
        {
            List<ISOLineString> lineStrings = new List<ISOLineString>();
            foreach (LinearRing ring in adaptRings)
            {
                ISOLineString lineString = ExportLinearRing(ring, lsgType);
                lineStrings.Add(lineString);
            }
            return lineStrings;
        }

        public ISOLineString ExportLinearRing(LinearRing ring, ISOLineStringType lsgType)
        {
            ISOLineString lineString = new ISOLineString(TaskDataMapper.Version);
            lineString.LineStringType = lsgType;

            ISOPointType pointType = ISOPointType.Other;
            if (TaskDataMapper.Version > 3)
            {
                switch (lsgType)
                {
                    case ISOLineStringType.PolygonExterior:
                    case ISOLineStringType.PolygonInterior:
                        pointType = ISOPointType.PartfieldReference;
                        break;
                    case ISOLineStringType.GuidancePattern:
                        pointType = ISOPointType.GuidancePoint;
                        break;
                    case ISOLineStringType.Flag:
                        pointType = ISOPointType.Flag;
                        break;
                    case ISOLineStringType.Obstacle:
                        pointType = ISOPointType.Obstacle;
                        break;
                }
            }
            else if (lineString.LineStringType == ISOLineStringType.Flag)
            {
                //Flag & Other (default) are the only 2 options for version 3
                pointType = ISOPointType.Flag;
            }

            PointMapper pointMapper = new PointMapper(TaskDataMapper);
            lineString.Points = pointMapper.ExportPoints(ring.Points, pointType).ToList();
            return lineString;
        }
        #endregion Export 

        #region Import

        public IEnumerable<LinearRing> ImportLinearRings(IEnumerable<ISOLineString> isoLineStrings)
        {
            List<LinearRing> adaptLineStrings = new List<LinearRing>();
            foreach (ISOLineString isoLineString in isoLineStrings)
            {
                LinearRing adaptLinearRing = ImportLinearRing(isoLineString);
                adaptLineStrings.Add(adaptLinearRing);
            }
            return adaptLineStrings;
        }

        public LinearRing ImportLinearRing(ISOLineString isoLineString)
        {
            LinearRing linearRing = new LinearRing();

            PointMapper pointMapper = new PointMapper(TaskDataMapper);
            linearRing.Points = pointMapper.ImportPoints(isoLineString.Points).ToList();
            return linearRing;
        }

        public IEnumerable<LineString> ImportLineStrings(IEnumerable<ISOLineString> isoLineStrings)
        {
            List<LineString> adaptLineStrings = new List<LineString>();
            foreach (ISOLineString isoLineString in isoLineStrings)
            {
                LineString adaptLineString = ImportLineString(isoLineString);
                adaptLineStrings.Add(adaptLineString);
            }
            return adaptLineStrings;
        }

        public IEnumerable<AttributeShape> ImportAttributeLineStrings(IEnumerable<ISOLineString> isoLineStrings)
        {
            List<AttributeShape> attributeLinestrings = new List<AttributeShape>();
            foreach (ISOLineString isoLineString in isoLineStrings)
            {
                AttributeShape lineString = ImportAttributeLineString(isoLineString);
                if (lineString != null)
                {
                    attributeLinestrings.Add(lineString);
                }               
            }
            return attributeLinestrings;
        }

        public LineString ImportLineString(ISOLineString isoLineString)
        {
            LineString lineString = new LineString();
            PointMapper pointMapper = new PointMapper(TaskDataMapper);
            lineString.Points = pointMapper.ImportPoints(isoLineString.Points).ToList();
            return lineString;
        }

        public AttributeShape ImportAttributeLineString(ISOLineString isoLineString)
        {
            if (IsFieldAttributeType(isoLineString))
            {
                return new AttributeShape(){
                    Shape = ImportLineString(isoLineString),
                    TypeName = Enum.GetName(typeof(ISOLineStringType), isoLineString.LineStringType),
                    Name = isoLineString.LineStringDesignator };
            }
            return null;
        }

        internal static bool IsFieldAttributeType(ISOLineString isoLineString)
        {
            return isoLineString.LineStringType == ISOLineStringType.Drainage ||
                isoLineString.LineStringType == ISOLineStringType.Fence ||
                isoLineString.LineStringType == ISOLineStringType.Flag ||
                isoLineString.LineStringType == ISOLineStringType.Obstacle ||
                isoLineString.LineStringType == ISOLineStringType.SamplingRoute ||
                isoLineString.LineStringType == ISOLineStringType.TramLine;
        }

        #endregion Import
    }
}
