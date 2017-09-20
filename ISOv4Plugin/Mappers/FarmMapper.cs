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

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IFarmMapper
    {
        IEnumerable<ISOFarm> Export(IEnumerable<Farm> adaptFarms);
        IEnumerable<Farm> Import(IEnumerable<ISOFarm> isoFarms);
    }

    public class FarmMapper : BaseMapper, IFarmMapper
    {
        public FarmMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "FRM")
        {
        }

        #region Export
        public IEnumerable<ISOFarm> Export(IEnumerable<Farm> adaptFarms)
        {
            List <ISOFarm> isoFarms = new List<ISOFarm>();
            foreach (Farm farm in adaptFarms)
            {
                ISOFarm isoFarm = Export(farm);
                isoFarms.Add(isoFarm);
            }
            return isoFarms;
        }

        private ISOFarm Export(Farm adaptFarm)
        {
            ISOFarm isoFarm = new ISOFarm();

            //Farm ID
            string farmID = adaptFarm.Id.FindIsoId() ?? GenerateId();
            isoFarm.FarmId = farmID;
            ExportUniqueIDs(adaptFarm.Id, farmID);
            TaskDataMapper.ISOIdMap.Add(adaptFarm.Id.ReferenceId, farmID);

            //Customer ID
            if (adaptFarm.GrowerId.HasValue)
            { 
                isoFarm.CustomerIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(adaptFarm.GrowerId.Value);
            }

            //Farm name
            isoFarm.FarmDesignator = adaptFarm.Description;

            //Farm address
            if (adaptFarm.ContactInfo != null)
            {
                isoFarm.FarmStreet = adaptFarm.ContactInfo.AddressLine1;
                isoFarm.FarmCity = adaptFarm.ContactInfo.City;
                isoFarm.FarmState = adaptFarm.ContactInfo.StateOrProvince;
                isoFarm.FarmPOBox = adaptFarm.ContactInfo.PoBoxNumber;
                isoFarm.FarmPostalCode = adaptFarm.ContactInfo.PostalCode;
                isoFarm.FarmCountry = adaptFarm.ContactInfo.Country;
            }

            return isoFarm;
        }
        #endregion Export 

        #region Import

        public IEnumerable<Farm> Import(IEnumerable<ISOFarm> isoFarms)
        {
            List<Farm> adaptFarms = new List<Farm>();
            foreach (ISOFarm isoFarm in isoFarms)
            {
                Farm adaptFarm = Import(isoFarm);
                adaptFarms.Add(adaptFarm);
            }
            return adaptFarms;
        }

        private Farm Import(ISOFarm isoFarm)
        {
            Farm farm = new Farm();

            //Farm ID
            farm.Id.UniqueIds.AddRange(ImportUniqueIDs(isoFarm.FarmId));
            TaskDataMapper.ADAPTIdMap.Add(isoFarm.FarmId, farm.Id.ReferenceId);

            //Grower ID
            farm.GrowerId = TaskDataMapper.ADAPTIdMap.FindByISOId(isoFarm.CustomerIdRef);

            //Farm name
            farm.Description = isoFarm.FarmDesignator;

            //Farm address
            farm.ContactInfo = new ContactInfo();
            farm.ContactInfo.AddressLine1 = isoFarm.FarmStreet;
            farm.ContactInfo.PoBoxNumber = isoFarm.FarmPOBox;
            farm.ContactInfo.PostalCode = isoFarm.FarmPostalCode;
            farm.ContactInfo.City = isoFarm.FarmCity;
            farm.ContactInfo.StateOrProvince = isoFarm.FarmState;
            farm.ContactInfo.Country = isoFarm.FarmCountry;

            //Add to Catalog
            DataModel.Catalog.ContactInfo.Add(farm.ContactInfo);

            return farm;
        }

        #endregion Import
    }
}
