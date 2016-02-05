using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using System.Collections.Generic;

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
