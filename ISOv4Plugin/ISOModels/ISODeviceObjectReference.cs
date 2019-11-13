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
    public class ISODeviceObjectReference : ISOElement
    {
        //Attributes
        public uint DeviceObjectId  { get; set; }
        
        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("DOR");
            xmlBuilder.WriteXmlAttribute<uint>("A", DeviceObjectId );
            base.WriteXML(xmlBuilder);
            xmlBuilder.WriteEndElement();
            return xmlBuilder;
        }

        public static ISODeviceObjectReference ReadXML(XmlNode node)
        {
            ISODeviceObjectReference item = new ISODeviceObjectReference();
            item.DeviceObjectId = node.GetXmlNodeValueAsUInt("@A");
            return item;
        }

        public static IEnumerable<ISODeviceObjectReference> ReadXML(XmlNodeList nodes)
        {
            List<ISODeviceObjectReference> items = new List<ISODeviceObjectReference>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISODeviceObjectReference.ReadXML(node));
            }
            return items;
        }

        public override List<IError> Validate(List<IError> errors)
        {
            RequireRange<ISODeviceObjectReference, uint>(this, x => x.DeviceObjectId, 1, 65534, errors, "A");
            return errors;
        }
    }
}
