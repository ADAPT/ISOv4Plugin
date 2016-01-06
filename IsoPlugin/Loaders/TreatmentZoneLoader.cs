using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class TreatmentZoneLoader
    {
        private TaskDataDocument _taskDocument;
        private Dictionary<string, List<TreatmentZone>> _zones;

        private TreatmentZoneLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _zones = new Dictionary<string, List<TreatmentZone>>();
        }

        internal static Dictionary<string, List<TreatmentZone>> Load(XmlNode inputNode, TaskDataDocument taskDocument)
        {
            var loader = new TreatmentZoneLoader(taskDocument);

            return loader.Load(inputNode);
        }

        private Dictionary<string, List<TreatmentZone>> Load(XmlNode inputNode)
        {
            LoadTreatmentZones(inputNode.SelectNodes("TZN"));

            return _zones;
        }

        private void LoadTreatmentZones(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string zoneId;
                var zones = LoadTreatmentZone(inputNode, out zoneId);
                if (zones != null)
                    _zones.Add(zoneId, zones);
            }
        }

        private List<TreatmentZone> LoadTreatmentZone(XmlNode inputNode, out string zoneId)
        {
            // Required fields. Do not proceed if they are missing
            zoneId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(zoneId))
                return null;

            // Optional fields
            var zone = LoadDataVariables(inputNode.SelectNodes("PDV"));

            return zone;
        }

        private List<TreatmentZone> LoadDataVariables(XmlNodeList inputNodes)
        {
            var zones = new List<TreatmentZone>();
            foreach (XmlNode inputNode in inputNodes)
            {
                var dataVariable = LoadDataVariable(inputNode);
                if (dataVariable != null)
                    zones.Add(dataVariable);
            }
            return zones;
        }

        private TreatmentZone LoadDataVariable(XmlNode inputNode)
        {
            var ddiValue = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(ddiValue))
                return null;

            var ddi = Convert.ToInt32(ddiValue, 16);

            long dataValue;
            if (!inputNode.GetXmlNodeValue("@B").ParseValue(out dataValue))
                dataValue = 0;

            var userUnit = _taskDocument.Units.FindById(inputNode.GetXmlNodeValue("@E"));

            var unit = UnitFactory.Instance.GetUnitByDdi(ddi);

            var numericValue = new NumericValue(unit.ToModelUom(), unit.ConvertToUnit(dataValue));
            var zone = new TreatmentZone();
            zone.ProductId = inputNode.GetXmlNodeValue("@C");
            zone.DataValue = new NumericRepresentationValue(null, userUnit.ToModelUom(), numericValue);

            return zone;
        }
    }
}
