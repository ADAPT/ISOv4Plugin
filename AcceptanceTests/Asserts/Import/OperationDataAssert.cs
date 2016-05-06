using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using NUnit.Framework;

namespace AcceptanceTests.Asserts.Import
{
    public class OperationDataAssert
    {
        public static void AreEqual(List<TLG> tlgs, string currentPath, List<OperationData> operationData)
        {
            Assert.AreEqual(tlgs.Count, operationData.Count);
            foreach (var tlg in tlgs)
            {
                var matchingOperationData = operationData.SingleOrDefault(x => x.Id.FindIsoId() == tlg.A);
                AreEqual(tlg, currentPath, matchingOperationData);
            }
        }

        public static void AreEqual(TLG tlg, string currentPath, OperationData operationData)
        {
            var isoSpatialRecords = GetIsoSpatialRecords(tlg.A, currentPath).ToList();
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
            return binaryReader.Read(currentPath, tlgA + ".bin", timHeader).ToList();
        }
    }
}