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
    public class ISOCodedCommentListValue : ISOElement
    {
        //Attributes
        public string CodedCommentListValueId { get; set; }
        public string CodedCommentListValueDesignator { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CCL");
            xmlBuilder.WriteXmlAttribute("A", CodedCommentListValueId);
            xmlBuilder.WriteXmlAttribute("B", CodedCommentListValueDesignator);
            base.WriteXML(xmlBuilder);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOCodedCommentListValue ReadXML(XmlNode commentListValueNode)
        {
            ISOCodedCommentListValue value = new ISOCodedCommentListValue();
            value.CodedCommentListValueId = commentListValueNode.GetXmlNodeValue("@A");
            value.CodedCommentListValueDesignator = commentListValueNode.GetXmlNodeValue("@B");
            return value;
        }

        public static IEnumerable<ISOCodedCommentListValue> ReadXML(XmlNodeList listValueNodes)
        {
            List<ISOCodedCommentListValue> values = new List<ISOCodedCommentListValue>();
            foreach (XmlNode valueNode in listValueNodes)
            {
                values.Add(ISOCodedCommentListValue.ReadXML(valueNode));
            }
            return values;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.CodedCommentListValueId, 14, errors, "A");
            RequireString(this, x => x.CodedCommentListValueDesignator, 32, errors, "B");
            return errors;
        }
    }
}
