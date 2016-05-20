using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface ITimReader
    {
        List<TIM> Read(XPathDocument xDocument);
        TIM Read(XPathNavigator timElement);
    }

    public class TimReader : ITimReader
    {
        private readonly IPtnReader _ptnReader;
        private readonly IDlvReader _dlvReader;

        public TimReader() : this(new PtnReader(), new DlvReader())
        {
            
        }

        public TimReader(IPtnReader ptnReader, IDlvReader dlvReader)
        {
            _ptnReader = ptnReader;
            _dlvReader = dlvReader;
        }

        //public TIMHeader Read(XDocument xDocument)
        //{
        //    var timElement = xDocument.Descendants("TIM").FirstOrDefault();

        //    if (timElement == null)
        //        return null;

        //    var ptnElement = timElement.Descendants("PTN").FirstOrDefault();
        //    var dlvElements = xDocument.Descendants("DLV").ToList();

        //    return new TIMHeader
        //    {
        //        Start = new HeaderProperty(timElement.Attribute("A")),
        //        Stop = new HeaderProperty(timElement.Attribute("B")),
        //        Duration = new HeaderProperty(timElement.Attribute("C")),
        //        Type = new HeaderProperty(timElement.Attribute("D")),
        //        PtnHeader = _ptnReader.Read(ptnElement),
        //        DLVs = _dlvReader.Read(dlvElements).ToList(),
        //    };
        //}

        public List<TIM> Read(XPathDocument xDocument)
        {
            var timHeaders = new List<TIM>();

            var navigator = xDocument.CreateNavigator();
            var timElements = navigator.SelectDescendants("TIM", navigator.NamespaceURI, true);

            if (timElements.Count == 0)
                return null;
                
            foreach (XPathNavigator timElement in timElements)
            {
                timHeaders.Add(Read(timElement));
            }

            return timHeaders;
        }

        public TIM Read(XPathNavigator timElement)
        {
            return new TIM
            {
                ASpecified = IsAttributePresent(timElement, "A"),
                BSpecified = IsAttributePresent(timElement, "B"),
                CSpecified = IsAttributePresent(timElement, "C"),
                DSpecified = IsAttributePresent(timElement, "D"),

                A = CreateDateTime(timElement, "A"),
                B = CreateDateTime(timElement, "B"),
                C = GetTimAttribute<ulong?>(timElement, "C"),
                D = FindEnumValue(timElement, "D"),
                Items = GetItems(timElement)
            };
        }

        private bool IsAttributePresent(XPathNavigator navigator, string attributeName)
        {
            if (navigator.SelectSingleNode("@" + attributeName) == null)
                return false;
            return true;
        }



        private T GetTimAttribute<T>(XPathNavigator navigator, string attributeName)
        {
            if (navigator.SelectSingleNode("@" + attributeName) == null)
            {
                return default(T);
            }

            var value = navigator.GetAttribute(attributeName, navigator.NamespaceURI);
            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            return (T)conv.ConvertFrom(value);
        }

        private ulong? FindULongValue(XPathNavigator node, string attribute)
        {
            if (node.SelectSingleNode("@" + attribute) == null)
                return null;

            var value = node.GetAttribute(attribute, node.NamespaceURI);
            return Convert.ToUInt64(value);
        }

        private IWriter[] GetItems(XPathNavigator node)
        {
            var iWriters = new List<IWriter>();

            var ptns = node.SelectDescendants("PTN", node.NamespaceURI, true);
            var mappedPtns = _ptnReader.Read(ptns);

            var dlvs = node.SelectDescendants("DLV", node.NamespaceURI, true);
            var mappedDlvs = _dlvReader.Read(dlvs);

            if(mappedPtns != null)
                iWriters.AddRange(mappedPtns.ToList());

            if(mappedDlvs != null)
                iWriters.AddRange(mappedDlvs.ToList());

            return iWriters.ToArray();
        }

        private DateTime? CreateDateTime(XPathNavigator node, string attribute)
        {
            if (node.SelectSingleNode("@" + attribute) == null)
                return null;

            var value = node.GetAttribute(attribute, node.NamespaceURI);
            if (string.IsNullOrEmpty(value))
                return null;

            return DateTime.Parse(value);
        }

        private TIMD FindEnumValue(XPathNavigator node, string attribute)
        {
            var value = node.GetAttribute(attribute, node.NamespaceURI);

            TIMD outValue;
            Enum.TryParse(value, out outValue);
            return outValue;
        }
    }
}
