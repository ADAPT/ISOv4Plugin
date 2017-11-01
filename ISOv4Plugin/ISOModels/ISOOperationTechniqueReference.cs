/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOOperationTechniqueReference : ISOElement
    {
        //Attributes
        public string OperationTechniqueIdRef { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("OTR");
            xmlBuilder.WriteXmlAttribute("A", OperationTechniqueIdRef);
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOOperationTechniqueReference ReadXML(XmlNode node)
        {
            ISOOperationTechniqueReference item = new ISOOperationTechniqueReference();
            item.OperationTechniqueIdRef = node.GetXmlNodeValue("@A");
            return item;
        }

        public static IEnumerable<ISOOperationTechniqueReference> ReadXML(XmlNodeList nodes)
        {
            List<ISOOperationTechniqueReference> items = new List<ISOOperationTechniqueReference>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOOperationTechniqueReference.ReadXML(node));
            }
            return items;
        }
    }
}