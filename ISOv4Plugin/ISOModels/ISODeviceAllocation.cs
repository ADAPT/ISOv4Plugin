/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISODeviceAllocation : ISOElement
    {
        public ISODeviceAllocation()
        {
            AllocationStamps = new List<ISOAllocationStamp>();
        }

        //Attributes
        public string ClientNAMEValue { get; set; }
        public string ClientNAMEMask { get; set; }
        public string DeviceIdRef { get; set; }

        //Child Elements
        public List<ISOAllocationStamp> AllocationStamps {get; set;}


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DAN");
            xmlBuilder.WriteXmlAttribute("A", ClientNAMEValue);
            xmlBuilder.WriteXmlAttribute("B", ClientNAMEMask);
            xmlBuilder.WriteXmlAttribute("C", DeviceIdRef);
            foreach (ISOAllocationStamp item in AllocationStamps) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceAllocation ReadXML(XmlNode node)
        {
            ISODeviceAllocation item = new ISODeviceAllocation();
            item.ClientNAMEValue = node.GetXmlNodeValue("@A");
            item.ClientNAMEMask = node.GetXmlNodeValue("@B");
            item.DeviceIdRef = node.GetXmlNodeValue("@C");
            XmlNodeList aspNodes = node.SelectNodes("ASP");
            if (aspNodes != null)
            {
                item.AllocationStamps.AddRange(ISOAllocationStamp.ReadXML(aspNodes));
            }
            return item;
        }

        public static IEnumerable<ISODeviceAllocation> ReadXML(XmlNodeList nodes)
        {
            List<ISODeviceAllocation> items = new List<ISODeviceAllocation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceAllocation.ReadXML(node));
            }
            return items;
        }
    }
}