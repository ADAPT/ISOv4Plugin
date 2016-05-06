using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class TIMHeader
    {
        public HeaderProperty Start { get; set; }
        public HeaderProperty Stop { get; set; }
        public HeaderProperty Duration { get; set; }
        public HeaderProperty Type { get; set; }

        public PTNHeader PtnHeader { get; set; }

        public List<DLVHeader> DLVs { get; set; } 
    }
}