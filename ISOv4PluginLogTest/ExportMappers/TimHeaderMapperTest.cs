using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class TimHeaderMapperTest
    {
        private List<WorkingData> _meters;
        private Mock<IPtnHeaderMapper> _ptnHeaderMapperMock;
        private Mock<IDlvHeaderMapper> _dlvHeaderMapperMock;
        private TimHeaderMapper _timHeaderMapper;

        [SetUp]
        public void Setup()
        {
            _meters = new List<WorkingData>();

            _ptnHeaderMapperMock = new Mock<IPtnHeaderMapper>();
            _dlvHeaderMapperMock = new Mock<IDlvHeaderMapper>();

            _timHeaderMapper = new TimHeaderMapper(_ptnHeaderMapperMock.Object, _dlvHeaderMapperMock.Object);
           
        }

        [Test]
        public void GivenMetersWhenMapThenStartTimeIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.IsTrue(result.ASpecified);
            Assert.IsNull(result.A);
        }

        [Test]
        public void GivenMetersWhenMapThenStopTimeIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.IsFalse(result.BSpecified);
        }

        [Test]
        public void GivenMetersWhenMapThenDurationIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.IsFalse(result.CSpecified);
        }

        [Test]
        public void GivenMetersWhenMapThenTypeIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.IsTrue(result.DSpecified);
            Assert.AreEqual(TIMD.Item4, result.D);
        }

        [Test]
        public void GivenMetersWhenMapThenPtnHeaderIsMapped()
        {
            var ptn = new PTN();
            _ptnHeaderMapperMock.Setup(x => x.Map()).Returns(ptn);
            
            var result = _timHeaderMapper.Map(_meters);
            var resultPtn = result.Items.First(x => x.GetType() == typeof (PTN));
            Assert.AreEqual(ptn, resultPtn);
        }

        [Test]
        public void GivenMetersWhenMapThenDlvHeadersAreMapped()
        {
            var dlvs = new List<DLV> { new DLV(), new DLV() };
            _dlvHeaderMapperMock.Setup(x => x.Map(_meters)).Returns(dlvs);

            var result = _timHeaderMapper.Map(_meters);

            var resultDlvs = result.Items.Where(x => x.GetType() == typeof (DLV)).Cast<DLV>();
            Assert.AreEqual(dlvs, resultDlvs);
        }
    }
}
