using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AcceptanceTests.Asserts.Import
{
    public class DocumentsAssert
    {
        public static void AreEqual(XmlNode taskData, Documents documents, Catalog catalog, string currentPath)
        {
            var tskNodes = taskData.SelectNodes("TSK");

            LoggedDataAssert.AreEqual(tskNodes, currentPath, documents.LoggedData.ToList(), catalog);
        }
    }
}