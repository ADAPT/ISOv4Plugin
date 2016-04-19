using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class ValuePresentation
    {
        public ValuePresentation()
        {
            Id = new CompoundIdentifier(0);
        }
        public string Symbol { get; set; }
        public double Offset { get; set; }
        public double Scale { get; set; }
        public byte DecimalDigits { get; set; }

        public CompoundIdentifier Id { get; private set; }
    }
}
