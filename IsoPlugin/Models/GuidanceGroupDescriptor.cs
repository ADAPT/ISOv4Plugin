using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.Plugins
{
    internal class GuidanceGroupDescriptor
    {
        internal GuidanceGroupDescriptor(GuidanceGroup group, Dictionary<string, GuidancePattern> patterns)
        {
            Group = group;
            Patterns = patterns;
        }

        internal GuidanceGroup Group { get; private set; }
        internal Dictionary<string, GuidancePattern> Patterns { get; private set; }
    }
}
