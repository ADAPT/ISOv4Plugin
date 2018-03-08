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
    public class ISOCropType : ISOElement
    {
        public ISOCropType()
        {
            CropVarieties = new List<ISOCropVariety>();
        }

        //Attributes
        public string CropTypeId { get; set; }
        public string CropTypeDesignator { get; set; }
        public string ProductGroupIdRef  { get; set; }

        //Child Elements
        public List<ISOCropVariety> CropVarieties { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("CTP");
            xmlBuilder.WriteXmlAttribute("A", CropTypeId);
            xmlBuilder.WriteXmlAttribute("B", CropTypeDesignator);
            xmlBuilder.WriteXmlAttribute("C", ProductGroupIdRef);

            foreach (var item in CropVarieties)
            {
                item.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOCropType ReadXML(XmlNode typeNode)
        {
            ISOCropType type = new ISOCropType();
            type.CropTypeId = typeNode.GetXmlNodeValue("@A");
            type.CropTypeDesignator = typeNode.GetXmlNodeValue("@B");
            type.ProductGroupIdRef = typeNode.GetXmlNodeValue("@C");

            XmlNodeList cvtNodes = typeNode.SelectNodes("CVT");
            if (cvtNodes != null)
            {
                type.CropVarieties.AddRange(ISOCropVariety.ReadXML(cvtNodes));
            }

            return type;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOCropType> items = new List<ISOCropType>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOCropType.ReadXML(node));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.CropTypeId, 14, errors, "A");
            RequireString(this, x => x.CropTypeDesignator, 32, errors, "B");
            ValidateString(this, x => x.ProductGroupIdRef, 14, errors, "C");
            CropVarieties.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}
