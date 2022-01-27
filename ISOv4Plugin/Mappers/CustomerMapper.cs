/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface ICustomerMapper
    {
        IEnumerable<ISOCustomer> Export(IEnumerable<Grower> adaptGrowers);
        IEnumerable<Grower> Import(IEnumerable<ISOCustomer> isoCustomers);
    }

    public class CustomerMapper : BaseMapper, ICustomerMapper
    {
        public CustomerMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CTR")
        {
        }

        #region Export 
        public IEnumerable<ISOCustomer> Export(IEnumerable<Grower> adaptGrowers)
        {
            List<ISOCustomer> customers = new List<ISOCustomer>();
            foreach (Grower grower in adaptGrowers)
            {
                ISOCustomer customer = Export(grower);
                customers.Add(customer);
            }
            return customers;
        }

		private ISOCustomer Export(Grower adaptGrower)
        {
            ISOCustomer customer = new ISOCustomer();

            //Customer ID
            string customerId = adaptGrower.Id.FindIsoId() ?? GenerateId();
            customer.CustomerId = customerId;
            ExportIDs(adaptGrower.Id, customerId);
            ExportContextItems(adaptGrower.ContextItems, customerId, "ADAPT_Context_Items:Grower");

            //Customer Name
            customer.CustomerLastName = adaptGrower.Name;

            //Customer Address
            if (adaptGrower.ContactInfo != null)
            {
                customer.CustomerStreet = adaptGrower.ContactInfo.AddressLine1;
                customer.CustomerCity = adaptGrower.ContactInfo.City;
                customer.CustomerState = adaptGrower.ContactInfo.StateOrProvince;
                customer.CustomerPostalCode = adaptGrower.ContactInfo.PostalCode;
                customer.CustomerPOBox = adaptGrower.ContactInfo.PoBoxNumber;
                customer.CustomerCountry = adaptGrower.ContactInfo.Country;

                //Customer phone/email
                Contact emailContact = adaptGrower.ContactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.Email);
                customer.CustomerEmail = emailContact != null ? emailContact.Number : null;

                Contact phoneContact = adaptGrower.ContactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.FixedPhone);
                customer.CustomerPhone = phoneContact != null ? phoneContact.Number : null;

                Contact faxContact = adaptGrower.ContactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.Fax);
                customer.CustomerFax = faxContact != null ? faxContact.Number : null;

                Contact mobileContact = adaptGrower.ContactInfo.Contacts.FirstOrDefault(c => c.Type == ContactTypeEnum.MobilePhone);
                customer.CustomerMobile = mobileContact != null ? mobileContact.Number : null;
            }

            return customer;
        }
        #endregion Export

        #region Import

        public IEnumerable<Grower> Import(IEnumerable<ISOCustomer> isoCustomers)
        {
            List<Grower> growers = new List<Grower>();
            foreach (ISOCustomer isoCustomer in isoCustomers)
            {
                Grower grower = Import(isoCustomer);
                growers.Add(grower);
            }
            return growers;
        }

        private Grower Import(ISOCustomer isoCustomer)
        {
            Grower grower = new Grower();

            //Customer ID
            ImportIDs(grower.Id, isoCustomer.CustomerId);
            grower.ContextItems = ImportContextItems(isoCustomer.CustomerId, "ADAPT_Context_Items:Grower", isoCustomer);

            //Customer Name
            grower.Name = !string.IsNullOrEmpty(isoCustomer.CustomerFirstName) ? string.Concat(isoCustomer.CustomerFirstName," ", isoCustomer.CustomerLastName) : isoCustomer.CustomerLastName;

            //Customer Address
            grower.ContactInfo = new ContactInfo();
            grower.ContactInfo.AddressLine1 = isoCustomer.CustomerStreet;
            grower.ContactInfo.PoBoxNumber = isoCustomer.CustomerPOBox;
            grower.ContactInfo.PostalCode = isoCustomer.CustomerPostalCode;
            grower.ContactInfo.City = isoCustomer.CustomerCity;
            grower.ContactInfo.StateOrProvince = isoCustomer.CustomerState;
            grower.ContactInfo.Country = isoCustomer.CustomerCountry;

            //Add to Catalog
            DataModel.Catalog.ContactInfo.Add(grower.ContactInfo);

            //Customer Phone/Email
            grower.ContactInfo.Contacts = new List<Contact>();
            if (!string.IsNullOrEmpty(isoCustomer.CustomerEmail))
            {
                grower.ContactInfo.Contacts.Add(new Contact { Number = isoCustomer.CustomerEmail, Type = ContactTypeEnum.Email });
            }

            if (!string.IsNullOrEmpty(isoCustomer.CustomerMobile))
            {
                grower.ContactInfo.Contacts.Add(new Contact { Number = isoCustomer.CustomerMobile, Type = ContactTypeEnum.MobilePhone });
            }

            if (!string.IsNullOrEmpty(isoCustomer.CustomerPhone))
            {
                grower.ContactInfo.Contacts.Add(new Contact { Number = isoCustomer.CustomerPhone, Type = ContactTypeEnum.FixedPhone });
            }

            if (!string.IsNullOrEmpty(isoCustomer.CustomerFax))
            {
                grower.ContactInfo.Contacts.Add(new Contact { Number = isoCustomer.CustomerFax, Type = ContactTypeEnum.Fax });
            }

            return grower;
        }

        #endregion Import
    }
}
