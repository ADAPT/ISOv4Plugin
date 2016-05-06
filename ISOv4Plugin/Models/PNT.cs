using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PNT : IWriter
    {
        public PNTA A { get; set; }
        public decimal C { get; set; }
        public decimal D { get; set; }
        public long E { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<PNT ");
            xmlBuilder.Append(string.Format("A=\"{0}\" ", (int)A));
            xmlBuilder.Append(string.Format("C=\"{0}\" ", C));
            xmlBuilder.Append(string.Format("D=\"{0}\" ", D));
            xmlBuilder.Append(string.Format("E=\"{0}\" ", E));
            xmlBuilder.Append(">");
            xmlBuilder.Append("</PNT>");
            return xmlBuilder.ToString();
        }
    }
}
