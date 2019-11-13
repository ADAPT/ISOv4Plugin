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
    public class ISOCommentAllocation : ISOElement
    {
        //Attributes
        public string CodedCommentIdRef { get; set; }
        public string CodedCommentListValueIdRef { get; set; }
        public string FreeCommentText { get; set; }

        //Child Elements
        public ISOAllocationStamp AllocationStamp { get; set; }


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CAN");
            xmlBuilder.WriteXmlAttribute("A", CodedCommentIdRef);
            xmlBuilder.WriteXmlAttribute("B", CodedCommentListValueIdRef);
            xmlBuilder.WriteXmlAttribute("C", FreeCommentText);
            base.WriteXML(xmlBuilder);
            if (AllocationStamp != null)
            {
                AllocationStamp.WriteXML(xmlBuilder);
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOCommentAllocation ReadXML(XmlNode node)
        {
            ISOCommentAllocation item = new ISOCommentAllocation();
            item.CodedCommentIdRef = node.GetXmlNodeValue("@A");
            item.CodedCommentListValueIdRef = node.GetXmlNodeValue("@B");
            item.FreeCommentText = node.GetXmlNodeValue("@C");
            item.AllocationStamp = ISOAllocationStamp.ReadXML(node.SelectSingleNode("ASP"));
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


        public override List<IError> Validate(List<IError> errors)
        {
            ValidateString(this, x => x.CodedCommentIdRef, 14, errors, "A");
            ValidateString(this, x => x.CodedCommentListValueIdRef, 14, errors, "B");
            ValidateString(this, x => x.FreeCommentText, 32, errors, "C");
            if (AllocationStamp != null) AllocationStamp.Validate(errors);
            return errors;
        }
    }
}
