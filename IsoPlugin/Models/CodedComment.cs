using AgGateway.ADAPT.ApplicationDataModel.Representations;
using System.Collections.Generic;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal class CodedComment
    {
        public EnumeratedRepresentation Comment { get; set; }
        public int Scope { get; set; }
        public Dictionary<string, EnumerationMember> Values { get; set; }
    }
}
