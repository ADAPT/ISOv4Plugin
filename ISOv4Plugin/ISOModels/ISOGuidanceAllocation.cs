/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOGuidanceAllocation : ISOElement
    {
        public ISOGuidanceAllocation()
        {
            AllocationStamps = new List<ISOAllocationStamp>();
            GuidanceShifts = new List<ISOGuidanceShift>();
        }

        //Attributes
        public string GuidanceGroupIdRef { get; set; }

        //Child Elements
        public List<ISOAllocationStamp> AllocationStamps {get; set;}
        public List<ISOGuidanceShift> GuidanceShifts { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GAN");
            xmlBuilder.WriteXmlAttribute("A", GuidanceGroupIdRef);
            foreach (ISOAllocationStamp item in AllocationStamps) { item.WriteXML(xmlBuilder); }
            foreach (ISOGuidanceShift item in GuidanceShifts) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOGuidanceAllocation ReadXML(XmlNode node)
        {
            ISOGuidanceAllocation item = new ISOGuidanceAllocation();
            item.GuidanceGroupIdRef = node.GetXmlNodeValue("@A");

            XmlNodeList aspNodes = node.SelectNodes("ASP");
            if (aspNodes != null)
            {
                item.AllocationStamps.AddRange(ISOAllocationStamp.ReadXML(aspNodes));
            }

            XmlNodeList gstNodes = node.SelectNodes("GST");
            if (gstNodes != null)
            {
                item.GuidanceShifts.AddRange(ISOGuidanceShift.ReadXML(gstNodes));
            }

            return item;
        }

        public static IEnumerable<ISOGuidanceAllocation> ReadXML(XmlNodeList nodes)
        {
            List<ISOGuidanceAllocation> items = new List<ISOGuidanceAllocation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOGuidanceAllocation.ReadXML(node));
            }
            return items;
        }
    }
}