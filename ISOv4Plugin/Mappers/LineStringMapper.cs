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

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ILineStringMapper
    {
        IEnumerable<ISOLineString> ExportLineStrings(IEnumerable<LineString> adaptLineStrings, ISOLineStringType lsgType);
        IEnumerable<LineString> ImportLineStrings(IEnumerable<ISOLineString> isoLineStrings);
        IEnumerable<ISOLineString> ExportLinearRings(IEnumerable<LinearRing> adaptLinearRings, ISOLineStringType lsgType);
        IEnumerable<LinearRing> ImportLinearRings(IEnumerable<ISOLineString> isoLineStrings);
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
            ISOLineString lineString = new ISOLineString();
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
            ISOLineString lineString = new ISOLineString();
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

        public LineString ImportLineString(ISOLineString isoLineString)
        {
            LineString lineString = new LineString();

            PointMapper pointMapper = new PointMapper(TaskDataMapper);
            lineString.Points = pointMapper.ImportPoints(isoLineString.Points).ToList();
            return lineString;
        }

        #endregion Import
    }
}
