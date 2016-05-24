using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class PointAssert
    {
        public static void AreEqual(XmlNode pointNode, Point point)
        {
            Assert.AreEqual(double.Parse(pointNode.GetXmlAttribute("C")), point.Y);
            Assert.AreEqual(double.Parse(pointNode.GetXmlAttribute("D")), point.X);
        }
    }
}