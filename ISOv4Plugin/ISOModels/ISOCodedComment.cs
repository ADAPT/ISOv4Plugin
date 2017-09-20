/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using System;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOCodedComment : ISOElement
    {
        public ISOCodedComment()
        {
            CodedCommentListValues = new List<ISOCodedCommentListValue>();
        }

        //Attributes
        public string CodedCommentID { get; set; }
        public string CodedCommentDesignator { get; set; }
        public ISOCodedCommentScope? CodedCommentScope { get; set; }
        public string CodedCommentGroupIdRef { get; set; }

        //Child Elements
        public List<ISOCodedCommentListValue> CodedCommentListValues { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CCT");
            xmlBuilder.WriteXmlAttribute("A", CodedCommentID);
            xmlBuilder.WriteXmlAttribute("B", CodedCommentDesignator);
            xmlBuilder.WriteXmlAttribute<ISOCodedCommentScope>("C", CodedCommentScope);
            xmlBuilder.WriteXmlAttribute("D", CodedCommentGroupIdRef);

            foreach (ISOCodedCommentListValue item in CodedCommentListValues) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOCodedComment ReadXML(XmlNode commentNode)
        {
            ISOCodedComment comment = new ISOCodedComment();
            comment.CodedCommentID = commentNode.GetXmlNodeValue("@A");
            comment.CodedCommentDesignator = commentNode.GetXmlNodeValue("@B");
            comment.CodedCommentScope = (ISOCodedCommentScope?)(commentNode.GetXmlNodeValueAsNullableInt("@C"));
            comment.CodedCommentGroupIdRef = commentNode.GetXmlNodeValue("@D");

            XmlNodeList cclNodes = commentNode.SelectNodes("CCL");
            if (cclNodes != null)
            {
                comment.CodedCommentListValues.AddRange(ISOCodedCommentListValue.ReadXML(cclNodes));
            }

            return comment;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList commentNodes)
        {
            List<ISOCodedComment> comments = new List<ISOCodedComment>();
            foreach (XmlNode commentNode in commentNodes)
            {
                comments.Add(ISOCodedComment.ReadXML(commentNode));
            }
            return comments;
        }
    }
}