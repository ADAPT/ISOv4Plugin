namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class DLVHeader
    {
        public HeaderProperty ProcessDataDDI { get; set; }

        public HeaderProperty ProcessDataValue { get; set; }

        public HeaderProperty DeviceElementIdRef { get; set; }

        public HeaderProperty DataLogPGN { get; set; }

        public HeaderProperty DataLogPGNStartBit { get; set; }

        public HeaderProperty DataLogPGNStopBit { get; set; }
    }
}
