using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IProductGroupsMapper
    {
        //IEnumerable<PGP> Export(IEnumerable<ProductGroup> productGroups, Dictionary<Guid, string> keyToIsoId);
    }

    public class ProductGroupsMapper : IProductGroupsMapper
    {
        //TODO
        //public IEnumerable<PGP> Export(IEnumerable<ProductGroup> productGroups, Dictionary<Guid, string> keyToIsoId)
        //{
        //    int productGroupIndex = 0;
        //    return productGroups.Select(x => Export(x, keyToIsoId, productGroupIndex++));
        //}

        //private PGP Export(ProductGroup productGroup, Dictionary<Guid, string> keyToIsoId, int productGroupIndex)
        //{
        //    var isoProductGroup = new PGP
        //    {
        //        B = productGroup.Name,
        //        C = Export(productGroup.ProductGroupType),
        //    };
        //    isoProductGroup.A = isoProductGroup.GetIsoId(productGroupIndex);

        //    keyToIsoId.Add(productGroup.Key, isoProductGroup.A);
        //    return isoProductGroup;
        //}

        //private PGPC Export(ProductGroupType productGroupType)
        //{
        //    return productGroupType == ProductGroupType.ProductGroup ? PGPC.Item1 : PGPC.Item2;
        //}
    }
}