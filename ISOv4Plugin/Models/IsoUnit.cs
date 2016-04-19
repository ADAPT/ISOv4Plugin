namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class IsoUnit
    {
        public string Code { get; private set; }
        public long Offset { get; private set; }
        public double Scale { get; private set; }

        public IsoUnit(string code, double scale, long offset)
        {
            Code = code;
            Offset = offset;
            Scale = scale;
        }
    }
}
