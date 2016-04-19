using System.Linq;
using System.Text;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TSK : IWriter
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public TSKG G { get; set; }
        public byte H { get; set; }
        public byte I { get; set; }
        public byte J { get; set; }
        public object[] Items { get; set; }

        public string WriteXML()
        {
            var xmlBuilder = new StringBuilder();
            xmlBuilder.Append("<TSK ");
            if (!string.IsNullOrEmpty(A)) xmlBuilder.Append(string.Format("A=\"{0}\" ", A));
            if (!string.IsNullOrEmpty(B)) xmlBuilder.Append(string.Format("B=\"{0}\" ", B));
            if (!string.IsNullOrEmpty(C)) xmlBuilder.Append(string.Format("C=\"{0}\" ", C));
            if (!string.IsNullOrEmpty(D)) xmlBuilder.Append(string.Format("D=\"{0}\" ", D));
            if (!string.IsNullOrEmpty(E)) xmlBuilder.Append(string.Format("E=\"{0}\" ", E));
            if (!string.IsNullOrEmpty(F)) xmlBuilder.Append(string.Format("F=\"{0}\" ", F));
            xmlBuilder.Append(string.Format("G=\"{0}\" ", (int)G));
            xmlBuilder.Append(string.Format("H=\"{0}\" ", H));
            xmlBuilder.Append(string.Format("I=\"{0}\" ", I));
            xmlBuilder.Append(string.Format("J=\"{0}\" ", J));
            xmlBuilder.Append(">");
            if(Items != null)
            {
                foreach (var item in Items.Cast<IWriter>())
                {
                    xmlBuilder.Append(item.WriteXML());
                }
            }
            xmlBuilder.Append("</TSK>");
            return xmlBuilder.ToString();
        }
    }
}