using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal static class ShapeLoader
    {
        internal static MultiPolygon LoadPolygon(XmlNodeList polygonNodes)
        {
            var multiPolygon = new MultiPolygon();
            multiPolygon.Polygons = new List<Polygon>();

            foreach (XmlNode polygonNode in polygonNodes)
            {
                var polygon = LoadPolygon(polygonNode);
                if (polygon == null)
                    return null;
                multiPolygon.Polygons.Add(polygon);
            }

            if (multiPolygon.Polygons.Count == 0)
                return null;

            return multiPolygon;
        }

        private static Polygon LoadPolygon(XmlNode polygonNode)
        {
            var polygon = new Polygon { InteriorRings = new List<LinearRing>() };

            var lineNodes = polygonNode.SelectNodes("LSG");
            foreach (XmlNode lineNode in lineNodes)
            {
                bool exteriorRing;
                var line = LoadRing(lineNode, out exteriorRing);
                if (line == null)
                    return null;

                if (exteriorRing)
                    polygon.ExteriorRing = line;
                else
                    polygon.InteriorRings.Add(line);
            }

            return polygon;
        }

        private static LinearRing LoadRing(XmlNode lineNode, out bool exteriorRing)
        {
            exteriorRing = false;

            var lineType = lineNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(lineType))
                return null;

            exteriorRing = string.Equals(lineType, "1", StringComparison.OrdinalIgnoreCase);

            var line = new LinearRing { Points = new List<Point>() };

            var pointNodes = lineNode.SelectNodes("PNT");
            foreach (XmlNode pointNode in pointNodes)
            {
                var point = LoadPoint(pointNode);
                if (point == null)
                    return null;
                line.Points.Add(point);
            }

            return line;
        }

        internal static LineString LoadLine(XmlNodeList pointNodes)
        {
            var line = new LineString { Points = new List<Point>() };

            foreach (XmlNode pointNode in pointNodes)
            {
                var point = LoadPoint(pointNode);
                if (point == null)
                    return null;
                line.Points.Add(point);
            }

            return line;
        }

        internal static Point LoadPoint(XmlNode pointNode)
        {
            double latitude, longitude;
            if (pointNode.GetXmlNodeValue("@C").ParseValue(out latitude) == false ||
                pointNode.GetXmlNodeValue("@D").ParseValue(out longitude) == false)
                return null;

            var point = new Point();
            point.X = longitude;
            point.Y = latitude;

            return point;
        }
    }
}
