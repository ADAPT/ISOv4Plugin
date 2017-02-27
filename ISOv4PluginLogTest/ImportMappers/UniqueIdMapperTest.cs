using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers
{
    [TestFixture]
    public class UniqueIdMapperTest
    {
        private string _id;
        private UniqueIdMapper _uniqueIdMapper;

        [SetUp]
        public void Setup()
        {
            _uniqueIdMapper = new UniqueIdMapper();
        }

        [Test]
        public void GivenIdWhenMapThenIdIsMapped()
        {
            _id = "DLV1";

            var uniqueId = Map();
            Assert.AreEqual(_id, uniqueId.Id);
        }

        [Test]
        public void GivenIdWhenMapThenSourceIsMapped()
        {
            _id = "DLV1";

            var uniqueId = Map();

            const string expectedSource = "http://dictionary.isobus.net/isobus/";
            Assert.AreEqual(expectedSource, uniqueId.Source);
        }

        [Test]
        public void GivenIdWhenMapThenIdTypeIsMapped()
        {
            _id = "DLV1";

            var uniqueId = Map();

            const IdTypeEnum expected = IdTypeEnum.String;
            Assert.AreEqual(expected, uniqueId.IdType);
        }

        [Test]
        public void GivenIdWhenMapThenSourceTypeIsMapped()
        {
            _id = "DLV1";

            var uniqueId = Map();

            const IdSourceTypeEnum expected = IdSourceTypeEnum.URI;
            Assert.AreEqual(expected, uniqueId.SourceType);
        }

        private UniqueId Map()
        {
            return _uniqueIdMapper.Map(_id);
        }
    }
}
