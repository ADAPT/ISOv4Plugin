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
    public interface IPointMapper
    {
        IEnumerable<ISOPoint> ExportPoints(IEnumerable<Point> adaptPolygons, ISOPointType pointType);
        ISOPoint ExportPoint(Point adaptPoint, ISOPointType pointType);
        IEnumerable<Point> ImportPoints(IEnumerable<ISOPoint> isoPolygons);
        Point ImportPoint(ISOPoint isoPoint);
    }

    public class PointMapper : BaseMapper, IPointMapper
    {
        public PointMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "PNT")
        {
        }

        #region Export
       
        public IEnumerable<ISOPoint> ExportPoints(IEnumerable<Point> adaptPoints, ISOPointType pointType)
        {
            List<ISOPoint> points = new List<ISOPoint>();
            foreach (Point adaptPoint in adaptPoints)
            {
                ISOPoint point = ExportPoint(adaptPoint, pointType);
                points.Add(point);
            }
            return points;
        }

        public ISOPoint ExportPoint(Point adaptPoint, ISOPointType pointType)
        {
            ISOPoint point = new ISOPoint();
            point.PointEast = Convert.ToDecimal(adaptPoint.X);
            point.PointNorth = Convert.ToDecimal(adaptPoint.Y);
            if (adaptPoint.Z.HasValue)
            {
                point.PointUp = (int)(adaptPoint.Z);
            }
            point.PointType = pointType;
            return point;
        }
        #endregion Export 

        #region Import

        public IEnumerable<Point> ImportPoints(IEnumerable<ISOPoint> isoPoints)
        {
            List<Point> adaptPoints = new List<Point>();
            foreach (ISOPoint isoPoint in isoPoints)
            {
                Point adaptPoint = ImportPoint(isoPoint);
                adaptPoints.Add(adaptPoint);
            }
            return adaptPoints;
        }

        public Point ImportPoint(ISOPoint isoPoint)
        {
            Point point = new Point();
            point.X = Convert.ToDouble(isoPoint.PointEast);
            point.Y = Convert.ToDouble(isoPoint.PointNorth);
            point.Z = isoPoint.PointUp;
            return point;
        }

        #endregion Import
    }
}
