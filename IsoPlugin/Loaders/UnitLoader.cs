using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal class UnitLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private Dictionary<string, ValuePresentation> _units;

        private UnitLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _units = new Dictionary<string, ValuePresentation>();
        }

        internal static Dictionary<string, ValuePresentation> Load(TaskDataDocument taskDocument)
        {
            var loader = new UnitLoader(taskDocument);

            return loader.Load();
        }

        private Dictionary<string, ValuePresentation> Load()
        {
            LoadUnits(_rootNode.SelectNodes("VPN"));
            ProcessExternalNodes();

            return _units;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'VPN')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadUnits(inputNodes);
            }
        }

        private void LoadUnits(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                string unitId;
                var unit = LoadUnit(inputNode, out unitId);
                if (unit != null)
                    _units.Add(unitId, unit);
            }
        }

        private ValuePresentation LoadUnit(XmlNode inputNode, out string unitId)
        {
            var unit = new ValuePresentation();

            // Required fields. Do not proceed if they are missing
            unitId = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(unitId) ||
                !LoadRequiredFields(inputNode, unit))
                return null;

            // Optional fields
            unit.Symbol = inputNode.GetXmlNodeValue("@E");

            _taskDocument.LoadLinkedIds(unitId, unit.Id);

            return unit;
        }

        private static bool LoadRequiredFields(XmlNode inputNode, ValuePresentation unit)
        {
            var offsetValue = inputNode.GetXmlNodeValue("@B");
            var scaleValue = inputNode.GetXmlNodeValue("@C");
            var decimalDigitsValue = inputNode.GetXmlNodeValue("@D");

            if (string.IsNullOrEmpty(offsetValue) ||
                string.IsNullOrEmpty(scaleValue) ||
                string.IsNullOrEmpty(decimalDigitsValue))
                return false;

            long offset;
            if (!long.TryParse(offsetValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out offset))
                return false;
            unit.Offset = offset;

            double scale;
            if (!double.TryParse(scaleValue, NumberStyles.Number, CultureInfo.InvariantCulture, out scale) ||
                scale < 0)
                return false;
            unit.Scale = scale;

            byte decimalDigits;
            if (!byte.TryParse(decimalDigitsValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimalDigits) ||
                decimalDigits < 0 || decimalDigits > 7)
                return false;
            unit.DecimalDigits = decimalDigits;

            return true;
        }
    }
}
