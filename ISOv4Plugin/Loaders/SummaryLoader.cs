using System;
using System.Collections.Generic;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Representation;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class TaskSummaryLoader
    {
        private readonly Dictionary<int, DdiDefinition> _ddis;
        private readonly RepresentationMapper _mapper;

        private TaskSummaryLoader()
        {
            _ddis = DdiLoader.Ddis;
            _mapper = new RepresentationMapper();
        }

        public static List<StampedMeteredValues> Load(XmlNodeList inputNodes)
        {
            var loader = new TaskSummaryLoader();

            var stampedValues = new List<StampedMeteredValues>();
            foreach (XmlNode inputNode in inputNodes)
            {
                var stampedValue = loader.LoadTimeNode(inputNode);
                if (stampedValue != null)
                    stampedValues.Add(stampedValue);
            }
            return stampedValues.Count == 0 ? null : stampedValues;
         }

        private StampedMeteredValues LoadTimeNode(XmlNode inputNode)
        {
            var timeScope = TimestampLoader.Load(inputNode);
            // Required attributes
            if (timeScope == null)
                return null;

            return new StampedMeteredValues
            {
                Stamp = timeScope,
                Values = LoadMeteredValues(inputNode.SelectNodes("DLV"))
            };
        }

        private List<MeteredValue> LoadMeteredValues(XmlNodeList inputNodes)
        {
            var values = new List<MeteredValue>();
            foreach (XmlNode inputNode in inputNodes)
            {
                var value = LoadMeteredValue(inputNode);
                if (value != null)
                    values.Add(value);
            }

            return values;
        }

        private MeteredValue LoadMeteredValue(XmlNode inputNode)
        {
            var ddiValue = inputNode.GetXmlNodeValue("@A");
            if (string.IsNullOrEmpty(ddiValue))
                return null;

            var ddi = Convert.ToInt32(ddiValue, 16);

            long dataValue;
            if (!inputNode.GetXmlNodeValue("@B").ParseValue(out dataValue))
                dataValue = 0;

            if (!_ddis.ContainsKey(ddi))
                return null;
            var unitOfMeasure = _mapper.GetUnitForDdi(ddi);
            if (unitOfMeasure == null)
                return null;

            var ddiDefintion = _ddis[ddi];

            return new MeteredValue
            {
                Value = new NumericRepresentationValue(_mapper.Map(ddi) as NumericRepresentation, 
                    unitOfMeasure, new NumericValue(unitOfMeasure, dataValue * ddiDefintion.Resolution))
            };
        }
    }
}
