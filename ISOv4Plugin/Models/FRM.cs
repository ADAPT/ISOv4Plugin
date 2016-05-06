using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class FRM : IWriter
    {
        public string A { get; set; }
        public string B { get; set; }
        public string I { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<FRM ");
            if (!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            if (!string.IsNullOrEmpty(B)) xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            if (!string.IsNullOrEmpty(I)) xmlBuilder.Append(string.Format("I=\"{0}\" ", I));
            xmlBuilder.Append(">");
            xmlBuilder.Append("</FRM>");
            return xmlBuilder.ToString();
        }
    }
}