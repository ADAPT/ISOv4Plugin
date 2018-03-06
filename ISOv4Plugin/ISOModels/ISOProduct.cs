/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

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
        public ISOProductType? ProductType { get { return (ISOProductType?)ProductTypeInt; } set { ProductTypeInt = (int?)value; } }
        private int? ProductTypeInt {get; set;}
        public int? MixtureRecipeQuantity { get; set; }
        public int? DensityMassPerVolume { get; set; }
        public int? DensityMassPerCount { get; set; }
        public int? DensityVolumePerCount { get; set; }

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
            xmlBuilder.WriteXmlAttribute<int>("G", MixtureRecipeQuantity);
            xmlBuilder.WriteXmlAttribute<int>("H", DensityMassPerVolume);
            xmlBuilder.WriteXmlAttribute<int>("I", DensityMassPerCount);
            xmlBuilder.WriteXmlAttribute<int>("J", DensityVolumePerCount);
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
            product.ProductTypeInt = pdtNode.GetXmlNodeValueAsNullableInt("@F");
            product.MixtureRecipeQuantity = pdtNode.GetXmlNodeValueAsNullableInt("@G");
            product.DensityMassPerVolume = pdtNode.GetXmlNodeValueAsNullableInt("@H");
            product.DensityMassPerCount = pdtNode.GetXmlNodeValueAsNullableInt("@I");
            product.DensityVolumePerCount = pdtNode.GetXmlNodeValueAsNullableInt("@J");

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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.ProductId, 14, errors, "A");
            RequireString(this, x => x.ProductDesignator, 32, errors, "B");
            ValidateString(this, x => x.ProductGroupRef, 14, errors, "C");
            ValidateString(this, x => x.ValuePresentationIdRef, 14, errors, "D");
            ValidateString(this, x => x.QuantityDDI, 4, errors, "E"); //DDI validation could be improved upon
            if (ProductTypeInt.HasValue) ValidateEnumerationValue(typeof(ISOProductType), ProductTypeInt.Value, errors);
            if (MixtureRecipeQuantity.HasValue) ValidateRange(this, x => x.MixtureRecipeQuantity.Value, 0, Int32.MaxValue - 1, errors, "G");
            if (DensityMassPerVolume.HasValue) ValidateRange(this, x => x.DensityMassPerVolume.Value, 0, Int32.MaxValue - 1, errors, "H");
            if (DensityMassPerCount.HasValue) ValidateRange(this, x => x.DensityMassPerCount.Value, 0, Int32.MaxValue - 1, errors, "I");
            if (DensityVolumePerCount.HasValue) ValidateRange(this, x => x.DensityVolumePerCount.Value, 0, Int32.MaxValue - 1, errors, "J");
            if (ProductRelations.Count > 0) ProductRelations.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}
