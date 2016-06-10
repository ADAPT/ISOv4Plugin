using System;
using System.Text;
using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.Writers;
using NUnit.Framework;

namespace ISOv4PluginTest.Writers
{
    [TestFixture]
    public class TreatmentZoneWriterTests
    {
        private XmlWriter _xmlWriter;
        private StringBuilder _sb;
        [SetUp]
        public void Setup()
        {
            _sb = new StringBuilder();
            _xmlWriter = XmlWriter.Create(_sb, new XmlWriterSettings {Indent = true});
        }

        [Test]
        public void ShouldWriteZoneWithMultipleVariables()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(TestData.TestData.MultipleVariables);

            // Act
            using (_xmlWriter)
            {
                TreatmentZoneWriter.Write(_xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestData.TestData.MultipleVariablesOutput, _sb.ToString());
        }

        [Test]
        public void ShouldWriteZoneWithNoVariables()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(TestData.TestData.NoVariables);

            // Act
            using (_xmlWriter)
            {
                TreatmentZoneWriter.Write(_xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestData.TestData.NoVariablesOutput, _sb.ToString());
        }

        [Test]
        public void ShouldWriteVariableWihtMissingUnit()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(TestData.TestData.MissingUnit);

            // Act
            using (_xmlWriter)
            {
                TreatmentZoneWriter.Write(_xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestData.TestData.MissingUnitOutput, _sb.ToString());
        }

        [Test]
        public void ShouldWriteVariableWihtUnsupportedUnitDimension()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(TestData.TestData.UnsupportedUnitDimension);

            // Act
            using (_xmlWriter)
            {
                TreatmentZoneWriter.Write(_xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestData.TestData.UnsupportedUnitDimensionOutput, _sb.ToString());
        }

        [Test]
        public void ShoulHandleNullTreatmentZone()
        {
            // Setup

            // Act
            using (_xmlWriter)
            {
                TreatmentZoneWriter.Write(_xmlWriter, "1", null);
            }

            // Verify
            Assert.IsEmpty(_sb.ToString());
        }
    }
}
