using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CTR : IWriter
    {
        public string A { get; set; }
        public string B { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<CTR ");
            if (!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            if (!string.IsNullOrEmpty(B)) xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            xmlBuilder.Append(">");
            xmlBuilder.Append("</CTR>");
            return xmlBuilder.ToString();
        }
    }
}