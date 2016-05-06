using System.Linq;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class LSG : IWriter
    {
        public LSGA A { get; set; }
        public object[] Items { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<LSG ");
            xmlBuilder.Append(string.Format("A=\"{0}\" ", (int)A));
            xmlBuilder.Append(">");
            if(Items != null)
            {
                foreach (var item in Items.Cast<IWriter>())
                {
                    xmlBuilder.Append(item.WriteXML());
                }
            }
            xmlBuilder.Append("</LSG>");
            return xmlBuilder.ToString();
        }
    }
}
