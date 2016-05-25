using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using Moq;
using NUnit.Framework;
using BinaryReader = AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.BinaryReader;

namespace AcceptanceTests.Asserts.Export
{
    public class TlgAssert
    {
        public static void AreEqual(List<OperationData> operationDatas, List<TLG> tlgs, Catalog catalog, string cardPath)
        {
            foreach (var operationData in operationDatas)
            {
                var matchingTlg = tlgs.SingleOrDefault(x => x.A == operationData.Id.FindIsoId());
                AreEqual(operationData, catalog.TimeScopes, matchingTlg, cardPath);
            }
        }

        private static void AreEqual(OperationData operationData, List<TimeScope> timeScopes, TLG tlg, string cardPath)
        {
            var fileName = tlg.A + ".xml";
            var tlgXmlHeaderFilePath = Path.Combine(cardPath, fileName);
            Assert.IsTrue(File.Exists(tlgXmlHeaderFilePath));

            var tims = new XmlReader().ReadTlgXmlData(cardPath, fileName);
            // TODO:  Assert this.
            //TimAssert.AreEqual(timeScopes,  tims);    

            var sections = operationData.GetAllSections();
            var meters = sections.SelectMany(x => x.GetMeters()).ToList();
            var adaptSpatialRecords = operationData.GetSpatialRecords();
            var binaryReader = new BinaryReader();
            var isoSpatialRecords =  binaryReader.Read(cardPath, tlg.A + ".bin", tims.First());

            Debug.WriteLine("Asserting " + fileName);
            IsoSpatialRecordAssert.AreEqual(adaptSpatialRecords, meters, isoSpatialRecords);
        }
    }
}