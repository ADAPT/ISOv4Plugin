using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface IDlvReader
    {
        IEnumerable<DLVHeader> Read(List<XElement> dlvElement);
    }

    public class DlvReader : IDlvReader
    {
        public IEnumerable<DLVHeader> Read(List<XElement> dlvElements)
        {
            if(dlvElements == null)
                return null;
                
            return dlvElements.Select(Read);
        }

        private DLVHeader Read(XElement dlvElement)
        {
            return new DLVHeader
            {
                ProcessDataDDI = GetProcessDataDdi(dlvElement),
                ProcessDataValue = new HeaderProperty(dlvElement.Attribute("B")),
                DeviceElementIdRef = new HeaderProperty(dlvElement.Attribute("C")),
                DataLogPGN = new HeaderProperty(dlvElement.Attribute("D")),
                DataLogPGNStartBit = new HeaderProperty(dlvElement.Attribute("E")),
                DataLogPGNStopBit = new HeaderProperty(dlvElement.Attribute("F"))
            };
        }

        private static HeaderProperty GetProcessDataDdi(XElement dlvElement)
        {
            if(dlvElement.Attribute("A") == null)
                return new HeaderProperty{ State = HeaderPropertyState.IsNull };

            var hexDdi = dlvElement.Attribute("A").Value;
            var intDdi = int.Parse(hexDdi, System.Globalization.NumberStyles.HexNumber);

            return new HeaderProperty { State = HeaderPropertyState.HasValue, Value = intDdi };
        }
    }
}
