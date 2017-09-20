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
        ISOCropType ExportCropType(Crop adaptCropType);

        IEnumerable<Crop> ImportCropTypes(IEnumerable<ISOCropType> isoCropTypes);
        Crop ImportCropType(ISOCropType isoCropType);
    }

    public class CropTypeMapper : BaseMapper, ICropTypeMapper
    {
        public CropTypeMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CTP")
        {
        }

        #region Export
        public IEnumerable<ISOCropType> ExportCropTypes(IEnumerable<Crop> adaptCropTypes)
        {
            List <ISOCropType> cropTypes = new List<ISOCropType>();
            foreach (Crop group in adaptCropTypes)
            {
                ISOCropType isoGroup = ExportCropType(group);
                cropTypes.Add(isoGroup);
            }
            return cropTypes;
        }

        public ISOCropType ExportCropType(Crop adaptCropType)
        {
            ISOCropType isoCrop = new ISOCropType();

            //ID
            string id = adaptCropType.Id.FindIsoId() ?? GenerateId();
            isoCrop.CropTypeId = id;
            ExportUniqueIDs(adaptCropType.Id, id);
            TaskDataMapper.ISOIdMap.Add(adaptCropType.Id.ReferenceId, id);

            //Designator
            isoCrop.CropTypeDesignator = adaptCropType.Name;

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
            adaptCrop.Id.UniqueIds.AddRange(ImportUniqueIDs(isoCropType.CropTypeId));
            TaskDataMapper.ADAPTIdMap.Add(isoCropType.CropTypeId, adaptCrop.Id.ReferenceId);

            //Description
            adaptCrop.Name = isoCropType.CropTypeDesignator;

            //Varieties
            if (isoCropType.CropVarieties.Any())
            {
                CropVarietyMapper varietyMapper = new CropVarietyMapper(TaskDataMapper);
                varietyMapper.ImportCropVarieties(isoCropType.CropVarieties);
            }

            return adaptCrop;
        }
        #endregion Import
    }
}
