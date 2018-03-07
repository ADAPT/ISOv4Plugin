/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    public static class IsoUnitOfMeasureList
    {
        private static readonly List<DdiToUnitOfMeasure> IsoMappings;

        static IsoUnitOfMeasureList()
        {
            var isoUnitOfMeasureFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "IsoUnitOfMeasure.xml");

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(DdiToUnitOfMeasureMapping));
            using (var stringReader = new StringReader(File.ReadAllText(isoUnitOfMeasureFile)))
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
