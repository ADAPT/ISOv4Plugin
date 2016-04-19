using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using Moq;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class SectionMapperTest
    {
        [Test]
        public void GivenTimHeaderWhenMapThenGetMetersIsMapped()
        {
            var timHeader = new TIMHeader();
            var isoSpatialRows = new List<ISOSpatialRow>();

            var meterMapperMock = new Mock<IMeterMapper>();

            var meters = new List<Meter>();
            meterMapperMock.Setup(x => x.Map(timHeader, isoSpatialRows, It.IsAny<int>())).Returns(meters);

            var result = new SectionMapper(meterMapperMock.Object).Map(timHeader, isoSpatialRows).First();

            Assert.AreSame(meters, result.GetMeters());
        }
    }
}
