using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface IPtnReader
    {
        PTN Read(XPathNavigator navigator);
        IEnumerable<PTN> Read(XPathNodeIterator nodeIterator);
    }

    public class PtnReader : IPtnReader
    {

        public PTN Read(XPathNavigator navigator)
        {
            if (navigator == null)
                return null;

            return new PTN
            {
                A = GetPtnAttribute<double?>(navigator, "A"),
                ASpecified = IsAttributePresent(navigator, "A"),

                B = GetPtnAttribute<double?>(navigator, "B"),
                BSpecified = IsAttributePresent(navigator, "B"),

                C = GetPtnAttribute<long?>(navigator, "C"),
                CSpecified = IsAttributePresent(navigator, "C"),

                D = GetPtnAttribute<byte?>(navigator, "D"),
                DSpecified = IsAttributePresent(navigator, "D"),

                E = GetPtnAttribute<double?>(navigator, "E"),
                ESpecified = IsAttributePresent(navigator, "E"),

                F = GetPtnAttribute<double?>(navigator, "F"),
                FSpecified = IsAttributePresent(navigator, "F"),

                G = GetPtnAttribute<byte?>(navigator, "G"),
                GSpecified = IsAttributePresent(navigator, "G"),

                H = GetPtnAttribute<ulong?>(navigator, "H"),
                HSpecified = IsAttributePresent(navigator, "H"),

                I = GetPtnAttribute<ushort?>(navigator, "I"),
                ISpecified = IsAttributePresent(navigator, "I"),
            };
        }

        private bool IsAttributePresent(XPathNavigator navigator, string attributeName)
        {
            if (navigator.SelectSingleNode("@" + attributeName) == null)
                return false;
            return true;
        }

        public IEnumerable<PTN> Read(XPathNodeIterator nodeIterator)
        {
            if(nodeIterator == null)
                return null;

            var ptns = new List<PTN>();
            foreach (XPathNavigator node in nodeIterator)
            {
                ptns.Add(Read(node));
            }
            return ptns;
        }

        private T GetPtnAttribute<T>(XPathNavigator navigator, string attributeName)
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
