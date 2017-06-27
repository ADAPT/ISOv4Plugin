using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface ICropTypeMapper
    {
        IEnumerable<CTP> Map(List<Crop> crops, Dictionary<int, string> keyToIsoId, Catalog setupCatalog);
    }

    public class CropTypeMapper : ICropTypeMapper
    {
        private int _cropVarietyIndex = 0;

        public IEnumerable<CTP> Map(List<Crop> crops, Dictionary<int, string> keyToIsoId, Catalog setupCatalog)
        {
            if(crops == null)
                return null;

            int cropIndex = 0;
            return crops.Select(x => Map(x, keyToIsoId, cropIndex++, setupCatalog));
        }

        private CTP Map(Crop crop, Dictionary<int, string> keyToIsoId, int cropIndex, Catalog setupCatalog)
        {
            var ctp = new CTP
                {
                    B = crop.Name
                };
            ctp.A = ctp.GetIsoId(cropIndex);

            var cvts = setupCatalog.Products.Where(x => x is CropVarietyProduct).Cast<CropVarietyProduct>().Where(v => v.CropId == crop.Id.ReferenceId).Select(Map);

            var cvtArray = cvts as CVT[] ?? cvts.ToArray();
            if (cvtArray.Any())
            {
                ctp.Items = cvtArray.ToArray();
            }

            keyToIsoId.Add(crop.Id.ReferenceId, ctp.A);
            return ctp;
        }

        private CVT Map(CropVarietyProduct cropVariety)
        {
            var isoVariety = new CVT
            {
                B = cropVariety.Description,
            };
            isoVariety.A = isoVariety.GetIsoId(_cropVarietyIndex++);
            return isoVariety;
        }
    }
}