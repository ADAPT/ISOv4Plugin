/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;

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
        public uint? Duration { get; set; }
        public ISOTimeType Type { get { return (ISOTimeType)TypeInt; } set { TypeInt = (int)value; } }
        private int TypeInt { get; set; }

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

            //Custom behavior for StartTime to support TimeLog use with an empty A
            string start = Start.HasValue ? Start.Value.ToString("yyyy-MM-ddTHH:mm:ss") : HasStart ? string.Empty : null;
            if (start != null)
            {
                xmlBuilder.WriteAttributeString("A", start);
            }

            xmlBuilder.WriteXmlAttribute("B", Stop.HasValue ? Stop.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "");
            xmlBuilder.WriteXmlAttribute<uint>("C", Duration);
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
            time.Duration = node.GetXmlNodeValueAsNullableUInt("@C");
            time.TypeInt = node.GetXmlNodeValueAsInt("@D");

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

        public override List<IError> Validate(List<IError> errors)
        {
            if (Duration.HasValue) ValidateRange<ISOTime, uint>(this, x => x.Duration.Value, 0, uint.MaxValue - 2, errors, "C");
            ValidateEnumerationValue(typeof(ISOTimeType), TypeInt, errors);
            Positions.ForEach(i => i.Validate(errors));
            DataLogValues.ForEach(i => i.Validate(errors));
            return errors;
        }

        public static ISOTime Merge(ISOTime time, ISOTime otherTime)
        {
            if (time == null)
            {
                return otherTime;
            }
            if (otherTime == null)
            {
                return time;
            }

            ISOTime result = new ISOTime
            {
                // Pick earlier date
                Start = time.Start.Min(otherTime.Start),
                // Pick later date
                Stop = time.Stop.Max(otherTime.Stop),
                // Pick max from both since they most likely overlap
                Duration = time.Duration.Max(otherTime.Duration),
                Type = time.Type == 0 ? otherTime.Type : time.Type,
                HasStart = time.HasStart || otherTime.HasStart,
                HasStop = time.HasStop || otherTime.HasStop,
                HasDuration = time.HasDuration || otherTime.HasDuration,
                HasType = time.HasType || otherTime.HasType
            };

            result.DataLogValues.AddRange(time.DataLogValues);
            result.DataLogValues.AddRange(otherTime.DataLogValues);

            return result;
        }
    }
}
