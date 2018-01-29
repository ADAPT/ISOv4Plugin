/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOGuidanceShift : ISOElement
    {
        //Attributes
        public string GuidanceGroupIdRef  { get; set; }
        public string GuidancePatternIdRef  { get; set; }
        public int? GuidanceEastShift  { get; set; }
        public int? GuidanceNorthShift { get; set; }
        public int? PropagationOffset { get; set; }

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
            item.GuidanceEastShift  = node.GetXmlNodeValueAsNullableInt("@C");
            item.GuidanceNorthShift = node.GetXmlNodeValueAsNullableInt("@D");
            item.PropagationOffset = node.GetXmlNodeValueAsNullableInt("@E");
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

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.GuidanceGroupIdRef, 14, errors, "A");
            RequireString(this, x => x.GuidancePatternIdRef, 14, errors, "B");
            if (GuidanceEastShift.HasValue) ValidateRange(this, x => x.GuidanceEastShift.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "C");
            if (GuidanceNorthShift.HasValue) ValidateRange(this, x => x.GuidanceNorthShift.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "D");
            if (PropagationOffset.HasValue) ValidateRange(this, x => x.PropagationOffset.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "E");
            if (AllocationStamp != null) AllocationStamp.Validate(errors);
            return errors;
        }
    }
}