using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CVT : IWriter 
    {
        public string B { get; set; }
        public string A { get; set; }
        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<CVT ");
            if (!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            if (!string.IsNullOrEmpty(B)) xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            xmlBuilder.Append(">");
            xmlBuilder.Append("</CVT>");

            return xmlBuilder.ToString();
        }
    }
}
