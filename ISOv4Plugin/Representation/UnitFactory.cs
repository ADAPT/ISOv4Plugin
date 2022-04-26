/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public class UnitFactory
    {
        private readonly SortedList<Interval, ISOUnit> _units = new SortedList<Interval, ISOUnit>();

        private static UnitFactory _instance;
        private static readonly object InstanceLock = new object();

        public static readonly Dictionary<UnitOfMeasureDimensionEnum, int> DimensionToDdi =
            new Dictionary<UnitOfMeasureDimensionEnum, int>
            {
                { UnitOfMeasureDimensionEnum.VolumePerArea, 1 },
                { UnitOfMeasureDimensionEnum.MassPerArea, 6 },
                { UnitOfMeasureDimensionEnum.CountPerArea, 11 },
                { UnitOfMeasureDimensionEnum.SeedsPerArea, 11 },
                { UnitOfMeasureDimensionEnum.VolumePerVolume, 21 },
                { UnitOfMeasureDimensionEnum.MassPerMass, 26 },
                { UnitOfMeasureDimensionEnum.VolumePerMass, 31 },
                { UnitOfMeasureDimensionEnum.VolumePerTime, 36 },
                { UnitOfMeasureDimensionEnum.MassPerTime, 41 },
                { UnitOfMeasureDimensionEnum.PerTime, 46 },
                { UnitOfMeasureDimensionEnum.Mass, 74 },
                { UnitOfMeasureDimensionEnum.Count, 77 },
                { UnitOfMeasureDimensionEnum.Seeds, 77 },
                { UnitOfMeasureDimensionEnum.PerVolume, 89 },
                { UnitOfMeasureDimensionEnum.Volume, 89 }
            };

        private class Interval : IComparable<Interval>
        {
            public int Min;
            public int Max;

            public int CompareTo(Interval other)
            {
                if (other == null)
                    return 1;
                if (Max < other.Min)
                    return -1;
                if (Min > other.Max)
                    return 1;
                return 0;
            }
        }

        private UnitFactory()
        {
            _units[new Interval { Min = 1, Max = 5 }] = new ISOUnit("l1ha-1", 0.0001, 0);           //0x01 - 0x05
            _units[new Interval { Min = 6, Max = 10 }] = new ISOUnit("kg1ha-1", 0.01, 0);           //0x06 - 0x0A
            _units[new Interval { Min = 11, Max = 15 }] = new ISOUnit("count1[m2]-1", 0.001, 0);    //0x0B - 0x0F
            _units[new Interval { Min = 16, Max = 20 }] = new ISOUnit("m", 0.001, 0);               //0x10 - 0x14
            _units[new Interval { Min = 21, Max = 25 }] = new ISOUnit("ml1[m3]-1", 0.001, 0);       //0x15 - 0x19
            _units[new Interval { Min = 26, Max = 30 }] = new ISOUnit("g1kg-1", 0.001, 0);          //0x1A - 0x1E
            _units[new Interval { Min = 31, Max = 35 }] = new ISOUnit("ml1kg-1", 0.001, 0);         //0x1F - 0x23
            _units[new Interval { Min = 36, Max = 40 }] = new ISOUnit("ml1sec-1", 0.001, 0);        //0x24 - 0x28
            _units[new Interval { Min = 41, Max = 45 }] = new ISOUnit("g1sec-1", 0.001, 0);         //0x29 - 0x2D
            _units[new Interval { Min = 46, Max = 50 }] = new ISOUnit("count1s-1", 0.001, 0);       //0x2E - 0x32
            _units[new Interval { Min = 51, Max = 70 }] = new ISOUnit("m", 0.001, 0);               //0x33 - 0x46
            _units[new Interval { Min = 71, Max = 73 }] = new ISOUnit("l", 0.001, 0);               //0x47 - 0x49
            _units[new Interval { Min = 74, Max = 76 }] = new ISOUnit("kg", 0.001, 0);              //0x4A - 0x4C
            _units[new Interval { Min = 77, Max = 79 }] = new ISOUnit("count", 1, 0);               //0x4D - 0x4F
            _units[new Interval { Min = 80, Max = 80 }] = new ISOUnit("l", 1, 0);                   //0x50
            _units[new Interval { Min = 81, Max = 81 }] = new ISOUnit("kg", 1, 0);                  //0x51
            _units[new Interval { Min = 82, Max = 82 }] = new ISOUnit("count", 1, 0);               //0x52
            _units[new Interval { Min = 83, Max = 83 }] = new ISOUnit("ml1[m2]-1", 1, 0);           //0x53
            _units[new Interval { Min = 84, Max = 84 }] = new ISOUnit("kg1ha-1", 0.01, 0);          //0x54
            _units[new Interval { Min = 85, Max = 85 }] = new ISOUnit("count1[m2]-1", 0.001, 0);    //0x55
            _units[new Interval { Min = 86, Max = 86 }] = new ISOUnit("ml1sec-1", 1, 0);            //0x56
            _units[new Interval { Min = 87, Max = 87 }] = new ISOUnit("kg1hr-1", 0.0036, 0);        //0x57
            _units[new Interval { Min = 88, Max = 88 }] = new ISOUnit("count1sec-1", 0.001, 0);     //0x58
            _units[new Interval { Min = 89, Max = 89 }] = new ISOUnit("l", 1, 0);                   //0x59
            _units[new Interval { Min = 90, Max = 90 }] = new ISOUnit("kg", 1, 0);                  //0x5A
            _units[new Interval { Min = 91, Max = 91 }] = new ISOUnit("count", 1, 0);               //0x5B
            _units[new Interval { Min = 92, Max = 92 }] = new ISOUnit("ml1[m2]-1", 1, 0);           //0x5C
            _units[new Interval { Min = 93, Max = 93 }] = new ISOUnit("kg1ha-1", 0.01, 0);          //0x5D
            _units[new Interval { Min = 94, Max = 94 }] = new ISOUnit("count1[m2]-1", 0.001, 0);    //0x5E
            _units[new Interval { Min = 95, Max = 95 }] = new ISOUnit("ml1sec-1", 1, 0);            //0x5F
            _units[new Interval { Min = 96, Max = 96 }] = new ISOUnit("kg1hr-1", 0.0036, 0);        //0x60
            _units[new Interval { Min = 97, Max = 97 }] = new ISOUnit("count1sec-1", 0.001, 0);     //0x61
            _units[new Interval { Min = 98, Max = 100 }] = new ISOUnit("prcnt", 0.0001, 0);         //0x62 - 0x64
            _units[new Interval { Min = 101, Max = 115 }] = new ISOUnit("m", 0.001, 0);             //0x65 - 0x73
            _units[new Interval { Min = 116, Max = 116 }] = new ISOUnit("m2", 1, 0);                //0x74
            _units[new Interval { Min = 117, Max = 118 }] = new ISOUnit("m", 0.001, 0);             //0x75 - 0x76
            _units[new Interval { Min = 119, Max = 120 }] = new ISOUnit("sec", 1, 0);               //0x77 - 0x78
            _units[new Interval { Min = 121, Max = 121 }] = new ISOUnit("mg1l-1", 1, 0);            //0x79
            _units[new Interval { Min = 122, Max = 122 }] = new ISOUnit("g1count-1", 1, 0);         //0x7A
            _units[new Interval { Min = 123, Max = 123 }] = new ISOUnit("ml1count-1", 0.001, 0);    //0x7B
            _units[new Interval { Min = 124, Max = 125 }] = new ISOUnit("prcnt", 0.1, 0);           //0x7C - 0x7D
            _units[new Interval { Min = 126, Max = 129 }] = new ISOUnit("sec", 0.001, 0);           //0x7E - 0x81
            _units[new Interval { Min = 130, Max = 130 }] = new ISOUnit("prcnt", 0.01, 0);          //0x82
            _units[new Interval { Min = 131, Max = 133 }] = new ISOUnit("count", 1, 0);             //0x83 - 0x85
            _units[new Interval { Min = 134, Max = 136 }] = new ISOUnit("m", 0.001, 0);             //0x86 - 0x88
            _units[new Interval { Min = 137, Max = 137 }] = new ISOUnit("ml", 1, 0);                //0x89
            _units[new Interval { Min = 138, Max = 138 }] = new ISOUnit("g", 1, 0);                 //0x8A
            _units[new Interval { Min = 139, Max = 139 }] = new ISOUnit("count", 1, 0);             //0x8B
            _units[new Interval { Min = 140, Max = 140 }] = new ISOUnit("ppm", 1, 0);               //0x8C
            _units[new Interval { Min = 142, Max = 143 }] = new ISOUnit("sec", 0.001, 0);           //0x8E - 0x8F
            _units[new Interval { Min = 144, Max = 146 }] = new ISOUnit("arcdeg", 0.001, 0);        //0x90 - 0x92
            _units[new Interval { Min = 147, Max = 147 }] = new ISOUnit("count", 1, 0);             //0x93
            _units[new Interval { Min = 148, Max = 148 }] = new ISOUnit("ml", 1, 0);                //0x94
            _units[new Interval { Min = 149, Max = 149 }] = new ISOUnit("l1hr-1", 0.0036, 0);       //0x95
            _units[new Interval { Min = 150, Max = 150 }] = new ISOUnit("l1ha-1", 0.01, 0);         //0x96
            _units[new Interval { Min = 151, Max = 151 }] = new ISOUnit("cm2sec-1", 0.01, 0);       //0x97
            _units[new Interval { Min = 152, Max = 152 }] = new ISOUnit("ml1[m3]-1", 0.00001, 0);   //0x98

            _units[new Interval { Min = 396, Max = 399 }] = new ISOUnit("mm1s-1", 1, 0);            //0x18C - 0x18F
        }


        public static UnitFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                            _instance = new UnitFactory();
                    }
                }
                return _instance;
            }
        }

        public ISOUnit GetUnitByDDI(int ddi)
        {
            var isoUnit = _units.FirstOrDefault(u => (u.Key.Min <= ddi && u.Key.Max >= ddi));
            if (isoUnit.Value != null)
                return isoUnit.Value;
            return null;
        }

        public ISOUnit GetUnitByDDIString(string ddiHexString)
        {
            return GetUnitByDDI(ddiHexString.AsInt32DDI());
        }

        public ISOUnit GetUnitByDimension(UnitOfMeasureDimensionEnum dimension)
        {
            if (DimensionToDdi.ContainsKey(dimension))
                return GetUnitByDDI(DimensionToDdi[dimension]);

            return null;
        }
    }
}
