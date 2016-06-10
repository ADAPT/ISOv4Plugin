using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using NUnit.Framework;

namespace ISOv4PluginLogTest.ImportMappers
{
    [TestFixture]
    public class TimeScopeMapperTest
    {

        private TimeScopeMapper _timeScopeMapper;
        private List<TIM> _tims;
        private Catalog _catalog;

        [SetUp]
        public void Setup()
        {
            _timeScopeMapper = new TimeScopeMapper();
            _tims = new List<TIM>();
            _catalog = new Catalog {TimeScopes = new List<TimeScope>()};
        }

        [Test]
        public void GivenTimsWhenMapThenTimescopeIsCreatedForEachTim()
        {
            _tims.Add(new TIM());            
            _tims.Add(new TIM());            
            _tims.Add(new TIM());

            var result = Map();

            Assert.AreEqual(_tims.Count, result.Count);
        }

        [Test]
        public void GivenTimWHenMapThenStamp1IsMapped()
        {
            var tim = new TIM
            {
                A = DateTime.Now.AddDays(2),
            };
            _tims.Add(tim);

            var timescope = MapSingle();
            Assert.AreEqual(tim.A, timescope.TimeStamp1);
            Assert.AreEqual(DateContextEnum.ActualStart, timescope.DateContext);
        }

        [Test]
        public void GivenTimWhenMapThenStamp2IsMapped()
        {
            var tim = new TIM
            {
                A = DateTime.Now,
                B = DateTime.Now.AddHours(8)
            };
            _tims.Add(tim);

            var timescope = MapSingle();
            Assert.AreEqual(tim.B, timescope.TimeStamp2);
            Assert.AreEqual(DateContextEnum.ActualStart, timescope.DateContext);
        }

        [Test]
        public void GivenTimWhenMapThenTimeStampIsAddedToCatalog()
        {
            _tims.Add(new TIM{ A = DateTime.Now.AddHours(-2), B = DateTime.MinValue.AddDays(24)});

            var timescope = MapSingle();
            Assert.AreEqual(_tims.Count, _catalog.TimeScopes.Count);
            Assert.AreSame(timescope, _catalog.TimeScopes[0]);
        }

        [Test]
        public void GivenNullTimListWhenMapThenNoTimeScope()
        {
            _tims = null;

            var result = Map();

            Assert.IsNull(result);
        }

        private TimeScope MapSingle()
        {
            return Map().First();
        }

        private List<TimeScope> Map()
        {
            return _timeScopeMapper.Map(_tims, _catalog);
        }
    }
}
