using System;
using System.Collections.Generic;

namespace AgGateway.ADAPT.IsoPlugin
{
    internal class UnitFactory
    {
        private SortedList<Interval, IsoUnit> _units = new SortedList<Interval, IsoUnit>();

        private static UnitFactory _instance;
        private static readonly object _instanceLock = new object();

        private class Interval : IComparable<Interval>
        {
            public int min;
            public int max;

            public int CompareTo(Interval other)
            {
                if (other == null)
                    return 1;
                if (max < other.min)
                    return -1;
                if (min > other.max)
                    return 1;
                return 0;
            }
        }

        private UnitFactory()
        {
            _units[new Interval { min = 1, max = 5 }] = new IsoUnit("l1[ha]-1", 0.0001, 0);         //0x01 - 0x05
            _units[new Interval { min = 6, max = 10 }] = new IsoUnit("kg1[ha]-1", 0.01, 0);         //0x06 - 0x0A
            _units[new Interval { min = 11, max = 15 }] = new IsoUnit("count1[m2]-1", 0.001, 0);    //0x0B - 0x0F
            _units[new Interval { min = 16, max = 20 }] = new IsoUnit("m", 0.001, 0);               //0x10 - 0x14
            _units[new Interval { min = 21, max = 25 }] = new IsoUnit("ml1[m3]-1", 0.001, 0);       //0x15 - 0x19
            _units[new Interval { min = 26, max = 30 }] = new IsoUnit("g1[kg]-1", 0.001, 0);        //0x1A - 0x1E
            _units[new Interval { min = 31, max = 35 }] = new IsoUnit("ml1[kg]-1", 0.001, 0);       //0x1F - 0x23
            _units[new Interval { min = 36, max = 40 }] = new IsoUnit("ml1[sec]-1", 0.001, 0);      //0x24 - 0x28
            _units[new Interval { min = 41, max = 45 }] = new IsoUnit("g1[sec]-1", 0.001, 0);       //0x29 - 0x2D
            _units[new Interval { min = 46, max = 50 }] = new IsoUnit("count1[s]-1", 0.001, 0);     //0x2E - 0x32
            _units[new Interval { min = 51, max = 70 }] = new IsoUnit("m", 0.001, 0);               //0x33 - 0x46
            _units[new Interval { min = 71, max = 73 }] = new IsoUnit("l", 0.001, 0);               //0x47 - 0x49
            _units[new Interval { min = 74, max = 76 }] = new IsoUnit("kg", 0.001, 0);              //0x4A - 0x4C
            _units[new Interval { min = 77, max = 79 }] = new IsoUnit("count", 1, 0);               //0x4D - 0x4F
            _units[new Interval { min = 80, max = 80 }] = new IsoUnit("l", 1, 0);                   //0x50
            _units[new Interval { min = 81, max = 81 }] = new IsoUnit("kg", 1, 0);                  //0x51
            _units[new Interval { min = 82, max = 82 }] = new IsoUnit("count", 1, 0);               //0x52
            _units[new Interval { min = 83, max = 83 }] = new IsoUnit("ml1[m2]-1", 1, 0);           //0x53
            _units[new Interval { min = 84, max = 84 }] = new IsoUnit("kg1[ha]-1", 0.01, 0);        //0x54
            _units[new Interval { min = 85, max = 85 }] = new IsoUnit("count1[m2]-1", 0.001, 0);    //0x55
            _units[new Interval { min = 86, max = 86 }] = new IsoUnit("ml1[sec]-1", 1, 0);          //0x56
            _units[new Interval { min = 87, max = 87 }] = new IsoUnit("kg1[hr]-1", 0.0036, 0);      //0x57
            _units[new Interval { min = 88, max = 88 }] = new IsoUnit("count1[sec]-1", 0.001, 0);   //0x58
            _units[new Interval { min = 89, max = 89 }] = new IsoUnit("l", 1, 0);                   //0x59
            _units[new Interval { min = 90, max = 90 }] = new IsoUnit("kg", 1, 0);                  //0x5A
            _units[new Interval { min = 91, max = 91 }] = new IsoUnit("count", 1, 0);               //0x5B
            _units[new Interval { min = 92, max = 92 }] = new IsoUnit("ml1[m2]-1", 1, 0);           //0x5C
            _units[new Interval { min = 93, max = 93 }] = new IsoUnit("kg1[ha]-1", 0.01, 0);        //0x5D
            _units[new Interval { min = 94, max = 94 }] = new IsoUnit("count1[m2]-1", 0.001, 0);    //0x5E
            _units[new Interval { min = 95, max = 95 }] = new IsoUnit("ml1[sec]-1", 1, 0);          //0x5F
            _units[new Interval { min = 96, max = 96 }] = new IsoUnit("kg1[hr]-1", 0.0036, 0);      //0x60
            _units[new Interval { min = 97, max = 97 }] = new IsoUnit("count1[sec]-1", 0.001, 0);   //0x61
            _units[new Interval { min = 98, max = 100 }] = new IsoUnit("prcnt", 0.0001, 0);         //0x62 - 0x64
            _units[new Interval { min = 101, max = 115 }] = new IsoUnit("m", 0.001, 0);             //0x65 - 0x73
            _units[new Interval { min = 116, max = 116 }] = new IsoUnit("m2", 1, 0);                //0x74
            _units[new Interval { min = 117, max = 118 }] = new IsoUnit("m", 0.001, 0);             //0x75 - 0x76
            _units[new Interval { min = 119, max = 120 }] = new IsoUnit("sec", 1, 0);               //0x77 - 0x78
            _units[new Interval { min = 121, max = 121 }] = new IsoUnit("g1[l]-1", 1, 0);           //0x79
            _units[new Interval { min = 122, max = 122 }] = new IsoUnit("g1[count]-1", 1, 0);       //0x7A
            _units[new Interval { min = 123, max = 123 }] = new IsoUnit("ml1[count]-1", 0.001, 0);  //0x7B
            _units[new Interval { min = 124, max = 125 }] = new IsoUnit("prcnt", 0.1, 0);           //0x7C - 0x7D
            _units[new Interval { min = 126, max = 129 }] = new IsoUnit("sec", 0.001, 0);           //0x7E - 0x81
            _units[new Interval { min = 130, max = 130 }] = new IsoUnit("prcnt", 0.01, 0);          //0x82
            _units[new Interval { min = 131, max = 133 }] = new IsoUnit("count", 1, 0);             //0x83 - 0x85
            _units[new Interval { min = 134, max = 136 }] = new IsoUnit("m", 0.001, 0);             //0x86 - 0x88
            _units[new Interval { min = 137, max = 137 }] = new IsoUnit("ml", 1, 0);                //0x89
            _units[new Interval { min = 138, max = 138 }] = new IsoUnit("g", 1, 0);                 //0x8A
            _units[new Interval { min = 139, max = 139 }] = new IsoUnit("count", 1, 0);             //0x8B
            _units[new Interval { min = 140, max = 140 }] = new IsoUnit("ppm", 1, 0);               //0x8C
            _units[new Interval { min = 142, max = 143 }] = new IsoUnit("sec", 0.001, 0);           //0x8E - 0x8F
            _units[new Interval { min = 147, max = 147 }] = new IsoUnit("count", 1, 0);             //0x93
            _units[new Interval { min = 148, max = 148 }] = new IsoUnit("ml", 1, 0);                //0x94
            _units[new Interval { min = 149, max = 149 }] = new IsoUnit("l1[hr]-1", 0.0036, 0);     //0x95
            _units[new Interval { min = 150, max = 150 }] = new IsoUnit("l1[ha]-1", 0.01, 0);       //0x96
            _units[new Interval { min = 151, max = 151 }] = new IsoUnit("cm2[sec]-1", 0.01, 0);     //0x97
            _units[new Interval { min = 152, max = 152 }] = new IsoUnit("ml1[m3]-1", 0.00001, 0);   //0x98
        }

        public static UnitFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                            _instance = new UnitFactory();
                    }
                }
                return _instance;
            }
        }

        public IsoUnit GetUnitByDdi(int ddi)
        {
            var interval = new Interval { min = ddi, max = ddi };
            if (_units.ContainsKey(interval))
                return _units[interval];
            return null;
        }
    }
}
