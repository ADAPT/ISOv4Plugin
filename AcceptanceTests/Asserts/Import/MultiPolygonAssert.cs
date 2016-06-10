using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AcceptanceTests.Asserts.Import
{
    public class MultiPolygonAssert
    {
        public static void AreEqual(XmlNodeList polygonNodes, MultiPolygon multiPolygon)
        {
            for (int i = 0; i < polygonNodes.Count; i++)
            {
                AreEqual(polygonNodes[i], multiPolygon.Polygons[i]);
            }
        }

        private static void AreEqual(XmlNode polygonNode, Polygon polygon)
        {
            var lineNodes = polygonNode.SelectNodes("LSG");
            var exteriorNode = GetExteriorNode(lineNodes);
            var interiorNodes = GetInteriorNodes(lineNodes);
            var exteriorRing = polygon.ExteriorRing;
            var interiorRings = polygon.InteriorRings;

            RingAssert.AreEqual(exteriorNode, exteriorRing);
            RingAssert.AreEqual(interiorNodes, interiorRings);
        }

        private static XmlNode GetExteriorNode(XmlNodeList lineNodes)
        {
            for (int i = 0; i < lineNodes.Count; i++)
            {
                if (lineNodes[i].GetXmlAttribute("A") == "1")
                    return lineNodes[i];
            }
            return null;
        }

        private static List<XmlNode> GetInteriorNodes(XmlNodeList lineNodes)
        {
            var nodes = new List<XmlNode>();
            for (int i = 0; i < lineNodes.Count; i++)
            {
                if (lineNodes[i].GetXmlAttribute("A") == "2")
                    nodes.Add(lineNodes[i]);
            }
            return nodes;
        }
    }
}