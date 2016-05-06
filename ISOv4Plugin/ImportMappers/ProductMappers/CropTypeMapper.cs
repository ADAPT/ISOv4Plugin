using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.ProductMappers
{
    public class CropTypeMapper
    {
        public List<Crop> Map(List<CTP> isoCropTypes)
        {
            return isoCropTypes.Select(Map).ToList();
        } 

        private Crop Map(CTP isoCropType)
        {
            var crop = new Crop();
            crop.Id.UniqueIds.Add(new UniqueId
            {
                Id = Guid.NewGuid().ToString(),
                CiTypeEnum = CompoundIdentifierTypeEnum.UUID,
                Source = "http://www.deere.com",
                SourceType = IdSourceTypeEnum.URI
            });
            return crop;
        }
    }
}
