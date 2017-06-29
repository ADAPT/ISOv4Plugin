using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;

namespace AgGateway.ADAPT.ISOv4Plugin.Writers
{
    public class ProductWriter : BaseWriter
    {
        private ProductWriter(TaskDocumentWriter taskWriter)
            : base(taskWriter, "PDT")
        {
        }

        public static void Write(TaskDocumentWriter taskWriter)
        {
            if (taskWriter.DataModel.Catalog.Products == null)
                return;

            var writer = new ProductWriter(taskWriter);
            writer.WriteProducts(taskWriter.RootWriter);
        }

        private void WriteProducts(XmlWriter writer)
        {
            WriteProducts(writer, TaskWriter.DataModel.Catalog.Products.Where(x => x is CropNutritionProduct || x is CropProtectionProduct));
            WriteProductMixes(writer, TaskWriter.DataModel.Catalog.Products.Where(x => x is MixProduct).Cast<MixProduct>().ToList());
        }

        private void WriteProducts(XmlWriter writer, IEnumerable<Product> products)
        {
            if (products == null)
                return;

            foreach (var product in products)
            {
                var productId = WriteProduct(writer, product);
                TaskWriter.Products[product.Id.ReferenceId] = productId;
            }
        }

        private void WriteProductMixes(XmlWriter writer, List<MixProduct> productMixes)
        {
            if (productMixes == null)
                return;

            foreach (var productMix in productMixes)
            {
                var productId = WriteProductMix(writer, productMix);
                TaskWriter.Products[productMix.Id.ReferenceId] = productId;
            }
        }

        private string WriteProduct(XmlWriter writer, Product product)
        {
            var productId = product.Id.FindIsoId() ?? GenerateId();
            TaskWriter.Ids.Add(productId, product.Id);

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", productId);
            writer.WriteAttributeString("B", product.Description);

            writer.WriteEndElement();

            return productId;
        }

        private string WriteProductMix(XmlWriter writer, MixProduct productMix)
        {
            var productId = productMix.Id.FindIsoId() ?? GenerateId();
            TaskWriter.Ids.Add(productId, productMix.Id);

            writer.WriteStartElement(XmlPrefix);
            writer.WriteAttributeString("A", productId);
            writer.WriteAttributeString("B", productMix.Description);
            writer.WriteAttributeString("F", "2");

            WriteTotalQuantity(writer, productMix.TotalQuantity);
            WriteProductComponents(writer, productMix.ProductComponents);

            writer.WriteEndElement();

            return productId;
        }

        private static void WriteTotalQuantity(XmlWriter writer, NumericRepresentationValue quantity)
        {
            if (quantity == null || quantity.Value == null)
                return;

            writer.WriteXmlAttribute("G", quantity.Value.Value.ToString("F0", CultureInfo.InvariantCulture));
        }

        private void WriteProductComponents(XmlWriter writer, List<ProductComponent> productComponents)
        {
            foreach (var productComponent in productComponents)
            {
                WriteProductComponent(writer, productComponent);
            }
        }

        private void WriteProductComponent(XmlWriter writer, ProductComponent productComponent)
        {
            var productId = TaskWriter.Products.FindById(productComponent.IngredientId);
            if (string.IsNullOrEmpty(productId) ||
                productComponent.Quantity == null ||
                productComponent.Quantity.Value == null)
                return;

            writer.WriteStartElement("PLN");
            writer.WriteAttributeString("A", productId);
            writer.WriteAttributeString("B", productComponent.Quantity.Value.Value.ToString("F0", CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }
    }
}