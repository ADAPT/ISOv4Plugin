/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods
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

        public static int? GetXmlNodeValueAsNullableInt(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            int outValue;
            if (int.TryParse(value, out outValue))
            {
                return outValue;
            }
            else
            {
                return null;
            }
        }

        public static int GetXmlNodeValueAsInt(this XmlNode xmlNode, string xPath)
        {
            int? value = xmlNode.GetXmlNodeValueAsNullableInt(xPath);
            if (value.HasValue)
            {
                return value.Value;
            }
            else
            {
                return 0;
            }
        }

        public static  bool IsAttributePresent(this XmlNode node, string attributeName)
        {
            if (node.SelectSingleNode("@" + attributeName) == null)
                return false;
            return true;
        }

        public static uint? GetXmlNodeValueAsNullableUInt(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            uint outValue;
            if (UInt32.TryParse(value, out outValue))
            {
                return outValue;
            }
            else
            {
                return null;
            }
        }

        public static uint GetXmlNodeValueAsUInt(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            uint outValue;
            if (UInt32.TryParse(value, out outValue))
            {
                return outValue;
            }
            else
            {
                return 0;
            }
        }

        public static DateTime? GetXmlNodeValueAsNullableDateTime(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            DateTime outValue;
            if (DateTime.TryParse(value, out outValue))
            {
                return outValue;
            }
            else
            {
                return null;
            }
        }

        public static byte? GetXmlNodeValueAsNullableByte(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            byte outValue;
            if (byte.TryParse(value, out outValue))
            {
                return outValue;
            }
            else
            {
                return null;
            }
        }

        public static byte GetXmlNodeValueAsByte(this XmlNode xmlNode, string xPath)
        {
            byte? value = xmlNode.GetXmlNodeValueAsNullableByte(xPath);
            if (value.HasValue)
            {
                return value.Value;
            }
            else
            {
                return 0;
            }
        }

        public static double? GetXmlNodeValueAsNullableDouble(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            double outValue;
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out outValue))
            {
                return outValue;
            }
            else
            {
                return null;
            }
        }

        public static double GetXmlNodeValueAsDouble(this XmlNode xmlNode, string xPath)
        {
            double? value = xmlNode.GetXmlNodeValueAsNullableDouble(xPath);
            if (value.HasValue)
            {
                return value.Value;
            }
            else
            {
                return 0d;
            }
        }

        public static decimal? GetXmlNodeValueAsNullableDecimal(this XmlNode xmlNode, string xPath)
        {
            string value = GetXmlNodeValue(xmlNode, xPath);
            decimal outValue;
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out outValue))
            {
                return outValue;
            }
            else
            {
                return null;
            }
        }

        public static decimal GetXmlNodeValueAsDecimal(this XmlNode xmlNode, string xPath)
        {
            decimal? value = xmlNode.GetXmlNodeValueAsNullableDecimal(xPath);
            if (value.HasValue)
            {
                return value.Value;
            }
            else
            {
                return 0m;
            }
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

        public static void WriteXmlAttribute<T>(this XmlWriter writer, string attributeName, Nullable<T> attributeValue) where T : struct
        {
            if (!attributeValue.HasValue)
                return;

            if (typeof(T) == typeof(decimal))
            {
                decimal helper = (decimal)(object)(attributeValue.Value);
                writer.WriteAttributeString(attributeName, helper.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (typeof(T) == typeof(double))
            {
                double helper = (double)(object)(attributeValue.Value);
                writer.WriteAttributeString(attributeName, helper.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else {
                writer.WriteAttributeString(attributeName, attributeValue.Value.ToString());
            }
        }
    }
}
