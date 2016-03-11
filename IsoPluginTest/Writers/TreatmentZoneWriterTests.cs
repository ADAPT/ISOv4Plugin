using System.Text;
using System.Xml;
using AgGateway.ADAPT.IsoPlugin;
using AgGateway.ADAPT.IsoPlugin.Writers;
using NUnit.Framework;

namespace IsoPluginTest.Writers
{
    [TestFixture]
    public class TreatmentZoneWriterTests
    {
        [Test]
        public void ShouldWriteZoneWithMultipleVariables()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(@"TestData\TreatmentZone\MultipleVariables.json");

            // Act
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true }))
            {
                TreatmentZoneWriter.Write(xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\TreatmentZone\MultipleVariablesOutput.xml"), sb.ToString());
        }

        [Test]
        public void ShouldWriteZoneWithNoVariables()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(@"TestData\TreatmentZone\NoVariables.json");

            // Act
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true }))
            {
                TreatmentZoneWriter.Write(xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\TreatmentZone\NoVariablesOutput.xml"), sb.ToString());
        }

        [Test]
        public void ShouldWriteVariableWihtMissingUnit()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(@"TestData\TreatmentZone\MissingUnit.json");

            // Act
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true }))
            {
                TreatmentZoneWriter.Write(xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\TreatmentZone\MissingUnitOutput.xml"), sb.ToString());
        }

        [Test]
        public void ShouldWriteVariableWihtUnsupportedUnitDimension()
        {
            // Setup
            var treatmentZone = TestHelpers.LoadFromJson<TreatmentZone>(@"TestData\TreatmentZone\UnsupportedUnitDimension.json");

            // Act
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true }))
            {
                TreatmentZoneWriter.Write(xmlWriter, "1", treatmentZone);
            }

            // Verify
            Assert.AreEqual(TestHelpers.LoadFromFile(@"TestData\TreatmentZone\UnsupportedUnitDimensionOutput.xml"), sb.ToString());
        }

        [Test]
        public void ShoulHandleNullTreatmentZone()
        {
            // Setup

            // Act
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true }))
            {
                TreatmentZoneWriter.Write(xmlWriter, "1", null);
            }

            // Verify
            Assert.IsNullOrEmpty(sb.ToString());
        }
    }
}
