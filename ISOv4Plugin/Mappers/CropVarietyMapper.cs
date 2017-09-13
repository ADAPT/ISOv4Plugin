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
    public interface ICropVarietyMapper
    {
        IEnumerable<ISOCropVariety> ExportCropVarieties(IEnumerable<CropVarietyProduct> adaptCropVarietys);
        ISOCropVariety ExportCropVariety(CropVarietyProduct adaptCropVariety);

        IEnumerable<CropVarietyProduct> ImportCropVarieties(IEnumerable<ISOCropVariety> isoCropVarietys);
        CropVarietyProduct ImportCropVariety(ISOCropVariety isoCropVariety);
    }

    public class CropVarietyMapper : BaseMapper, ICropVarietyMapper
    {
        public CropVarietyMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CVT")
        {
        }

        #region Export
        public IEnumerable<ISOCropVariety> ExportCropVarieties(IEnumerable<CropVarietyProduct> adaptCropVarieties)
        {
            List <ISOCropVariety> varieties = new List<ISOCropVariety>();
            foreach (CropVarietyProduct adaptVariety in adaptCropVarieties)
            {
                ISOCropVariety variety = ExportCropVariety(adaptVariety);
                varieties.Add(variety);
            }
            return varieties;
        }

        public ISOCropVariety ExportCropVariety(CropVarietyProduct adaptCropVariety)
        {
            ISOCropVariety isoVariety = new ISOCropVariety();

            //ID
            string id = adaptCropVariety.Id.FindIsoId() ?? GenerateId();
            isoVariety.CropVarietyId = id;
            ExportUniqueIDs(adaptCropVariety.Id, id);
            TaskDataMapper.ISOIdMap.Add(adaptCropVariety.Id.ReferenceId, id);

            //Designator
            isoVariety.CropVarietyDesignator = adaptCropVariety.Description;

            return isoVariety;
        }

        #endregion Export 

        #region Import

        public IEnumerable<CropVarietyProduct> ImportCropVarieties(IEnumerable<ISOCropVariety> isoCropVarieties)
        {
            //Import crop varieties
            List<CropVarietyProduct> adaptVarieties = new List<CropVarietyProduct>();
            foreach (ISOCropVariety isoCropVariety in isoCropVarieties)
            {
                CropVarietyProduct adaptVariety = ImportCropVariety(isoCropVariety);
                adaptVarieties.Add(adaptVariety);
            }

            //Add the varieties to the Catalog
            if (adaptVarieties.Any())
            { 
                if (DataModel.Catalog.Products == null)
                {
                    DataModel.Catalog.Products = new List<Product>();
                }
                DataModel.Catalog.Products.AddRange(adaptVarieties);
            }

            return adaptVarieties;
        }

        public CropVarietyProduct ImportCropVariety(ISOCropVariety isoCropVariety)
        {
            CropVarietyProduct variety = new CropVarietyProduct();

            //ID
            variety.Id.UniqueIds.AddRange(ImportUniqueIDs(isoCropVariety.CropVarietyId));
            TaskDataMapper.ADAPTIdMap.Add(isoCropVariety.CropVarietyId, variety.Id.ReferenceId);

            //Description
            variety.Description = isoCropVariety.CropVarietyDesignator;

            return variety;
        }
        #endregion Import
    }
}
