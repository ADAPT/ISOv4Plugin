/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOAllocationStamp : ISOElement
    {
        public ISOAllocationStamp()
        {
            Positions = new List<ISOPosition>();
        }

        //Attributes
        public DateTime? Start { get; set; }  
        public DateTime? Stop { get; set; }
        public uint? Duration { get; set; } //Duration in XML is of unsignedLong type (Max 2^32-1 = 4,294,967,295). Found a possible typo in ISO 11783-10 Table D.2: "2^32-2" or is this correct so maximum value is 2 * int.MaxValue?
        public ISOAllocationStampType Type { get { return (ISOAllocationStampType)TypeInt; } set { TypeInt = (int)value; } }
        private int TypeInt { get; set; }

        //Child Elements
        public List<ISOPosition> Positions { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("ASP");
            xmlBuilder.WriteXmlAttribute("A", Start.HasValue ? Start.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute("B", Stop.HasValue ? Stop.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute<uint>("C", Duration);
            xmlBuilder.WriteXmlAttribute("D", ((int)Type).ToString());
            foreach (ISOPosition item in Positions) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOAllocationStamp ReadXML(XmlNode node)
        {
            if (node == null)
                return null;

            ISOAllocationStamp item = new ISOAllocationStamp();
            item.Start = node.GetXmlNodeValueAsNullableDateTime("@A");
            item.Stop = node.GetXmlNodeValueAsNullableDateTime("@B");
            item.Duration = node.GetXmlNodeValueAsNullableUInt("@C");
            item.TypeInt = node.GetXmlNodeValueAsInt("@D");

            XmlNodeList ptnNodes = node.SelectNodes("PTN");
            if (ptnNodes != null)
            {
                item.Positions.AddRange(ISOPosition.ReadXML(ptnNodes));
            }

            return item;
        }

        public static List<ISOAllocationStamp> ReadXML(XmlNodeList nodes)
        {
            List<ISOAllocationStamp> items = new List<ISOAllocationStamp>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOAllocationStamp.ReadXML(node));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            Require(this, x => x.Start, errors, "A");

            if (Duration.HasValue) ValidateRange<ISOAllocationStamp, uint>(this, x => x.Duration.Value, 0, UInt32.MaxValue - 2, errors, "C");
            ValidateEnumerationValue(typeof(ISOAllocationStampType), TypeInt, errors);
            Positions.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}
