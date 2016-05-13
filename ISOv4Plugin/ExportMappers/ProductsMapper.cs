using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IProductsMapper
    {
        IEnumerable<PDT> Map(IEnumerable<Product> products);
    }

    public class ProductsMapper : IProductsMapper
    {
        public IEnumerable<PDT> Map(IEnumerable<Product> products)
        {
            int productIndex = 0;
            return products.Select(x => Map(x, productIndex++));
        }

        private PDT Map(Product product, int productIndex)
        {
            var pdt = new PDT
            {
                B = product.Description,
                F = Map(product.ProductType),
            };
            pdt.A = pdt.GetIsoId(productIndex);

            return pdt;
        }

        private PDTF Map(ProductTypeEnum productType)
        {
            return productType != ProductTypeEnum.Mix ? PDTF.Single : PDTF.Mixture;
        }
    }
}