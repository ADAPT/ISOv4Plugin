/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
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
        public int ProcessDataValue { get; set; }
        public string ProductIdRef { get; set; }
        public string DeviceElementIdRef { get; set; }
        public string ValuePresentationIdRef { get; set; }
        public int? ActualCulturalPracticeValue { get; set; }
        public int? ElementTypeInstanceValue { get; set; }

        //Child Elements
        public List<ISOProcessDataVariable> ChildProcessDataVariables { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PDV");
            xmlBuilder.WriteXmlAttribute("A", ProcessDataDDI);
            xmlBuilder.WriteXmlAttribute<int>("B", ProcessDataValue);
            xmlBuilder.WriteXmlAttribute("C", ProductIdRef);
            xmlBuilder.WriteXmlAttribute("D", DeviceElementIdRef);
            xmlBuilder.WriteXmlAttribute("E", ValuePresentationIdRef);
            xmlBuilder.WriteXmlAttribute<int>("F", ActualCulturalPracticeValue);
            xmlBuilder.WriteXmlAttribute<int>("G", ElementTypeInstanceValue);

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
            pdv.ProcessDataValue = node.GetXmlNodeValueAsInt("@B");
            pdv.ProductIdRef = node.GetXmlNodeValue("@C");
            pdv.DeviceElementIdRef = node.GetXmlNodeValue("@D");
            pdv.ValuePresentationIdRef = node.GetXmlNodeValue("@E");
            pdv.ActualCulturalPracticeValue = node.GetXmlNodeValueAsNullableInt("@F");
            pdv.ElementTypeInstanceValue = node.GetXmlNodeValueAsNullableInt("@G");

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

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.ProcessDataDDI, 4, errors, "A"); //DDI validation could be improved upon
            RequireRange(this, x => x.ProcessDataValue, Int32.MinValue, Int32.MaxValue - 1, errors, "B");
            ValidateString(this, x => x.ProductIdRef, 14, errors, "C");
            ValidateString(this, x => x.DeviceElementIdRef, 14, errors, "D");
            ValidateString(this, x => x.ValuePresentationIdRef, 14, errors, "E");
            if (ActualCulturalPracticeValue.HasValue) ValidateRange(this, x => x.ActualCulturalPracticeValue.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "F");
            if (ElementTypeInstanceValue.HasValue) ValidateRange(this, x => x.ElementTypeInstanceValue.Value, Int32.MinValue, Int32.MaxValue - 1, errors, "G");
            if (ChildProcessDataVariables.Count > 0) ChildProcessDataVariables.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}
