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
            _timeScope.TimeStamp1 = DateTime.Now;
            _timeScope.DateContext = DateContextEnum.ActualStart;

            var result = MapSingle();
            Assert.AreEqual(_timeScope.TimeStamp1, result.A);
        }

        [Test]
        public void GivenTimeScopeWithStamp2WhenMapThenBIsMapped()
        {
            _timeScope.TimeStamp2 = DateTime.Now;
            _timeScope.DateContext = DateContextEnum.ActualStart;

            var result = MapSingle();
            Assert.AreEqual(_timeScope.TimeStamp2, result.B);
        }

        [Test]
        public void GivenTimeScopeWithStamp2WhenMapThenBSpecifiedIsTrue()
        {
            _timeScope.TimeStamp2 = DateTime.Now;
            _timeScope.DateContext = DateContextEnum.ActualStart;

            var result = MapSingle();
            Assert.IsTrue(result.BSpecified);
        }

        [Test]
        public void GivenTimeScopeWithoutStamp2WhenMapThenBSpecifiedIsFalse()
        {
            _timeScope.TimeStamp2 = null;

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
