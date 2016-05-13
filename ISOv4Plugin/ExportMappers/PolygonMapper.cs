using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IPolygonMapper
    {
        PLN Map(MultiPolygon polygon, BoundaryType type, string name);
    }

    public class PolygonMapper : IPolygonMapper
    {
        public PLN Map(MultiPolygon polygon, BoundaryType type, string name)
        {
            var isoPolygon = new PLN
            {
                A = ConvertPolygonType(type),
                B = name
            };
            var exteriors = polygon.Polygons.Select(p => p.ExteriorRing).SelectMany(x => Map(x, LSGA.Item1));
            var interiors = polygon.Polygons.SelectMany(p => p.InteriorRings).SelectMany(x => Map(x, LSGA.Item2));
            isoPolygon.Items = exteriors.Concat(interiors).ToArray();

            return isoPolygon;
        }

        public PLNA ConvertPolygonType(BoundaryType polygonType)
        {
            if (polygonType == BoundaryType.Field)
                return PLNA.Item1;
            if (polygonType == BoundaryType.CropZone)
                return PLNA.Item2;
            return PLNA.Item8; //other
        }

        private IEnumerable<LSG> Map(LinearRing ring, LSGA ringType)
        {
            var isoLineStrings = new List<LSG>
            {
                new LSG
                {
                    A = ringType,
                    Items = ring.Points.Select(Map).ToArray()
                }
            };

            return isoLineStrings;
        }

        private PNT Map(Point coordinate)
        {
            var map = new PNT
                {
                    A = PNTA.Item2, //Other
                    C = new decimal(coordinate.Y), 
                    D = new decimal(coordinate.X)
                };
            if (coordinate.Z != 0)
            {
                map.E = (long) Math.Round(coordinate.Z.GetValueOrDefault(), 0);
            }

            return map;
        }
    }

    public enum BoundaryType
    {
        Field,
        CropZone
    }
}
