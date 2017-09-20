/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml.Serialization;

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    [XmlRoot("ddiToUnitOfMeasureMapping")]
    public class DdiToUnitOfMeasureMapping
    {
        [XmlArray("mappings")]
        [XmlArrayItem("mapping", typeof(DdiToUnitOfMeasure))]
        public DdiToUnitOfMeasure[] Mappings { get; set; }
    }

    public class DdiToUnitOfMeasure
    {
        [XmlAttribute("unit")]
        public string Unit { get; set; }

        [XmlAttribute("domainId")]
        public string AdaptCode { get; set; }

        [XmlAttribute("operationType")]
        public string OperationType { get; set; }
    }
}
