using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PTN : IWriter
    {
        public double? A { get; set; }
        public bool ASpecified { get; set; }
        public double? B { get; set; }
        public bool BSpecified { get; set; }
        public long? C { get; set; }
        public bool CSpecified { get; set; }
        public byte? D { get; set; }
        public bool DSpecified { get; set; }
        public double? E { get; set; }
        public bool ESpecified { get; set; }
        public double? F { get; set; }
        public bool FSpecified { get; set; }
        public byte? G { get; set; }
        public bool GSpecified { get; set; }
        public ulong? H { get; set; }
        public bool HSpecified { get; set; }
        public ushort? I { get; set; }
        public bool ISpecified { get; set; }



        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
