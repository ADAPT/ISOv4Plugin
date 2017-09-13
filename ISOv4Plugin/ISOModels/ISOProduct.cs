/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOProduct : ISOElement
    {
        public ISOProduct()
        {
            ProductRelations = new List<ISOProductRelation>();
        }

        //Attributes
        public string ProductId { get; set; }
        public string ProductDesignator { get; set; }
        public string ProductGroupRef { get; set; }
        public string ValuePresentationIdRef { get; set; }
        public string QuantityDDI { get; set; }
        public ISOProductType? ProductType { get; set; }
        public long? MixtureRecipeQuantity { get; set; }
        public long? DensityMassPerVolume { get; set; }
        public long? DensityMassPerCount { get; set; }
        public long? DensityVolumePerCount { get; set; }

        //Child Elements
        public List<ISOProductRelation> ProductRelations { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PDT");
            xmlBuilder.WriteXmlAttribute("A", ProductId);
            xmlBuilder.WriteXmlAttribute("B", ProductDesignator);
            xmlBuilder.WriteXmlAttribute("C", ProductGroupRef);
            xmlBuilder.WriteXmlAttribute("D", ValuePresentationIdRef);
            xmlBuilder.WriteXmlAttribute("E", QuantityDDI);
            xmlBuilder.WriteXmlAttribute("F", ((int)ProductType).ToString());
            xmlBuilder.WriteXmlAttribute<long>("G", MixtureRecipeQuantity);
            xmlBuilder.WriteXmlAttribute<long>("H", DensityMassPerVolume);
            xmlBuilder.WriteXmlAttribute<long>("I", DensityMassPerCount);
            xmlBuilder.WriteXmlAttribute<long>("J", DensityVolumePerCount);
            foreach (ISOProductRelation item in ProductRelations) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOProduct ReadXML(XmlNode pdtNode)
        {
            ISOProduct product = new ISOProduct();
            product.ProductId = pdtNode.GetXmlNodeValue("@A");
            product.ProductDesignator = pdtNode.GetXmlNodeValue("@B");
            product.ProductGroupRef = pdtNode.GetXmlNodeValue("@C");
            product.ValuePresentationIdRef = pdtNode.GetXmlNodeValue("@D");
            product.QuantityDDI = pdtNode.GetXmlNodeValue("@E");
            product.ProductType = (ISOProductType?)pdtNode.GetXmlNodeValueAsNullableInt("@F");
            product.MixtureRecipeQuantity = pdtNode.GetXmlNodeValueAsNullableLong("@G");
            product.DensityMassPerVolume = pdtNode.GetXmlNodeValueAsNullableLong("@H");
            product.DensityMassPerCount = pdtNode.GetXmlNodeValueAsNullableLong("@I");
            product.DensityVolumePerCount = pdtNode.GetXmlNodeValueAsNullableLong("@J");

            XmlNodeList prnNodes = pdtNode.SelectNodes("PRN");
            if (prnNodes != null)
            {
                product.ProductRelations.AddRange(ISOProductRelation.ReadXML(prnNodes));
            }

            return product;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList pdtNodes)
        {
            List<ISOProduct> products = new List<ISOProduct>();
            foreach (XmlNode pdtNode in pdtNodes)
            {
                products.Add(ISOProduct.ReadXML(pdtNode));
            }
            return products;
        }
    }
}