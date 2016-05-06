using System;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TIM : IWriter
    {
        public DateTime A { get; set; }
        public DateTime B { get; set; }
        public bool BSpecified { get; set; }
        public TIMD D { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<TIM ");
            xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            xmlBuilder.Append(string.Format("D=\"{0}\" ", (int)D));
            xmlBuilder.Append(">");
            xmlBuilder.Append("</TIM>");
            return xmlBuilder.ToString();
        }
    }
}
