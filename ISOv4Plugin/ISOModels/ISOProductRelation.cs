/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOProductRelation : ISOElement
    {
        //Attributes
        public string ProductIdRef { get; set; }
        public int QuantityValue { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PRN");
            xmlBuilder.WriteXmlAttribute("A", ProductIdRef);
            xmlBuilder.WriteXmlAttribute<int>("B", QuantityValue);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOProductRelation ReadXML(XmlNode node)
        {
            ISOProductRelation item = new ISOProductRelation();
            item.ProductIdRef = node.GetXmlNodeValue("@A");
            item.QuantityValue = node.GetXmlNodeValueAsInt("@B");
            return item;
        }

        public static IEnumerable<ISOProductRelation> ReadXML(XmlNodeList nodes)
        {
            List<ISOProductRelation> items = new List<ISOProductRelation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOProductRelation.ReadXML(node));
            }
            return items;
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.ProductIdRef, 14, errors, "A");
            RequireRange(this, x => x.QuantityValue, 0, Int32.MaxValue - 1, errors, "B");
            return errors;
        }
    }
}