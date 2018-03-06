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
    public class ISOOperationTechnique : ISOElement
    {
        //Attributes
        public string OperationTechniqueId { get; set; }
        public string OperationTechniqueDesignator { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("OTQ");
            xmlBuilder.WriteXmlAttribute("A", OperationTechniqueId);
            xmlBuilder.WriteXmlAttribute("B", OperationTechniqueDesignator);

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOOperationTechnique ReadXML(XmlNode node)
        {
            ISOOperationTechnique item = new ISOOperationTechnique();
            item.OperationTechniqueId = node.GetXmlNodeValue("@A");
            item.OperationTechniqueDesignator = node.GetXmlNodeValue("@B");

            return item;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOOperationTechnique> items = new List<ISOOperationTechnique>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOOperationTechnique.ReadXML(node));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.OperationTechniqueId, 14, errors, "A");
            RequireString(this, x => x.OperationTechniqueDesignator, 32, errors, "B");
            return errors;
        }
    }
}
