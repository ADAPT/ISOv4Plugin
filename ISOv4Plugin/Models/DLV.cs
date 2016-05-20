using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class DLV : IWriter
    {

        public string A { get; set; }
        public long? B { get; set; }
        public string C { get; set; }
        public ulong? D { get; set; }
        public byte? E { get; set; }
        public byte? F { get; set; }


        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DLV");
            //todo
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
