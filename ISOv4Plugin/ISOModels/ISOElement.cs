/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public abstract class ISOElement
    {
        public abstract XmlWriter WriteXML(XmlWriter xmlBuilder);
    }
}
