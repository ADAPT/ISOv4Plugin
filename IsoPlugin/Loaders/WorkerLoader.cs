using AgGateway.ADAPT.ApplicationDataModel;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class WorkerLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, Person> _workers;

        private WorkerLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _workers = new Dictionary<string, Person>();
        }

        internal static Dictionary<string, Person> Load(TaskDataDocument taskDocument)
        {
            var loader = new WorkerLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, Person> Load()
        {
            LoadWorkers(_rootNode.SelectNodes("WKR"));
            ProcessExternalNodes();

            return _workers;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'WKR')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadWorkers(inputNodes);
            }
        }

        private void LoadWorkers(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string workerId;
                var worker = LoadWorker(inputNode, out workerId);
                if (worker != null)
                    _workers.Add(workerId, worker);
            }
        }

        private Person LoadWorker(XmlNode inputNode, out string workerId)
        {
            var worker = new Person();

            // Required fields. Do not proceed if they are missing
            workerId = inputNode.GetXmlNodeValue("@A");
            worker.LastName = inputNode.GetXmlNodeValue("@B");
            if (workerId == null || worker.LastName == null)
                return null;

            // Optional fields
            worker.FirstName = inputNode.GetXmlNodeValue("@C");
            LoadContactInfo(inputNode, worker);

            _taskDocument.LoadLinkedIds(workerId, worker.Id);
            return worker;
        }

        private void LoadContactInfo(XmlNode inputNode, Person worker)
        {
            var contactInfo = new ContactInfo();

            contactInfo.AddressLine1 = inputNode.GetXmlNodeValue("@D");
            contactInfo.PoBoxNumber = inputNode.GetXmlNodeValue("@E");
            contactInfo.PostalCode = inputNode.GetXmlNodeValue("@F");
            contactInfo.City = inputNode.GetXmlNodeValue("@G");
            contactInfo.StateOrProvince = inputNode.GetXmlNodeValue("@H");
            contactInfo.Country = inputNode.GetXmlNodeValue("@I");

            LoadPhoneNumbers(inputNode, contactInfo);

            //worker.ContactInfo = contactInfo;

            _taskDocument.Contacts.Add(contactInfo);
        }

        private static void LoadPhoneNumbers(XmlNode inputNode, ContactInfo contactInfo)
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
