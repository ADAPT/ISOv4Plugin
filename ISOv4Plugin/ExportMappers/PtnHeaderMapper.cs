using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IPtnHeaderMapper
    {
        PTNHeader Map();

    }

    public class PtnHeaderMapper : IPtnHeaderMapper
    {
        public PTNHeader Map()
        {
            return new PTNHeader
            {
                GpsUtcDate = new HeaderProperty { State = HeaderPropertyState.IsNull },
                GpsUtcTime = new HeaderProperty { State = HeaderPropertyState.IsNull },
                HDOP = new HeaderProperty { State = HeaderPropertyState.IsNull},
                NumberOfSatellites = new HeaderProperty { State = HeaderPropertyState.IsNull},
                PDOP = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PositionStatus = new HeaderProperty { State = HeaderPropertyState.IsNull },
                PositionEast = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
                PositionNorth = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
                PositionUp = new HeaderProperty { State = HeaderPropertyState.IsEmpty },
            };
        }
    }
}
