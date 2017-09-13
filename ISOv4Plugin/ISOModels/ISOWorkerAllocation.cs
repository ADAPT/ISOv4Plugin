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
        public ISOWorkerAllocation()
        {
            AllocationStamps = new List<ISOAllocationStamp>();
        }

        //Attributes
        public string WorkerIdRef { get; set; }

        //Child Elements
        public List<ISOAllocationStamp> AllocationStamps {get; set;}
        
        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("WAN");
            xmlBuilder.WriteXmlAttribute("A", WorkerIdRef);
            foreach (ISOAllocationStamp item in AllocationStamps) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOWorkerAllocation ReadXML(XmlNode node)
        {
            ISOWorkerAllocation item = new ISOWorkerAllocation();
            item.WorkerIdRef = node.GetXmlNodeValue("@A");
            XmlNodeList aspNodes = node.SelectNodes("ASP");
            if (aspNodes != null)
            {
                item.AllocationStamps.AddRange(ISOAllocationStamp.ReadXML(aspNodes));
            }
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