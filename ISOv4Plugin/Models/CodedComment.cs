using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Representations;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CodedComment
    {
        public EnumeratedRepresentation Comment { get; set; }
        public int Scope { get; set; }
        public Dictionary<string, EnumerationMember> Values { get; set; }
    }
}
