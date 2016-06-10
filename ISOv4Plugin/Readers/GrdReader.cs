using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Readers
{
    public interface IGrdReader
    {
        IEnumerable<GRD> Read(XPathNodeIterator nodeIterator);
    }

    public class GrdReader : IGrdReader
    {
        public IEnumerable<GRD> Read(XPathNodeIterator nodeIterator)
        {
            var grds = new List<GRD>();
            if (nodeIterator.Count == 0)
                return grds;

            foreach (XPathNavigator node in nodeIterator)
            {
                var grd = new GRD
                {
                    A = GetGrdAttribute<double>(node, "A"),
                    B = GetGrdAttribute<double>(node, "B"),
                    C = GetGrdAttribute<double>(node, "C"),
                    D = GetGrdAttribute<double>(node, "D"),
                    E = GetGrdAttribute<ulong>(node, "E"),
                    F = GetGrdAttribute<ulong>(node, "F"),
                    G = GetGrdAttribute<string>(node, "G"),
                    H = GetGrdAttribute<ulong?>(node, "H"),
                    I = GetGrdAttribute<byte>(node, "I"),
                    J = GetGrdAttribute<byte?>(node, "J"),
                };

                grds.Add(grd);
            }

            return grds;
        }

        private T GetGrdAttribute<T>(XPathNavigator navigator, string attributeName)
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