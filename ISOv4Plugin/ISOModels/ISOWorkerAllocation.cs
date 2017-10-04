/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOWorkerAllocation : ISOElement
    {
        //Attributes
        public string WorkerIdRef { get; set; }

        //Child Elements
        public ISOAllocationStamp AllocationStamp { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("WAN");
            xmlBuilder.WriteXmlAttribute("A", WorkerIdRef);
            if (AllocationStamp != null)
            {
                AllocationStamp.WriteXML(xmlBuilder);
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOWorkerAllocation ReadXML(XmlNode node)
        {
            ISOWorkerAllocation item = new ISOWorkerAllocation();
            item.WorkerIdRef = node.GetXmlNodeValue("@A");
            item.AllocationStamp = ISOAllocationStamp.ReadXML(node.SelectSingleNode("ASP"));
            return item;
        }

        public static IEnumerable<ISOWorkerAllocation> ReadXML(XmlNodeList nodes)
        {
            List<ISOWorkerAllocation> items = new List<ISOWorkerAllocation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOWorkerAllocation.ReadXML(node));
            }
            return items;
        }
    }
}