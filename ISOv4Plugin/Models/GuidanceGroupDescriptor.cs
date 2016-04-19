using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class GuidanceGroupDescriptor
    {
        public GuidanceGroupDescriptor(GuidanceGroup group, Dictionary<string, GuidancePattern> patterns)
        {
            Group = group;
            Patterns = patterns;
        }

        public GuidanceGroup Group { get; private set; }
        public Dictionary<string, GuidancePattern> Patterns { get; private set; }
    }
}
