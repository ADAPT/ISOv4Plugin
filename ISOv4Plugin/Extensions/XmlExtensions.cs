using System;
using System.IO;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.Extensions
{
    public static class XmlExtensions
    {
        public static XmlNodeList LoadActualNodes(this XmlNode xmlNode, string externalNodeTag, string baseFolder)
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

        public static string GetXmlNodeValue(this XmlNode xmlNode, string xPath)
        {
            var selectedNode = xmlNode.SelectSingleNode(xPath);
            if (selectedNode != null)
                return selectedNode.Value;
            return null;
        }

        public static string GetXmlAttribute(this XmlNode xmlNode, string xPath)
        {
            if (xmlNode.Attributes == null)
                return null;

            var attribute = xmlNode.Attributes[xPath];
            return attribute != null ? attribute.Value : null;
        }

        public static void WriteXmlAttribute(this XmlWriter writer, string attributeName, string attributeValue)
        {
            if (string.IsNullOrEmpty(attributeValue))
                return;

            writer.WriteAttributeString(attributeName, attributeValue);
        }
    }
}
