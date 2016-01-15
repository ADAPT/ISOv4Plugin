using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal class TreatmentZoneLoader
    {
        private TaskDataDocument _taskDocument;
        private Dictionary<string, TreatmentZone> _zones;

        private TreatmentZoneLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _zones = new Dictionary<string, TreatmentZone>();
        }

        internal static Dictionary<string, TreatmentZone> Load(XmlNode inputNode, TaskDataDocument taskDocument)
        {
            var loader = new TreatmentZoneLoader(taskDocument);

            return loader.Load(inputNode);
        }

        private Dictionary<string, TreatmentZone> Load(XmlNode inputNode)
        {
            LoadTreatmentZones(inputNode.SelectNodes("TZN"));

            return _zones;
        }

        private void LoadTreatmentZones(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string zoneId;
                var zone = LoadTreatmentZone(inputNode, out zoneId);
                if (zone != null)
                    _zones.Add(zoneId, zone);
            }
        }

        private TreatmentZone LoadTreatmentZone(XmlNode inputNode, out string zoneId)
        {
            // Required fields. Do not proceed if they are missing
            zoneId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(zoneId))
                return null;

            // Optional fields
            var zone = new TreatmentZone { Variables = new List<DataVariable>() };
            zone.Name = inputNode.GetXmlNodeValue("@B");
            LoadDataVariables(inputNode.SelectNodes("PDV"), zone);

            return zone;
        }

        private void LoadDataVariables(XmlNodeList inputNodes, TreatmentZone zone)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                var dataVariable = LoadDataVariable(inputNode);
                if (dataVariable != null)
                    zone.Variables.Add(dataVariable);
            }
        }

        private DataVariable LoadDataVariable(XmlNode inputNode)
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

            var dataVariable = new DataVariable();

            var numericValue = new NumericValue(unit.ToModelUom(), unit.ConvertToUnit(dataValue));
            dataVariable.Value = new NumericRepresentationValue(null, userUnit.ToModelUom(), numericValue);

            dataVariable.ProductId = inputNode.GetXmlNodeValue("@C");

            return dataVariable;
        }
    }
}
