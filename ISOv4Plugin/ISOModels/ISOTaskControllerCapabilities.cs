/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOTaskControllerCapabilities : ISOElement
    {
        //Attributes
        public string TaskControllerControlFunctionNAME { get; set; }
        public string TaskControllerDesignator { get; set; }
        public byte VersionNumber { get; set; }
        public byte ProvidedCapabilities { get; set; }
        public byte NumberOfBoomsSectionControl { get; set; }
        public byte NumberOfSectionsSectionControl { get; set; }
        public byte NumberOfControlChannels { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TCC");
            xmlBuilder.WriteXmlAttribute("A", TaskControllerControlFunctionNAME);
            xmlBuilder.WriteXmlAttribute("B", TaskControllerDesignator);
            xmlBuilder.WriteXmlAttribute("C", VersionNumber.ToString());
            xmlBuilder.WriteXmlAttribute("D", ProvidedCapabilities.ToString());
            xmlBuilder.WriteXmlAttribute("E", NumberOfBoomsSectionControl.ToString());
            xmlBuilder.WriteXmlAttribute("F", NumberOfSectionsSectionControl.ToString());
            xmlBuilder.WriteXmlAttribute("G", NumberOfControlChannels.ToString());
            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOTaskControllerCapabilities ReadXML(XmlNode node)
        {
            ISOTaskControllerCapabilities capabilities = new ISOTaskControllerCapabilities();
            capabilities.TaskControllerControlFunctionNAME = node.GetXmlNodeValue("@A");
            capabilities.TaskControllerDesignator = node.GetXmlNodeValue("@B");
            capabilities.VersionNumber = byte.Parse(node.GetXmlNodeValue("@C"));
            capabilities.ProvidedCapabilities = byte.Parse(node.GetXmlNodeValue("@D"));
            capabilities.NumberOfBoomsSectionControl = byte.Parse(node.GetXmlNodeValue("@E"));
            capabilities.NumberOfSectionsSectionControl = byte.Parse(node.GetXmlNodeValue("@F"));
            capabilities.NumberOfControlChannels = byte.Parse(node.GetXmlNodeValue("@G"));
            return capabilities;
        }

        public static IEnumerable<ISOElement> ReadXML(XmlNodeList nodes)
        {
            List<ISOTaskControllerCapabilities> items = new List<ISOTaskControllerCapabilities>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOTaskControllerCapabilities.ReadXML(node));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireString(this, x => x.TaskControllerControlFunctionNAME, 16, errors, "A"); //Hex validation could be improved upon
            RequireString(this, x => x.TaskControllerDesignator, 153, errors, "B");
            RequireRange<ISOTaskControllerCapabilities, byte>(this, x => x.VersionNumber, 0, 4, errors, "C");
            RequireRange<ISOTaskControllerCapabilities, byte>(this, x => x.ProvidedCapabilities, 0, 63, errors, "D");
            RequireRange<ISOTaskControllerCapabilities, byte>(this, x => x.NumberOfBoomsSectionControl, 0, 254, errors, "E");
            RequireRange<ISOTaskControllerCapabilities, byte>(this, x => x.NumberOfSectionsSectionControl, 0, 254, errors, "F");
            RequireRange<ISOTaskControllerCapabilities, byte>(this, x => x.NumberOfControlChannels, 0, 254, errors, "G");
            return errors;
        }
    }
}
