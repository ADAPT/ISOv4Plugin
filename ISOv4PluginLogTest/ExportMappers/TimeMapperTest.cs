using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ExportMappers
{
    [TestFixture]
    public class TimeMapperTest
    {
        private TimeScope _timeScope;
        private List<TimeScope> _timeScopes; 
        private TimeMapper _timeMapper;

        [SetUp]
        public void Setup()
        {
            _timeScope = new TimeScope();
            _timeScopes = new List<TimeScope>{ _timeScope };

            _timeMapper = new TimeMapper();
        }

        [Test]
        public void GivenTimeScopesWhenMapThenTimsAreMapped()
        {
            _timeScopes.Add(new TimeScope());
            _timeScopes.Add(new TimeScope());
            _timeScopes.Add(new TimeScope());

            var result = Map();
            Assert.AreEqual(_timeScopes.Count, result.Count());
        }

        [Test]
        public void GivenTimeScopeWIthStampOneWhenMapThenTimAIsMapped()
        {
            _timeScope.Stamp1 = new DateWithContext
            {
                DateContext = DateContextEnum.ActualStart,
                TimeStamp = DateTime.Now
            };

            var result = MapSingle();
            Assert.AreEqual(_timeScope.Stamp1.TimeStamp, result.A);
        }

        [Test]
        public void GivenTimeScopeWithStamp2WhenMapThenBIsMapped()
        {
            _timeScope.Stamp2 = new DateWithContext
            {
                DateContext = DateContextEnum.ActualEnd,
                TimeStamp = DateTime.Now
            };

            var result = MapSingle();
            Assert.AreEqual(_timeScope.Stamp2.TimeStamp, result.B);
        }

        [Test]
        public void GivenTimeScopeWithStamp2WhenMapThenBSpecifiedIsTrue()
        {
            _timeScope.Stamp2 = new DateWithContext
            {
                DateContext = DateContextEnum.ActualEnd,
                TimeStamp = DateTime.Now
            };

            var result = MapSingle();
            Assert.IsTrue(result.BSpecified);
        }

        [Test]
        public void GivenTimeScopeWithoutStamp2WhenMapThenBSpecifiedIsFalse()
        {
            _timeScope.Stamp2 = null;

            var result = MapSingle();
            Assert.IsFalse(result.BSpecified);
        }

        [Test]
        public void GivenNullWhenMapThenNull()
        {
            _timeScopes = null;

            var result = Map();

            Assert.IsNull(result);
        }

        private TIM MapSingle()
        {
            return Map().First();
        }

        private IEnumerable<TIM> Map()
        {
            return _timeMapper.Map(_timeScopes);
        }
    }
}
