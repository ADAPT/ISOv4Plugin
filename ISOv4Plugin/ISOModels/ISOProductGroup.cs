/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOProductGroup : ISOElement
    {
        //Attributes
        public string ProductGroupId { get; set; }
        public string ProductGroupDesignator { get; set; }
        public ISOProductGroupType? ProductGroupType { get { return (ISOProductGroupType?)ProductGroupTypeInt; } set { ProductGroupTypeInt = (int?)value; } }
        private int? ProductGroupTypeInt { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PGP");
            xmlBuilder.WriteXmlAttribute("A", ProductGroupId);
            xmlBuilder.WriteXmlAttribute("B", ProductGroupDesignator);
            xmlBuilder.WriteXmlAttribute("C", (int?)ProductGroupType);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOProductGroup ReadXML(XmlNode pgpNode)
        {
            ISOProductGroup group = new ISOProductGroup();
            group.ProductGroupId = pgpNode.GetXmlNodeValue("@A");
            group.ProductGroupDesignator = pgpNode.GetXmlNodeValue("@B");
            group.ProductGroupTypeInt = pgpNode.GetXmlNodeValueAsNullableInt("@C");
            return group;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOProductGroup> items = new List<ISOProductGroup>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOProductGroup.ReadXML(node));
            }
            return items;
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.ProductGroupId, 14, errors, "A");
            RequireString(this, x => x.ProductGroupDesignator, 32, errors, "B");
            
            return errors;
        }
    }
}