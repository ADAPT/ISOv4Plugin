using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers.LogMappers
{
    [TestFixture]
    public class StatusUpdateMapperTest
    {
        private StatusUpdateMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _mapper = new StatusUpdateMapper();
        }

        [Test]
        public void GivenTskgItem1WhenMapThenStatusIsScheduled()
        {
            var result = _mapper.Map(TSKG.Item1);
            Assert.AreEqual(WorkStatusEnum.Scheduled, result.Status);
        }

        [Test]
        public void GivenTskgItem2WhenMapThenStatusIsInProgress()
        {
            var result = _mapper.Map(TSKG.Item2);
            Assert.AreEqual(WorkStatusEnum.InProgress, result.Status);
        }

        [Test]
        public void GivenTskgItem3WhenMapThenStatusIsPaused()
        {
            var result = _mapper.Map(TSKG.Item3);
            Assert.AreEqual(WorkStatusEnum.Paused, result.Status);
        }

        [Test]
        public void GivenTskgItem4WhenMapThenStatusIsCompleted()
        {
            var result = _mapper.Map(TSKG.Item4);
            Assert.AreEqual(WorkStatusEnum.Completed, result.Status);
        }

        [Test]
        public void GivenTskgItem5WhenMapThenStatusIsScheduled()
        {
            var result = _mapper.Map(TSKG.Item5);
            Assert.AreEqual(WorkStatusEnum.Scheduled, result.Status);
        }
    }
}
