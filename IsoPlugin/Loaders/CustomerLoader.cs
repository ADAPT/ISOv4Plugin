using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class CustomerLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, Grower> _growers;


        private CustomerLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _growers = new Dictionary<string, Grower>();
        }

        internal static Dictionary<string, Grower> Load(TaskDataDocument taskDocument)
        {
            var customerLoader = new CustomerLoader(taskDocument);

            return customerLoader.LoadCustomers();
        }

        private Dictionary<string, Grower> LoadCustomers()
        {
            LoadCustomers(_rootNode.SelectNodes("CTR"));
            ProcessExternalNodes();

            return _growers;
        }

        private void ProcessExternalNodes()
        {
            var xfrNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'CTR')]");
            foreach (XmlNode xfrNode in xfrNodes)
            {
                var inputNodes = xfrNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadCustomers(inputNodes);
            }
        }

        private void LoadCustomers(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string growerId;
                var grower = LoadCustomer(inputNode, out growerId);
                if (grower != null)
                    _growers.Add(growerId, grower);
            }
        }

        private Grower LoadCustomer(XmlNode inputNode, out string growerId)
        {
            Grower grower = new Grower();

            // Required fields. Do not proceed if they are missing
            growerId = inputNode.GetXmlNodeValue("@A");
            grower.Name = inputNode.GetXmlNodeValue("@B");
            if (growerId == null || grower.Name == null)
                return null;

            // Optional fields
            var firstName = inputNode.GetXmlNodeValue("@C");
            if (!string.IsNullOrWhiteSpace(firstName))
                grower.Name += ", " + firstName;

            LoadContactInfo(inputNode, grower);

            _taskDocument.LoadLinkedIds(growerId, grower.Id);

            return grower;
        }

        private void LoadContactInfo(XmlNode inputNode, Grower grower)
        {
            var contactInfo = new ContactInfo();

            contactInfo.AddressLine1 = inputNode.GetXmlNodeValue("@D");
            contactInfo.PoBoxNumber = inputNode.GetXmlNodeValue("@E");
            contactInfo.PostalCode = inputNode.GetXmlNodeValue("@F");
            contactInfo.City = inputNode.GetXmlNodeValue("@G");
            contactInfo.StateOrProvince = inputNode.GetXmlNodeValue("@H");
            contactInfo.Country = inputNode.GetXmlNodeValue("@I");

            LoadContacts(inputNode, contactInfo);

            grower.ContactInfo = contactInfo;

            _taskDocument.Contacts.Add(contactInfo);
        }

        private static void LoadContacts(XmlNode inputNode, ContactInfo contactInfo)
        {
            contactInfo.Contacts = new List<Contact>();

            var phone = inputNode.GetXmlNodeValue("@J");
            if (string.IsNullOrEmpty(phone) == false)
                contactInfo.Contacts.Add(new Contact { Number = phone, Type = ContactTypeEnum.FixedPhone });

            var mobile = inputNode.GetXmlNodeValue("@K");
            if (string.IsNullOrEmpty(mobile) == false)
                contactInfo.Contacts.Add(new Contact { Number = mobile, Type = ContactTypeEnum.MobilePhone });

            var fax = inputNode.GetXmlNodeValue("@L");
            if (string.IsNullOrEmpty(fax) == false)
                contactInfo.Contacts.Add(new Contact { Number = fax, Type = ContactTypeEnum.Fax });

            var email = inputNode.GetXmlNodeValue("@M");
            if (string.IsNullOrEmpty(email) == false)
                contactInfo.Contacts.Add(new Contact { Number = email, Type = ContactTypeEnum.Email });
        }
    }
}
