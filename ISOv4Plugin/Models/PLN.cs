using System.Linq;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PLN : IWriter
    {
        public PLNA A { get; set; }
        public string B { get; set; }
        public object[] Items { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<PLN ");
            xmlBuilder.Append(string.Format("A=\"{0}\" ", (int)A));
            if (!string.IsNullOrEmpty(B)) xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            xmlBuilder.Append(">");
            if(Items != null)
            {
                foreach (var item in Items.Cast<IWriter>())
                {
                    xmlBuilder.Append(item.WriteXML());
                }
            }
            xmlBuilder.Append("</PLN>");
            return xmlBuilder.ToString();
        }
    }
}
