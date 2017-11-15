/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOProcessDataVariable : ISOElement
    {
        public ISOProcessDataVariable()
        {
            ChildProcessDataVariables = new List<ISOProcessDataVariable>();
        }

        //Attributes
        public string ProcessDataDDI { get; set; }
        public long ProcessDataValue { get; set; }
        public string ProductIdRef { get; set; }
        public string DeviceElementIdRef { get; set; }
        public string ValuePresentationIdRef { get; set; }
        public long? ActualCulturalPracticeValue { get; set; }
        public long? ElementTypeInstanceValue { get; set; }

        //Child Elements
        public List<ISOProcessDataVariable> ChildProcessDataVariables { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PDV");
            xmlBuilder.WriteXmlAttribute("A", ProcessDataDDI);
            xmlBuilder.WriteXmlAttribute<long>("B", ProcessDataValue);
            xmlBuilder.WriteXmlAttribute("C", ProductIdRef);
            xmlBuilder.WriteXmlAttribute("D", DeviceElementIdRef);
            xmlBuilder.WriteXmlAttribute("E", ValuePresentationIdRef);
            xmlBuilder.WriteXmlAttribute<long>("F", ActualCulturalPracticeValue);
            xmlBuilder.WriteXmlAttribute<long>("G", ElementTypeInstanceValue);

            foreach (var item in ChildProcessDataVariables)
            {
                item.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOProcessDataVariable ReadXML(XmlNode node)
        {
            ISOProcessDataVariable pdv = new ISOProcessDataVariable();
            pdv.ProcessDataDDI = node.GetXmlNodeValue("@A");
            pdv.ProcessDataValue = node.GetXmlNodeValueAsLong("@B");
            pdv.ProductIdRef = node.GetXmlNodeValue("@C");
            pdv.DeviceElementIdRef = node.GetXmlNodeValue("@D");
            pdv.ValuePresentationIdRef = node.GetXmlNodeValue("@E");
            pdv.ActualCulturalPracticeValue = node.GetXmlNodeValueAsNullableLong("@F");
            pdv.ElementTypeInstanceValue = node.GetXmlNodeValueAsNullableLong("@G");

            XmlNodeList pdvNodes = node.SelectNodes("PDV");
            if (pdvNodes != null)
            {
                pdv.ChildProcessDataVariables.AddRange(ISOProcessDataVariable.ReadXML(pdvNodes));
            }

            return pdv;
        }

        public static List<ISOProcessDataVariable> ReadXML(XmlNodeList nodes)
        {
            List<ISOProcessDataVariable> items = new List<ISOProcessDataVariable>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOProcessDataVariable.ReadXML(node));
            }
            return items;
        }
    }
}