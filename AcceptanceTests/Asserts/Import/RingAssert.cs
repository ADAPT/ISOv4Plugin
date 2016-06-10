using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;

namespace AcceptanceTests.Asserts.Import
{
    public class RingAssert
    {
        public static void AreEqual(List<XmlNode> nodes, List<LinearRing> rings)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                AreEqual(nodes[i], rings[i]);
            }
        }

        public static void AreEqual(XmlNode node, LinearRing ring)
        {
            var pointNodes = node.SelectNodes("PNT");
            var points = ring.Points;
            for (int i = 0; i < pointNodes.Count; i++)
            {
                PointAssert.AreEqual(pointNodes[i], points[i]);
            }
        }
    }
}