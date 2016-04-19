using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class CTP : IWriter
    {
        public string B { get; set; }
        public string A { get; set; }
        public CVT[] Items { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<CTP ");
            if (!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            if (!string.IsNullOrEmpty(B)) xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            xmlBuilder.Append(">"); 
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    xmlBuilder.Append(item.WriteXML());
                }
            }
            xmlBuilder.Append("</CTP>");
            return xmlBuilder.ToString();
        }
    }
}