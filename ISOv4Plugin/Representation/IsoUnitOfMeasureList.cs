/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.Resources;

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    public static class IsoUnitOfMeasureList
    {
        private static readonly List<DdiToUnitOfMeasure> IsoMappings;

        static IsoUnitOfMeasureList()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(DdiToUnitOfMeasureMapping));
            using (var stringReader = new StringReader(Resource.IsoUnitOfMeasure))
                IsoMappings = ((DdiToUnitOfMeasureMapping)serializer.Deserialize(stringReader)).Mappings.ToList();
        }

        public static List<DdiToUnitOfMeasure> Mappings
        {
            get
            {
                return IsoMappings;
            }
        }
    }
}
