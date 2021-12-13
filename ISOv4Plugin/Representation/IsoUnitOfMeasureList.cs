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
        private static List<DdiToUnitOfMeasure> _isoMappings;

        private static string _isoUOMDataLocation = null;

        public static string ISOUOMDataFile
        {
            get
            {
                if (_isoUOMDataLocation == null)
                {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "IsoUnitOfMeasure.xml");
                }
                else
                {
                    return _isoUOMDataLocation;
                }
            }
            set { _isoUOMDataLocation = value; }
        }


        public static List<DdiToUnitOfMeasure> Mappings
        {
            get
            {
                if (_isoMappings == null)
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(DdiToUnitOfMeasureMapping));
                    using (var stringReader = new StringReader(File.ReadAllText(ISOUOMDataFile)))
                        _isoMappings = ((DdiToUnitOfMeasureMapping)serializer.Deserialize(stringReader)).Mappings.ToList();
                }
                return _isoMappings;
            }
        }
    }
}
