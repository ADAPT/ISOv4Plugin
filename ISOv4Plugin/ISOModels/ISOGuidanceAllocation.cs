/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOGuidanceAllocation : ISOElement
    {
        public ISOGuidanceAllocation()
        {
            GuidanceShifts = new List<ISOGuidanceShift>();
        }

        //Attributes
        public string GuidanceGroupIdRef { get; set; }

        //Child Elements
        public ISOAllocationStamp AllocationStamp { get; set; }
        public List<ISOGuidanceShift> GuidanceShifts { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("GAN");
            xmlBuilder.WriteXmlAttribute("A", GuidanceGroupIdRef);
            base.WriteXML(xmlBuilder);
            if (AllocationStamp != null)
            {
                AllocationStamp.WriteXML(xmlBuilder);
            }
            foreach (ISOGuidanceShift item in GuidanceShifts) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOGuidanceAllocation ReadXML(XmlNode node)
        {
            ISOGuidanceAllocation item = new ISOGuidanceAllocation();
            item.GuidanceGroupIdRef = node.GetXmlNodeValue("@A");
            item.AllocationStamp = ISOAllocationStamp.ReadXML(node.SelectSingleNode("ASP"));

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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.GuidanceGroupIdRef, 14, errors, "A");
            if (RequireChildElement(AllocationStamp, "ASP", errors)) AllocationStamp.Validate(errors);
            GuidanceShifts.ForEach(i => i.Validate(errors));

            return errors;
        }
    }
}
