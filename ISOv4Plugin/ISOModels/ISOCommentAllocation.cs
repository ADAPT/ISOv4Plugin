/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOCommentAllocation : ISOElement
    {
        public ISOCommentAllocation()
        {
            AllocationStamps = new List<ISOAllocationStamp>();
        }

        //Attributes
        public string CodedCommentIdRef { get; set; }
        public string CodedCommentListValueIdRef { get; set; }
        public string FreeCommentText { get; set; }

        //Child Elements
        public List<ISOAllocationStamp> AllocationStamps {get; set;}


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CAN");
            xmlBuilder.WriteXmlAttribute("A", CodedCommentIdRef);
            xmlBuilder.WriteXmlAttribute("B", CodedCommentListValueIdRef);
            xmlBuilder.WriteXmlAttribute("C", FreeCommentText);
            foreach (ISOAllocationStamp item in AllocationStamps) { item.WriteXML(xmlBuilder); }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOCommentAllocation ReadXML(XmlNode node)
        {
            ISOCommentAllocation item = new ISOCommentAllocation();
            item.CodedCommentIdRef = node.GetXmlNodeValue("@A");
            item.CodedCommentListValueIdRef = node.GetXmlNodeValue("@B");
            item.FreeCommentText = node.GetXmlNodeValue("@C");
            
            XmlNodeList aspNodes = node.SelectNodes("ASP");
            if (aspNodes != null)
            {
                item.AllocationStamps.AddRange(ISOAllocationStamp.ReadXML(aspNodes));
            }
            return item;
        }

        public static IEnumerable<ISOCommentAllocation> ReadXML(XmlNodeList nodes)
        {
            List<ISOCommentAllocation> items = new List<ISOCommentAllocation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOCommentAllocation.ReadXML(node));
            }
            return items;
        }
    }
}