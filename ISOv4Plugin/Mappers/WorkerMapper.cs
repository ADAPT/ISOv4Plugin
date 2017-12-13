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
    public interface IWorkerMapper
    {
        IEnumerable<ISOWorker> Export(IEnumerable<Person> adaptWorkers);
        IEnumerable<Person> Import(IEnumerable<ISOWorker> isoWorkers);
    }

    public class WorkerMapper : BaseMapper, IWorkerMapper
    {
        public WorkerMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "WKR")
        {
        }

        #region Export
        public IEnumerable<ISOWorker> Export(IEnumerable<Person> adaptWorkers)
        {
            List <ISOWorker> isoWorkers = new List<ISOWorker>();
            foreach (Person person in adaptWorkers)
            {
                ISOWorker isoWorker = Export(person);
                isoWorkers.Add(isoWorker);
            }
            return isoWorkers;
        }

        private ISOWorker Export(Person adaptWorker)
        {
            ISOWorker isoWorker = new ISOWorker();

            //Worker ID
            string workerID = adaptWorker.Id.FindIsoId() ?? GenerateId();
            isoWorker.WorkerId = workerID;
            ExportIDs(adaptWorker.Id, workerID);

            //Worker name
            isoWorker.WorkerFirstName = adaptWorker.FirstName;
            isoWorker.WorkerLastName = adaptWorker.LastName;

            //Worker address
            if (adaptWorker.ContactInfoId.HasValue)
            {
                ContactInfo contactInfo = DataModel.Catalog.ContactInfo.FirstOrDefault(c => c.Id.ReferenceId == adaptWorker.ContactInfoId.Value);
                if (contactInfo != null)
                {
                    isoWorker.WorkerStreet = contactInfo.AddressLine1;
                    isoWorker.WorkerCity = contactInfo.City;
                    isoWorker.WorkerState = contactInfo.StateOrProvince;
                    isoWorker.WorkerPOBox = contactInfo.PoBoxNumber;
                    isoWorker.WorkerPostalCode = contactInfo.PostalCode;
                    isoWorker.WorkerCountry = contactInfo.Country;

                    Contact emailContact = contactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.Email);
                    isoWorker.WorkerEmail = emailContact != null ? emailContact.Number : null;

                    Contact phoneContact = contactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.FixedPhone);
                    isoWorker.WorkerPhone = phoneContact != null ? phoneContact.Number : null;

                    Contact mobileContact = contactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.MobilePhone);
                    isoWorker.WorkerMobile = mobileContact != null ? mobileContact.Number : null;
                }
            }

            //isoWorker.WorkerLicenseNumber = ? //TODO ContextItem

            return isoWorker;
        }
        #endregion Export 

        #region Import

        public IEnumerable<Person> Import(IEnumerable<ISOWorker> isoWorkers)
        {
            List<Person> adaptWorkers = new List<Person>();
            foreach (ISOWorker isoWorker in isoWorkers)
            {
                Person adaptWorker = Import(isoWorker);
                adaptWorkers.Add(adaptWorker);
            }
            return adaptWorkers;
        }

        private Person Import(ISOWorker isoWorker)
        {
            Person worker = new Person();

            //Worker ID
            ImportIDs(worker.Id, isoWorker.WorkerId);

            //Worker name
            worker.LastName = isoWorker.WorkerLastName;
            worker.FirstName = isoWorker.WorkerFirstName;

            //Worker address
            ContactInfo contactInfo = new ContactInfo();
            contactInfo.AddressLine1 = isoWorker.WorkerStreet;
            contactInfo.PoBoxNumber = isoWorker.WorkerPOBox;
            contactInfo.PostalCode = isoWorker.WorkerPostalCode;
            contactInfo.City = isoWorker.WorkerCity;
            contactInfo.StateOrProvince = isoWorker.WorkerState;
            contactInfo.Country = isoWorker.WorkerCountry;

            //Add to Catalog
            DataModel.Catalog.ContactInfo.Add(contactInfo);

            worker.ContactInfoId = contactInfo.Id.ReferenceId;
            
            //? = isoWorker.WorkerLicenseNumber;  //TODO ContextItem

            return worker;
        }

        #endregion Import
    }
}
