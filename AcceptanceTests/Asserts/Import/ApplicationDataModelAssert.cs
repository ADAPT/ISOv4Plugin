using System.Collections.Generic;
using System.Xml;
using AcceptanceTests.Steps;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AcceptanceTests.Asserts.Import
{
    public class ApplicationDataModelAssert
    {
        public static void AreEqual(XmlNode taskData, ApplicationDataModel applicationDataModel, string currentPath, Dictionary<string, List<UniqueId>> linkList)
        {
            CatalogAssert.AreEqual(taskData, applicationDataModel.Catalog, linkList);
            DocumentsAssert.AreEqual(taskData, applicationDataModel.Documents, applicationDataModel.Catalog, currentPath);
        }
    }
}