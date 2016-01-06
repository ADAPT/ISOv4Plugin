using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.Plugins
{
    internal class IsoUnit
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
