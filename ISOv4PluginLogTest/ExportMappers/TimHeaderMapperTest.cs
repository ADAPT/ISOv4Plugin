using System.Collections.Generic;
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
        private List<Meter> _meters;
        private Mock<IPtnHeaderMapper> _ptnHeaderMapperMock;
        private Mock<IDlvHeaderMapper> _dlvHeaderMapperMock;
        private TimHeaderMapper _timHeaderMapper;

        [SetUp]
        public void Setup()
        {
            _meters = new List<Meter>();

            _ptnHeaderMapperMock = new Mock<IPtnHeaderMapper>();
            _dlvHeaderMapperMock = new Mock<IDlvHeaderMapper>();

            _timHeaderMapper = new TimHeaderMapper(_ptnHeaderMapperMock.Object, _dlvHeaderMapperMock.Object);
           
        }

        [Test]
        public void GivenMetersWhenMapThenStartTimeIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.AreEqual(HeaderPropertyState.IsEmpty, result.Start.State);
        }

        [Test]
        public void GivenMetersWhenMapThenStopTimeIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.AreEqual(HeaderPropertyState.IsNull, result.Stop.State);
        }

        [Test]
        public void GivenMetersWhenMapThenDurationIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.AreEqual(HeaderPropertyState.IsNull, result.Duration.State);
        }

        [Test]
        public void GivenMetersWhenMapThenTypeIsMapped()
        {
            var result = _timHeaderMapper.Map(_meters);

            Assert.AreEqual(HeaderPropertyState.HasValue, result.Type.State);
            Assert.AreEqual((int)TIMD.Item4, result.Type.Value);
        }

        [Test]
        public void GivenMetersWhenMapThenPtnHeaderIsMapped()
        {
            var ptnHeader = new PTNHeader();
            _ptnHeaderMapperMock.Setup(x => x.Map()).Returns(ptnHeader);
            
            var result = _timHeaderMapper.Map(_meters);

            Assert.AreEqual(ptnHeader, result.PtnHeader);
        }

        [Test]
        public void GivenMetersWhenMapThenDlvHeadersAreMapped()
        {
            var dlvHeaders = new List<DLVHeader>{ new DLVHeader(), new DLVHeader()};
            _dlvHeaderMapperMock.Setup(x => x.Map(_meters)).Returns(dlvHeaders);

            var result = _timHeaderMapper.Map(_meters);

            Assert.AreEqual(dlvHeaders, result.DLVs);
        }
    }
}
