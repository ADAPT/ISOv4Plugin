using System;
using System.IO;
using System.Xml;

namespace AgGateway.ADAPT.Plugins
{
    internal static class XmlExtensions
    {
        internal static XmlNodeList LoadActualNodes(this XmlNode xmlNode, string externalNodeTag, string baseFolder)
        {
            if (string.Equals(xmlNode.Name, externalNodeTag, StringComparison.OrdinalIgnoreCase))
            {
                var fileName = xmlNode.GetXmlNodeValue("@A");
                if (fileName == null)
                    return null;
                var xmlDoc = new XmlDocument();

                string filePath = Path.ChangeExtension(Path.Combine(baseFolder, fileName), ".xml");
                try
                {
                    xmlDoc.Load(filePath);

                    return xmlDoc.SelectNodes("XFC/*");
                }
                catch (XmlException) { }
                catch (IOException) { }
            }
            return null;
        }

        internal static string GetXmlNodeValue(this XmlNode xmlNode, string xPath)
        {
            var selectedNode = xmlNode.SelectSingleNode(xPath);
            if (selectedNode != null)
                return selectedNode.Value;
            return null;
        }

        internal static void WriteXmlAttribute(this XmlWriter writer, string attributeName, string attributeValue)
        {
            if (string.IsNullOrEmpty(attributeValue))
                return;

            writer.WriteAttributeString(attributeName, attributeValue);
        }
    }
}
