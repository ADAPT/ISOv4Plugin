using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.Plugins
{
    internal class CodedComment
    {
        public EnumeratedRepresentation Comment { get; set; }
        public int Scope { get; set; }
        public Dictionary<string, EnumerationMember> Values { get; set; }
    }
}
