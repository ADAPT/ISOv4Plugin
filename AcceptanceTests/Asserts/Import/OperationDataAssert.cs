using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;
using XmlReader = AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders.XmlReader;

namespace AcceptanceTests.Asserts.Import
{
    public class OperationDataAssert
    {
        public static void AreEqual(XmlNodeList tlgNodes, string currentPath, List<OperationData> operationData)
        {
            Assert.AreEqual(tlgNodes.Count, operationData.Count);
            for (int i = 0; i < tlgNodes.Count; i++)
            {
                var matchingOperationData = operationData.SingleOrDefault(x => x.Id.FindIsoId() == tlgNodes[i].Attributes["A"].Value);
                AreEqual(tlgNodes[i], currentPath, matchingOperationData);
            }
        }

        private static void AreEqual(XmlNode tlgNode, string currentPath, OperationData operationData)
        {
            var isoSpatialRecords = GetIsoSpatialRecords(tlgNode.Attributes["A"].Value, currentPath).ToList();
            var adaptSpatialRecords = operationData.GetSpatialRecords().ToList();

            var sections = operationData.GetAllSections();
            var meters = sections.SelectMany(x => x.GetMeters()).ToList();

            SpatialRecordAssert.AreEqual(isoSpatialRecords, adaptSpatialRecords, meters);
        }

        private static IEnumerable<ISOSpatialRow> GetIsoSpatialRecords(string tlgA, string currentPath)
        {
            var xmlReader = new XmlReader();
            var timHeader = xmlReader.ReadTlgXmlData(currentPath, tlgA + ".xml");
            var binaryReader = new BinaryReader();
            return binaryReader.Read(currentPath, tlgA + ".bin", timHeader.First()).ToList();
        }
    }
}