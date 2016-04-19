using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IProductsMapper
    {
        IEnumerable<PDT> Map(IEnumerable<Product> products, Dictionary<Guid, string> keyToIsoId);
    }

    public class ProductsMapper : IProductsMapper
    {
        private int _productRelationIndex = 0;

        public IEnumerable<PDT> Map(IEnumerable<Product> products, Dictionary<Guid, string> keyToIsoId)
        {
            int productIndex = 0;
            return products
                        .Select(x => Map(x, keyToIsoId, productIndex++));
        }

        private PDT Map(Product product, Dictionary<Guid, string> keyToIsoId, int productIndex)
        {
            var pdt = new PDT
                {
                    B = product.Description,
                };
            pdt.A = pdt.GetIsoId(productIndex);
            //TODO
            //if (product.ProductGroup != null)
            //{
            //    pdt.C = keyToIsoId[product.ProductGroup.Key];
            //}
            //if (product.ValuePresentation != null)
            //{
            //    pdt.D = keyToIsoId[product.ValuePresentation.Key];
            //}
            //if (product. > 0)
            //{
            //    pdt.E = BitConverter.GetBytes(product.AmountDDI);
            //}

            pdt.F = Map(product.ProductType);

            if (product.ProductType == ProductTypeEnum.Mix)
            {
                //TODO
                //pdt.G = (long) product.MixtureAmount;
                //pdt.Items = product.ProductRelations.Select(Export).ToArray();
            }
            //TODO
            //keyToIsoId.Add(product.Key, pdt.A);
            return pdt;
        }

        //private PRN Export(ProductRelation productRelation)
        //{
        //    var isoProductRelation = new PRN
        //    {
        //        B = (long) productRelation.Amount
        //    };
        //    isoProductRelation.A = isoProductRelation.GetIsoId(_productRelationIndex++);


        //    return isoProductRelation;
        //}

        private PDTF Map(ProductTypeEnum productType)
        {
            return productType != ProductTypeEnum.Mix ? PDTF.Item1 : PDTF.Item2;
        }
    }
}