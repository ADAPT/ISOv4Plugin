using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TIM : IWriter
    {
        public DateTime? A { get; set; }
        public bool ASpecified { get; set; }
        public DateTime? B { get; set; }
        public bool BSpecified { get; set; }
        public ulong? C { get; set; }
        public bool CSpecified { get; set; }
        public TIMD? D { get; set; }
        public bool DSpecified { get; set; }
        public IWriter[] Items { get; set; }

        public TIM()
        {
            Items = new IWriter[0];
        }

        public XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TIM");

            if (ASpecified)
            {
                xmlBuilder.WriteAttributeString("A", A.HasValue ? A.Value.ToString("yyyy-MM-ddThh:mm:ss") : "");
            }

            if (BSpecified)
            {
                xmlBuilder.WriteAttributeString("B", B.HasValue ? B.Value.ToString("yyyy-MM-ddThh:mm:ss") : "");
            }

            if (DSpecified)
            {
                xmlBuilder.WriteAttributeString("D", D.HasValue ? ((int) D.Value).ToString(CultureInfo.InvariantCulture) : "");
            }

            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }
    }
}
