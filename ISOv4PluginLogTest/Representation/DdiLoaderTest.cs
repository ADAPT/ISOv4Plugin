using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using NUnit.Framework;

namespace ISOv4PluginLogTest.Representation
{
    [TestFixture]
    public class DdiLoaderTest
    {
        [Test]
        public void GivenNumericDdiEntryWhenLoadThenSetsId()
        {
            var result = DdiLoader.Load(NumericDdiDefinition);

            Assert.AreEqual(7, result.Values.Single().Id);
        }

        [Test]
        public void GivenNumericDdiEntryWhenLoadThenSetsName()
        {
            var result = DdiLoader.Load(NumericDdiDefinition);

            Assert.AreEqual("Actual Mass Per Area Application Rate", result.Values.Single().Name);
        }

        [Test]
        public void GivenNumericDdiEntryWhenLoadThenSetsDescription()
        {
            var result = DdiLoader.Load(NumericDdiDefinition);

            Assert.AreEqual("Actual Application Rate specified as mass per area", result.Values.Single().Definition);
        }

        [Test]
        public void GivenNumericDdiEntryWhenLoadThenSetsUnit()
        {
            var result = DdiLoader.Load(NumericDdiDefinition);

            var ddiDefinition = result.Values.Single();
            Assert.AreEqual("mg/m²", ddiDefinition.Unit);
        }

        [Test]
        public void GivenNumericDdiEntryWhenLoadThenSetsResolution()
        {
            var result = DdiLoader.Load(NumericDdiDefinition2);

            var ddiDefinition = result.Values.Single();
            Assert.AreEqual(.001, ddiDefinition.Resolution);
        }

        [TearDown]
        public void TearDown()
        {
            DdiLoader.Load();
        }

        private const string NumericDdiDefinition = "\n\nDD Entity: 7 Actual Mass Per Area Application Rate\nDefinition: Actual Application Rate specified as mass per area\nComment: \nTypically used by Device Classes: \n4 - Planters /Seeders\n5 - Fertilizer\n6 - Sprayers\n10 - Irrigation\nUnit: mg/m² - Mass per area unit\nResolution: 1\nSAE SPN: not specified\nRange: 0 - 2147483647\nSubmit by: Part 10 Task Force\nSubmit Date: 2003-08-01\nSubmit Company: 89 - Kverneland Group, Electronics Division\nRevision Number: 1\nCurrent Status: ISO-Published\nStatus Date: 2005-02-02\nStatus Comments: DDEs have been moved to published for creating the new Annex A version.\nAttachments: \nnone\n\n";
        private const string NumericDdiDefinition2 = "\n\nDD Entity: 11 Setpoint Count Per Area Application Rate\nDefinition: Setpoint Application Rate specified as count per area\nComment: \nTypically used by Device Classes: \n4 - Planters /Seeders\n5 - Fertilizer\n6 - Sprayers\n10 - Irrigation\nUnit: /m² - Quantity per area unit\nResolution: 0,001\nSAE SPN: not specified\nRange: 0 - 2147483647\nSubmit by: Part 10 Task Force\nSubmit Date: 2003-08-01\nSubmit Company: 89 - Kverneland Group, Electronics Division\nRevision Number: 1\nCurrent Status: ISO-Published\nStatus Date: 2005-02-02\nStatus Comments: DDEs have been moved to published for creating the new Annex A version.\nAttachments: \nnone\n\n\n";
    }
}
