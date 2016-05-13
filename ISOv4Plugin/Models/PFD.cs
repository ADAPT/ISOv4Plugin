using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class PFD : IWriter
    {
        public string A { get; set; }
        public string C { get; set; }
        public ulong? D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string I { get; set; }
        public IWriter[] Items { get; set; }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PFD");
            if (!string.IsNullOrEmpty(A)) 
                xmlBuilder.WriteAttributeString("A", A);
            if (!string.IsNullOrEmpty(C)) 
                xmlBuilder.WriteAttributeString("C", C);
            if(D != null)
                xmlBuilder.WriteAttributeString("D", D.ToString());
            if (!string.IsNullOrEmpty(E)) 
                xmlBuilder.WriteAttributeString("E", E);
            if (!string.IsNullOrEmpty(F)) 
                xmlBuilder.WriteAttributeString("F", F);
            if (!string.IsNullOrEmpty(G)) 
                xmlBuilder.WriteAttributeString("G", G);
            if (!string.IsNullOrEmpty(I)) 
                xmlBuilder.WriteAttributeString("I", I);
            if(Items != null)
            {
                foreach (var item in Items)
                {
                    xmlBuilder = item.WriteXML(xmlBuilder);
                }
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}