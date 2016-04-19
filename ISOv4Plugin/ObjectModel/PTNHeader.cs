namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class PTNHeader
    {
        public HeaderProperty PositionNorth { get; set; }

        public HeaderProperty PositionEast { get; set; }

        public HeaderProperty PositionUp { get; set; }

        public HeaderProperty PositionStatus { get; set; }

        public HeaderProperty PDOP { get; set; }

        public HeaderProperty HDOP { get; set; }

        public HeaderProperty NumberOfSatellites { get; set; }

        public HeaderProperty GpsUtcTime { get; set; }

        public HeaderProperty GpsUtcDate { get; set; }
    }
}