using System.Linq;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PFD : IWriter
    {
        public string A { get; set; }
        public string C { get; set; }
        public ulong D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string I { get; set; }
        public object[] Items { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<PFD ");
            if (!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            if (!string.IsNullOrEmpty(C)) xmlBuilder.Append(string.Format("C=\"{0}\" ", C));
            xmlBuilder.Append(string.Format("D=\"{0}\" ", D));
            if (!string.IsNullOrEmpty(E)) xmlBuilder.Append(string.Format("E=\"{0}\" ", E));
            if (!string.IsNullOrEmpty(F)) xmlBuilder.Append(string.Format("F=\"{0}\" ", F));
            if (!string.IsNullOrEmpty(G)) xmlBuilder.Append(string.Format("G=\"{0}\" ", G));
            if (!string.IsNullOrEmpty(I)) xmlBuilder.Append(string.Format("I=\"{0}\" ", I));
            xmlBuilder.Append(">");
            if(Items != null)
            {
                foreach (var item in Items.Cast<IWriter>())
                {
                    xmlBuilder.Append(item.WriteXML());
                }
            }
            xmlBuilder.Append("</PFD>");
            return xmlBuilder.ToString();
        }
    }
}