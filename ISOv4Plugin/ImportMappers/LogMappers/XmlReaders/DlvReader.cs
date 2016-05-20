using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface IDlvReader
    {
        IEnumerable<DLV> Read(XPathNodeIterator nodeIterator);
    }

    public class DlvReader : IDlvReader
    {
        public IEnumerable<DLV> Read(XPathNodeIterator nodeIterator)
        {
            if(nodeIterator == null)
                return null;

            var dvls = new List<DLV>();
            foreach (XPathNavigator node in nodeIterator)
            {
                dvls.Add(Read(node));
            }
            return dvls;
        }

        private DLV Read(XPathNavigator navigator)
        {
            return new DLV
            {
                A = GetDlvAttribute<string>(navigator, "A"),
                B = GetDlvAttribute<long?>(navigator, "B"),
                C = GetDlvAttribute<string>(navigator, "C"),
                D = GetDlvAttribute<ulong?>(navigator, "D"),
                E = GetDlvAttribute<byte?>(navigator, "E"),
                F = GetDlvAttribute<byte?>(navigator, "F")
            };
        }



        private T GetDlvAttribute<T>(XPathNavigator navigator, string attributeName) 
        {
            if (navigator.SelectSingleNode("@" + attributeName) == null)
            {
                return default(T);
            }

            var value = navigator.GetAttribute(attributeName, navigator.NamespaceURI);
            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            return (T)conv.ConvertFrom(value);
        }



        
    }
}
