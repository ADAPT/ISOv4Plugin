using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface IPtnReader
    {
        PTNHeader Read(XElement ptnElement);
    }

    public class PtnReader : IPtnReader
    {
        public PTNHeader Read(XElement ptnElement)
        {
            if (ptnElement == null)
                return null;

            return new PTNHeader
            {
                PositionNorth = new HeaderProperty(ptnElement.Attribute("A")),
                PositionEast = new HeaderProperty(ptnElement.Attribute("B")),
                PositionUp = new HeaderProperty(ptnElement.Attribute("C")),
                PositionStatus = new HeaderProperty(ptnElement.Attribute("D")),
                PDOP = new HeaderProperty(ptnElement.Attribute("E")),
                HDOP = new HeaderProperty(ptnElement.Attribute("F")),
                NumberOfSatellites = new HeaderProperty(ptnElement.Attribute("G")),
                GpsUtcTime = new HeaderProperty(ptnElement.Attribute("H")),
                GpsUtcDate = new HeaderProperty(ptnElement.Attribute("I")),
            };

        }
    }
}
