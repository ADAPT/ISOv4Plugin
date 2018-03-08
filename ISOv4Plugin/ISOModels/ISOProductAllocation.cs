/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOProductAllocation : ISOElement
    {
        //Attributes
        public string ProductIdRef { get; set; }
        public string QuantityDDI { get; set; }
        public int? QuantityValue { get; set; }
        public ISOTransferMode? TransferMode { get { return (ISOTransferMode?)TransferModeInt; } set { TransferModeInt = (int?)value; } }
        private int? TransferModeInt { get; set; }
        public string DeviceElementIdRef { get; set; }
        public string ValuePresentationIdRef { get; set; }

        //Child Elements
        public ISOAllocationStamp AllocationStamp {get; set;}


        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("PAN");
            xmlBuilder.WriteXmlAttribute("A", ProductIdRef);
            xmlBuilder.WriteXmlAttribute("B", QuantityDDI);
            xmlBuilder.WriteXmlAttribute("C", QuantityValue);
            xmlBuilder.WriteXmlAttribute<ISOTransferMode>("D", TransferMode);
            xmlBuilder.WriteXmlAttribute("E", DeviceElementIdRef);
            xmlBuilder.WriteXmlAttribute("F", ValuePresentationIdRef);
            if (AllocationStamp != null)
            {
                AllocationStamp.WriteXML(xmlBuilder);
            }
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISOProductAllocation ReadXML(XmlNode node)
        {
            ISOProductAllocation item = new ISOProductAllocation();
            item.ProductIdRef = node.GetXmlNodeValue("@A");
            item.QuantityDDI = node.GetXmlNodeValue("@B");
            item.QuantityValue = node.GetXmlNodeValueAsNullableInt("@C");
            item.TransferModeInt = node.GetXmlNodeValueAsNullableInt("@D");
            item.DeviceElementIdRef = node.GetXmlNodeValue("@E");
            item.ValuePresentationIdRef = node.GetXmlNodeValue("@F");
            item.AllocationStamp = ISOAllocationStamp.ReadXML(node.SelectSingleNode("ASP"));
            return item;
        }

        public static IEnumerable<ISOProductAllocation> ReadXML(XmlNodeList nodes)
        {
            List<ISOProductAllocation> items = new List<ISOProductAllocation>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOProductAllocation.ReadXML(node));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.ProductIdRef, 14, errors, "A");
            ValidateString(this, x => x.QuantityDDI, 4, errors, "B"); //DDI validation could be improved upon
            if (QuantityValue.HasValue) ValidateRange(this, x => x.QuantityValue.Value, 0, Int32.MaxValue - 1, errors, "C");
            if (TransferModeInt.HasValue) ValidateEnumerationValue(typeof(ISOTransferMode), TransferModeInt.Value, errors);
            ValidateString(this, x => x.DeviceElementIdRef, 14, errors, "E");
            ValidateString(this, x => x.ValuePresentationIdRef, 14, errors, "F");
            if (AllocationStamp != null) AllocationStamp.Validate(errors);
            return errors;
        }
    }
}
