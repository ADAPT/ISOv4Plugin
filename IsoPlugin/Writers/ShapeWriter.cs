using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.Plugins.Writers
{
    internal static class ShapeWriter
    {
        internal static void WritePolygon(XmlWriter writer, MultiPolygon multiPolygon)
        {
            if (multiPolygon.Polygons == null || 
                multiPolygon.Polygons.Count == 0)
                return;

            foreach (var polygon in multiPolygon.Polygons)
            {
                WritePolygon(writer, polygon);
            }
        }

        internal static void WritePolygon(XmlWriter writer, Polygon polygon)
        {
            if (polygon.ExteriorRing == null)
                return;

            writer.WriteStartElement("PLN");
            writer.WriteXmlAttribute("A", "1");

            WriteRing(writer, polygon.ExteriorRing, true);
            if (polygon.InteriorRings != null)
            {
                foreach (var ring in polygon.InteriorRings)
                {
                    WriteRing(writer, ring, false);
                }
            }

            writer.WriteEndElement();
        }

        private static void WriteRing(XmlWriter writer, LinearRing ring, bool isExterior)
        {
            if (ring.Points == null ||
                ring.Points.Count == 0)
                return;

            writer.WriteStartElement("LSG");
            writer.WriteXmlAttribute("A", isExterior ? "1": "2");

            for (int i = 0; i < ring.Points.Count; i++)
            {
                var pointLabel = i == 0 ? "start" : i == ring.Points.Count - 1 ? "end" : "";
                WritePoint(writer, ring.Points[i], pointLabel);
            }

            writer.WriteEndElement();
        }

        internal  static void WriteLine(XmlWriter writer, LineString line, string lineType)
        {
            if (line == null || line.Points == null || line.Points.Count == 0)
                return;

            writer.WriteStartElement("LSG");
            writer.WriteXmlAttribute("A", lineType);

            for (int i = 0; i < line.Points.Count; i++)
            {
                var pointLabel = i == 0 ? "start" : i == line.Points.Count - 1 ? "end" : "";
                WritePoint(writer, line.Points[i], pointLabel);
            }

            writer.WriteEndElement();
        }

        private static void WritePoint(XmlWriter writer, Point point, string label)
        {
            writer.WriteStartElement("PNT");
            writer.WriteXmlAttribute("A", "2");
            writer.WriteXmlAttribute("B", label);
            writer.WriteXmlAttribute("C", point.Y.ToString(CultureInfo.InvariantCulture)); // Latitude
            writer.WriteXmlAttribute("D", point.X.ToString(CultureInfo.InvariantCulture)); // Longitude

            writer.WriteEndElement();
        }
    }
}