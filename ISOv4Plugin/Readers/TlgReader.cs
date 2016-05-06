using System.Collections.Generic;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Readers
{
    public interface ITlgReader
    {
        IEnumerable<TLG> Read(XPathNodeIterator nodeIterator);
    }

    public class TlgReader : ITlgReader
    {
        public IEnumerable<TLG> Read(XPathNodeIterator nodeIterator)
        {
            var tlgs = new List<TLG>();
            if (nodeIterator.Count == 0)
                return tlgs;

            foreach (XPathNavigator node in nodeIterator)
            {
                var tlg = new TLG
                {
                    A = GetValue(node, "A")
                };

                tlgs.Add(tlg);
            }

            return tlgs;
        }

        private string GetValue(XPathNavigator node, string attribute)
        {
            var value = node.GetAttribute(attribute, node.NamespaceURI);
            return value != string.Empty ? value : null;
        }
    }
}
