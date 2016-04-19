using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class EnumeratedMeterFactoryTest
    {
        private EnumeratedMeterFactory _enumeratedMeterFactory;

        [SetUp]
        public void Setup()
        {
            _enumeratedMeterFactory = new EnumeratedMeterFactory();
        }

        [Test]
        public void GivenCondensedWorkStateDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(161);

            Assert.IsInstanceOf<CondensedWorkStateMeterCreator>(result);
        }

        [Test]
        public void GivenSetpointCondensedWorkStateDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(290);

            Assert.IsInstanceOf<CondensedWorkStateMeterCreator>(result);
        }

        [Test]
        public void GivenCondensedSectionOverrideDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(367);

            Assert.IsInstanceOf<CondensedSectionOverrideStateMeterCreator>(result);
        }

        [Test]
        public void GivenActualWorkStateDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(141);

            Assert.IsInstanceOf<WorkStateMeterCreator>(result);
        }

        [Test]
        public void GivenPrescriptionControlMeterDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(158);

            Assert.IsInstanceOf<PrescriptionControlMeterCreator>(result);
        }

        [Test]
        public void GivenSectionControlStateDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(160);

            Assert.IsInstanceOf<SectionControlStateMeterCreator>(result);
        }

        [Test]
        public void GivenSkyConditionsDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(210);

            Assert.IsInstanceOf<SkyConditionsMeterCreator>(result);
        }

        [Test]
        public void GivenNetWeightStateDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(230);

            Assert.IsInstanceOf<NetWeightStateMeterCreator>(result);
        }

        [Test]
        public void GivenActualLoadingSystemStatusDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(240);

            Assert.IsInstanceOf<ActualLoadingSystemStatusMeterCreator>(result);
        }

        [Test]
        public void GivenConnectorTypeDdiWhenCreateMetersThenMatchingMeterCreator()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(157);

            Assert.IsInstanceOf<ConnectorTypeMeterCreator>(result);
        }

        [Test]
        public void GivenNumericDdiWhenCreateMetersThenNull()
        {
            var result = _enumeratedMeterFactory.GetMeterCreator(100);

            Assert.IsNull(result);
        }
    }
}
