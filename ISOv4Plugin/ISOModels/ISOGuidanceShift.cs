/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOGuidanceShift : ISOElement
    {
        //Attributes
        public string GuidanceGroupIdRef  { get; set; }
        public string GuidancePatternIdRef  { get; set; }
        public long? GuidanceEastShift  { get; set; }
        public long? GuidanceNorthShift { get; set; }
        public long? PropagationOffset { get; set; }

        //Child Element
        public ISOAllocationStamp AllocationStamp {get; set;}

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GST");
            xmlBuilder.WriteXmlAttribute("A", GuidanceGroupIdRef );
            xmlBuilder.WriteXmlAttribute("B", GuidancePatternIdRef );
            xmlBuilder.WriteXmlAttribute("C", GuidanceEastShift );
            xmlBuilder.WriteXmlAttribute("D", GuidanceNorthShift);
            xmlBuilder.WriteXmlAttribute("E", PropagationOffset);
            if (AllocationStamp != null)
            {
                AllocationStamp.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOGuidanceShift ReadXML(XmlNode node)
        {
            ISOGuidanceShift item = new ISOGuidanceShift();
            item.GuidanceGroupIdRef  = node.GetXmlNodeValue("@A");
            item.GuidancePatternIdRef  = node.GetXmlNodeValue("@B");
            item.GuidanceEastShift  = node.GetXmlNodeValueAsNullableLong("@C");
            item.GuidanceNorthShift = node.GetXmlNodeValueAsNullableLong("@D");
            item.PropagationOffset = node.GetXmlNodeValueAsNullableLong("@E");
            item.AllocationStamp = ISOAllocationStamp.ReadXML(node.SelectSingleNode("ASP"));
            return item;
        }

        public static IEnumerable<ISOGuidanceShift> ReadXML(XmlNodeList nodes)
        {
            List<ISOGuidanceShift> items = new List<ISOGuidanceShift>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOGuidanceShift.ReadXML(node));
            }
            return items;
        }
    }
}