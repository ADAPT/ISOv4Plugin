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
    public class ISOProductAllocation : ISOElement
    {
        //Attributes
        public string ProductIdRef { get; set; }
        public string QuantityDDI { get; set; }
        public long? QuantityValue { get; set; }
        public ISOTransferMode? TransferMode { get; set; }
        public string DeviceElementIdRef { get; set; }
        public string ValuePresentationIdRef { get; set; }

        //Child Elements
        public ISOAllocationStamp AllocationStamp {get; set;}


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PAN");
            xmlBuilder.WriteXmlAttribute("A", ProductIdRef);
            xmlBuilder.WriteXmlAttribute("B", QuantityDDI);
            xmlBuilder.WriteXmlAttribute("C", QuantityValue);
            xmlBuilder.WriteXmlAttribute<ISOTransferMode>("D", TransferMode);
            xmlBuilder.WriteXmlAttribute("E", DeviceElementIdRef);
            xmlBuilder.WriteXmlAttribute("F", ValuePresentationIdRef);
            if (AllocationStamp != null)
            {
                AllocationStamp.WriteXML(xmlBuilder);
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOProductAllocation ReadXML(XmlNode node)
        {
            ISOProductAllocation item = new ISOProductAllocation();
            item.ProductIdRef = node.GetXmlNodeValue("@A");
            item.QuantityDDI = node.GetXmlNodeValue("@B");
            item.QuantityValue = node.GetXmlNodeValueAsNullableLong("@C");
            item.TransferMode = (ISOTransferMode?)(node.GetXmlNodeValueAsNullableInt("@D"));
            item.DeviceElementIdRef = node.GetXmlNodeValue("@E");
            item.ValuePresentationIdRef = node.GetXmlNodeValue("@F");
            item.AllocationStamp = ISOAllocationStamp.ReadXML(node.SelectSingleNode("ASP"));
            return item;
        }

        public static IEnumerable<ISOProductAllocation> ReadXML(XmlNodeList nodes)
        {
            List<ISOProductAllocation> items = new List<ISOProductAllocation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOProductAllocation.ReadXML(node));
            }
            return items;
        }
    }
}