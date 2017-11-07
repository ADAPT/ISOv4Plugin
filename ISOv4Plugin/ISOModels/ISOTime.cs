/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOTime : ISOElement
    {
        public ISOTime()
        {
            Positions = new List<ISOPosition>();
            DataLogValues = new List<ISODataLogValue>();
        }

        //Attributes
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public long? Duration { get; set; }
        public ISOTimeType Type { get; set; }

        //Child Elements
        public List<ISOPosition> Positions { get; set; }
        public List<ISODataLogValue> DataLogValues { get; set; }

        public bool HasStart { get; set; }
        public bool HasStop { get; set; }
        public bool HasDuration { get; set; }
        public bool HasType { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TIM");
            xmlBuilder.WriteXmlAttribute("A", Start.HasValue ? Start.Value.ToString("yyyy-MM-ddThh:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute("B", Stop.HasValue ? Stop.Value.ToString("yyyy-MM-ddThh:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute<long>("C", Duration);
            xmlBuilder.WriteXmlAttribute("D", ((int)Type).ToString());

            foreach (ISOPosition item in Positions) { item.WriteXML(xmlBuilder); }
            foreach (ISODataLogValue item in DataLogValues) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOTime ReadXML(XmlNode node)
        {
            ISOTime time = new ISOTime();
            time.Start = node.GetXmlNodeValueAsNullableDateTime("@A");
            time.Stop = node.GetXmlNodeValueAsNullableDateTime("@B");
            time.Duration = node.GetXmlNodeValueAsNullableLong("@C");
            time.Type = (ISOTimeType)(node.GetXmlNodeValueAsInt("@D"));

            time.HasStart = node.IsAttributePresent("A");
            time.HasStop = node.IsAttributePresent("B");
            time.HasDuration = node.IsAttributePresent("C");
            time.HasType = node.IsAttributePresent("D");

            XmlNodeList ptnNodes = node.SelectNodes("PTN");
            if (ptnNodes != null)
            {
                time.Positions.AddRange(ISOPosition.ReadXML(ptnNodes));
            }

            XmlNodeList dlvNodes = node.SelectNodes("DLV");
            if (dlvNodes != null)
            {
                time.DataLogValues.AddRange(ISODataLogValue.ReadXML(dlvNodes));
            }

            return time;
        }

        public static List<ISOTime> ReadXML(XmlNodeList nodes)
        {
            List<ISOTime> items = new List<ISOTime>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOTime.ReadXML(node));
            }
            return items;
        }
    }
}