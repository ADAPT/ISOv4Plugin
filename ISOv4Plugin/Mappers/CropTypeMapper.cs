/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Products;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ICropTypeMapper
    {
        IEnumerable<ISOCropType> ExportCropTypes(IEnumerable<Crop> adaptCropTypes);
        ISOCropType ExportCropType(Crop adaptCropType, ISOProductGroup cropTypeProductGroup);

        IEnumerable<Crop> ImportCropTypes(IEnumerable<ISOCropType> isoCropTypes);
        Crop ImportCropType(ISOCropType isoCropType);
    }

    public class CropTypeMapper : BaseMapper, ICropTypeMapper
    {
        public CropTypeMapper(TaskDataMapper taskDataMapper, ProductGroupMapper productGroupMapper) : base(taskDataMapper, "CTP")
        {
            _productGroupMapper = productGroupMapper;
        }

        #region Export
        ProductGroupMapper _productGroupMapper;
        public IEnumerable<ISOCropType> ExportCropTypes(IEnumerable<Crop> adaptCropTypes)
        {
            ISOProductGroup productGroup = _productGroupMapper.ExportProductGroup("CropType", true);
            List <ISOCropType> cropTypes = new List<ISOCropType>();
            foreach (Crop crop in adaptCropTypes)
            {
                ISOCropType isoGroup = ExportCropType(crop, productGroup);
                cropTypes.Add(isoGroup);
            }
            return cropTypes;
        }

        public ISOCropType ExportCropType(Crop adaptCropType, ISOProductGroup cropTypeProductGroup)
        {
            ISOCropType isoCrop = new ISOCropType();

            //ID
            string id = adaptCropType.Id.FindIsoId() ?? GenerateId();
            isoCrop.CropTypeId = id;
            ExportIDs(adaptCropType.Id, id);
            ExportContextItems(adaptCropType.ContextItems, id, "ADAPT_Context_Items:Crop");

            //Designator
            isoCrop.CropTypeDesignator = adaptCropType.Name;

            //Product Group
            isoCrop.ProductGroupIdRef = cropTypeProductGroup.ProductGroupId;

            //Varieties
            if (DataModel.Catalog.Products != null)
            {
                IEnumerable<Product> varietyProducts = DataModel.Catalog.Products.Where(p => p.ProductType == ProductTypeEnum.Variety);
                if (varietyProducts.Any())
                {
                    IEnumerable<CropVarietyProduct> cropVarieties = varietyProducts.Cast<CropVarietyProduct>().Where(v => v.CropId == adaptCropType.Id.ReferenceId);
                    if (cropVarieties.Any())
                    {
                        CropVarietyMapper varietyMapper = new CropVarietyMapper(TaskDataMapper);
                        isoCrop.CropVarieties = varietyMapper.ExportCropVarieties(cropVarieties).ToList();
                    }
                }
            }

            return isoCrop;
        }

        #endregion Export 

        #region Import

        public IEnumerable<Crop> ImportCropTypes(IEnumerable<ISOCropType> isoCropTypes)
        {
            //Import crops
            List<Crop> adaptCrops = new List<Crop>();
            foreach (ISOCropType isoCropType in isoCropTypes)
            {
                Crop adaptCrop = ImportCropType(isoCropType);
                adaptCrops.Add(adaptCrop);
            }

            return adaptCrops;
        }

        public Crop ImportCropType(ISOCropType isoCropType)
        {
            Crop adaptCrop = new Crop();

            //ID
            ImportIDs(adaptCrop.Id, isoCropType.CropTypeId);
            adaptCrop.ContextItems = ImportContextItems(isoCropType.CropTypeId, "ADAPT_Context_Items:Crop", isoCropType);

            //Description
            adaptCrop.Name = isoCropType.CropTypeDesignator;

            //Varieties
            if (isoCropType.CropVarieties.Any())
            {
                CropVarietyMapper varietyMapper = new CropVarietyMapper(TaskDataMapper);
                varietyMapper.ImportCropVarieties(adaptCrop, isoCropType.CropVarieties);
            }

            return adaptCrop;
        }
        #endregion Import
    }
}
