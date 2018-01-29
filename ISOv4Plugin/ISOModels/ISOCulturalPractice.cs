/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOCulturalPractice : ISOElement
    {
        public ISOCulturalPractice()
        {
            OperationTechniqueReferences = new List<ISOOperationTechniqueReference>();
        }

        //Attributes
        public string CulturalPracticeID { get; set; }
        public string CulturalPracticeDesignator { get; set; }

        //Child Elements
        public List<ISOOperationTechniqueReference> OperationTechniqueReferences { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CPC");
            xmlBuilder.WriteXmlAttribute("A", CulturalPracticeID);
            xmlBuilder.WriteXmlAttribute("B", CulturalPracticeDesignator);

            foreach (var item in OperationTechniqueReferences)
            {
                item.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOCulturalPractice ReadXML(XmlNode node)
        {
            ISOCulturalPractice item = new ISOCulturalPractice();
            item.CulturalPracticeID = node.GetXmlNodeValue("@A");
            item.CulturalPracticeDesignator = node.GetXmlNodeValue("@B");

            XmlNodeList otrNodes = node.SelectNodes("OTR");
            if (otrNodes != null)
            {
                item.OperationTechniqueReferences.AddRange(ISOOperationTechniqueReference.ReadXML(otrNodes));
            }

            return item;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOCulturalPractice> items = new List<ISOCulturalPractice>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOCulturalPractice.ReadXML(node));
            }
            return items;
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireString(this, x => x.CulturalPracticeID, 14, errors, "A");
            RequireString(this, x => x.CulturalPracticeDesignator, 32, errors, "B");
            OperationTechniqueReferences.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}