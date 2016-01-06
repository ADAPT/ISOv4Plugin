using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.Plugins
{
    internal class ValuePresentation
    {
        internal ValuePresentation()
        {
            Id = new CompoundIdentifier(0);
        }
        internal string Symbol { get; set; }
        internal double Offset { get; set; }
        internal double Scale { get; set; }
        internal byte DecimalDigits { get; set; }

        internal CompoundIdentifier Id { get; private set; }
    }
}
