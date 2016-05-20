using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
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
            var tim = new TIM();
            var tims = new List<TIM> {tim};
            var isoSpatialRows = new List<ISOSpatialRow>();

            var meterMapperMock = new Mock<IMeterMapper>();

            var meters = new List<Meter>();
            meterMapperMock.Setup(x => x.Map(tim, isoSpatialRows, It.IsAny<int>())).Returns(meters);

            var result = new SectionMapper(meterMapperMock.Object).Map(tims, isoSpatialRows).First();

            Assert.AreSame(meters, result.GetMeters());
        }

        [Test]
        public void GivenTwoTimHeadersWhenMapThenGetMetersIsMapped()
        {
            var tim1 = new TIM();
            var tim2 = new TIM();
            var tims = new List<TIM> { tim1, tim2 };
            var isoSpatialRows = new List<ISOSpatialRow>();

            var meterMapperMock = new Mock<IMeterMapper>();

            var meters1 = new List<Meter>();
            var meters2 = new List<Meter>();
            meterMapperMock.Setup(x => x.Map(tim1, isoSpatialRows, It.IsAny<int>())).Returns(meters1);
            meterMapperMock.Setup(x => x.Map(tim2, isoSpatialRows, It.IsAny<int>())).Returns(meters2);

            var result = new SectionMapper(meterMapperMock.Object).Map(tims, isoSpatialRows);

            Assert.AreEqual(2, result.Count);
            Assert.AreSame(meters1, result.ElementAt(0).GetMeters());
            Assert.AreSame(meters2, result.ElementAt(1).GetMeters());
        }
    }
}
