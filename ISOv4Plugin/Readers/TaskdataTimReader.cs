using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Readers
{
    public interface ITaskdataTimReader
    {
        IEnumerable<TIM> Read(XPathNodeIterator xPathNodeIterator);
    }

    public class TaskdataTimReader : ITaskdataTimReader
    {
        private readonly IDlvReader _dlvReader;

        public TaskdataTimReader() : this(new DlvReader())
        {
            
        }

        public TaskdataTimReader(IDlvReader dlvReader)
        {
            _dlvReader = dlvReader;
        }

        public IEnumerable<TIM> Read(XPathNodeIterator xPathNodeIterator)
        {
            var tims = new List<TIM>();
            if (xPathNodeIterator.Count == 0)
                return tims;
            foreach (XPathNavigator node in xPathNodeIterator)
            {
                var tim = new TIM
                {
                    A = CreateDateTime(node, "A"),
                    B = CreateDateTime(node, "B"),
                    D = FindEnumValue(node, "D"),
                    Items = GetItems(node)
                };
                tims.Add(tim);
            }

            return tims;
        }

        private IWriter[] GetItems(XPathNavigator node)
        {
            var dlvs = node.SelectDescendants("DLV", node.NamespaceURI, true);
            var mappedDlvs = _dlvReader.Read(dlvs);

            return mappedDlvs.ToArray();
        }

        private TIMD FindEnumValue(XPathNavigator node, string attribute)
        {
            var value = node.GetAttribute(attribute, node.NamespaceURI);

            TIMD outValue;
            Enum.TryParse(value, out outValue);
            return outValue;
        }

        private DateTime CreateDateTime(XPathNavigator node, string attribute)
        {
            var value = node.GetAttribute(attribute, node.NamespaceURI);
            return DateTime.Parse(value);
        }
    }
}
