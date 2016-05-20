using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public static class XmlReaderUtilities
    {
        public static string GetStringValue(XPathNavigator node, string attributeName)
        {
            var value = node.GetAttribute(attributeName, node.NamespaceURI);
            return value != string.Empty ? value : null;
        }

        public static byte GetByteValue(XPathNavigator node, string attributeName)
        {
            var value = node.GetAttribute(attributeName, node.NamespaceURI);
            return value != string.Empty ? byte.Parse(value) : (byte)0;
        }

    }
}
