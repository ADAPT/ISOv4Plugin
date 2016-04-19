using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class FarmLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, Farm> _farms;

        private FarmLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _farms = new Dictionary<string, Farm>();
        }

        public static Dictionary<string, Farm> Load(TaskDataDocument taskDocument)
        {
            var farmLoader = new FarmLoader(taskDocument);

            return farmLoader.Load();
        }

        private Dictionary<string, Farm> Load()
        {
            LoadFarms(_rootNode.SelectNodes("FRM"));
            ProcessExternalNodes();

            return _farms;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'FRM')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadFarms(inputNodes);
            }
        }

        private void LoadFarms(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string farmId;
                var farm = LoadFarm(inputNode, out farmId);
                if (farm != null)
                    _farms.Add(farmId, farm);
            }
        }

        private Farm LoadFarm(XmlNode inputNode, out string farmId)
        {
            var farm = new Farm();

            // Required fields. Do not proceed if they are missing
            farmId = inputNode.GetXmlNodeValue("@A");
            farm.Description = inputNode.GetXmlNodeValue("@B");
            if (farmId == null || farm.Description == null)
                return null;

            farm.Id.UniqueIds.Add(ImportHelper.CreateUniqueId(farmId));

            // Optional fields
            LoadContactInfo(inputNode, farm);

            AssignCustomer(inputNode, farm);

            _taskDocument.LoadLinkedIds(farmId, farm.Id);
            return farm;
        }

        private void AssignCustomer(XmlNode inputNode, Farm farm)
        {
            var customerId = inputNode.GetXmlNodeValue("@I");
            if (string.IsNullOrEmpty(customerId) == false)
            {
                var customer = _taskDocument.Customers.FindById(customerId);
                if (customer != null)
                    farm.GrowerId = customer.Id.ReferenceId;
            }
        }

        private void LoadContactInfo(XmlNode inputNode, Farm farm)
        {
            var contactInfo = new ContactInfo();

            contactInfo.AddressLine1 = inputNode.GetXmlNodeValue("@C");
            contactInfo.PoBoxNumber = inputNode.GetXmlNodeValue("@D");
            contactInfo.PostalCode = inputNode.GetXmlNodeValue("@E");
            contactInfo.City = inputNode.GetXmlNodeValue("@F");
            contactInfo.StateOrProvince = inputNode.GetXmlNodeValue("@G");
            contactInfo.Country = inputNode.GetXmlNodeValue("@H");

            farm.ContactInfo = contactInfo;

            _taskDocument.Contacts.Add(contactInfo);
        }
    }
}
