/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ISOModels;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class ISOUnit
    {
        public string Code { get; private set; }
        public long Offset { get; private set; }
        public double Scale { get; private set; }

        public ISOUnit(string code, double scale, long offset)
        {
            Code = code;
            Offset = offset;
            Scale = scale;
        }

        public ISOUnit(ISODeviceValuePresentation dvp)
            :this (dvp.UnitDesignator, dvp.Scale, dvp.Offset)
        {
        }

        public ISOUnit(ISOValuePresentation vpn)
            : this(vpn.UnitDesignator, vpn.Scale, vpn.Offset)
        {
        }
    }
}
