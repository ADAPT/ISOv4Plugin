using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TLG : IWriter
    {
        public string A { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<TLG ");
            if(!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            xmlBuilder.Append(">");
            xmlBuilder.Append("</TLG>");
            return xmlBuilder.ToString();

        }
    }
}
