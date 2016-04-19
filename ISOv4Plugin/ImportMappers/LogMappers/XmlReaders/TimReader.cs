using System.Linq;
using System.Xml.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface ITimReader
    {
        TIMHeader Read(XDocument xDocument);
    }

    public class TimReader : ITimReader
    {
        private readonly IPtnReader _ptnReader;
        private readonly IDlvReader _dlvReader;

        public TimReader() : this(new PtnReader(), new DlvReader())
        {
            
        }

        public TimReader(IPtnReader ptnReader, IDlvReader dlvReader)
        {
            _ptnReader = ptnReader;
            _dlvReader = dlvReader;
        }

        public TIMHeader Read(XDocument xDocument)
        {
            var timElement = xDocument.Descendants("TIM").FirstOrDefault();

            if (timElement == null)
                return null;

            var ptnElement = timElement.Descendants("PTN").FirstOrDefault();
            var dlvElements = xDocument.Descendants("DLV").ToList();

            return new TIMHeader
            {
                Start = new HeaderProperty(timElement.Attribute("A")),
                Stop = new HeaderProperty(timElement.Attribute("B")),
                Duration = new HeaderProperty(timElement.Attribute("C")),
                Type = new HeaderProperty(timElement.Attribute("D")),
                PtnHeader = _ptnReader.Read(ptnElement),
                DLVs = _dlvReader.Read(dlvElements).ToList(),
            };
        }
    }
}
