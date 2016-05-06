using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Readers;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers.XmlReaders
{
    public interface IXmlReader
    {
        ISO11783_TaskData Read(string dataPath, string fileName);
        ISO11783_TaskData Read(string filePath);
        TIMHeader ReadTlgXmlData(string dataPath, string fileName);
        void Write(string dataPath, ISO11783_TaskData taskData);
        XDocument WriteTlgXmlData(string datacardPath, string fileName, TIMHeader timHeader);
    }

    public class XmlReader : IXmlReader
    {
        private readonly ITimReader _timReader;
        private readonly ITaskDataReader _taskDataReader;

        public XmlReader() : this(new TimReader(), new TaskDataReader())
        {
            
        }

        public XmlReader(ITimReader timReader, ITaskDataReader taskDataReader)
        {
            _timReader = timReader;
            _taskDataReader = taskDataReader;
        }

        public ISO11783_TaskData Read(string dataPath, string fileName)
        {
            var file = Path.Combine(dataPath, fileName);

            return Read(file);
        }

        public ISO11783_TaskData Read(string file)
        {
            var xpathReader = new XPathDocument(file);
            var navigator = xpathReader.CreateNavigator();

            return _taskDataReader.Read(navigator);
        }

        public TIMHeader ReadTlgXmlData(string dataPath, string fileName)
        {
            var file = Path.Combine(dataPath, fileName);

            using (var streamReader = new StreamReader(file))
            {
                var xDocument = XDocument.Load(streamReader);

                var tim = _timReader.Read(xDocument);

                return tim;
            }
        }

        public void Write(string dataPath, ISO11783_TaskData taskData)
        {

        }

        public XDocument WriteTlgXmlData(string datacardPath, string fileName, TIMHeader timHeader)
        {
            var filePath = Path.Combine(datacardPath, fileName);

            var timElement = CreateTimElement(timHeader);

            var xdoc = new XDocument(timElement);

            xdoc.Save(filePath);

            return xdoc;
        }

        private static XElement CreateTimElement(TIMHeader timHeader)
        {
            var dlvElements = CreateDlvElements(timHeader);
            var ptnElement = CreatePtnElement(timHeader.PtnHeader);

            var timElement = new XElement("TIM", ptnElement, dlvElements);

            timElement.SetAttributeValue("A", GetAttributeValue(timHeader.Start));
            timElement.SetAttributeValue("B", GetAttributeValue(timHeader.Stop));
            timElement.SetAttributeValue("C", GetAttributeValue(timHeader.Duration));
            timElement.SetAttributeValue("D", GetAttributeValue(timHeader.Type));

            return timElement;
        }

        private static XElement CreatePtnElement(PTNHeader ptnHeader)
        {
            if(ptnHeader == null)
                return null;

            var ptn = new XElement("PTN");

            ptn.SetAttributeValue("A", GetAttributeValue(ptnHeader.PositionNorth));
            ptn.SetAttributeValue("B", GetAttributeValue(ptnHeader.PositionEast));
            ptn.SetAttributeValue("C", GetAttributeValue(ptnHeader.PositionUp));
            ptn.SetAttributeValue("D", GetAttributeValue(ptnHeader.PositionStatus));
            ptn.SetAttributeValue("E", GetAttributeValue(ptnHeader.PDOP));
            ptn.SetAttributeValue("F", GetAttributeValue(ptnHeader.HDOP));
            ptn.SetAttributeValue("G", GetAttributeValue(ptnHeader.NumberOfSatellites));
            ptn.SetAttributeValue("H", GetAttributeValue(ptnHeader.GpsUtcTime));
            ptn.SetAttributeValue("I", GetAttributeValue(ptnHeader.GpsUtcDate));

            return ptn;
        }

        private static string GetAttributeValue(HeaderProperty property)
        {
            if (property == null || property.State == HeaderPropertyState.IsNull)
                return null;
            if(property.Value is TIMD)
                return property.State == HeaderPropertyState.IsEmpty ? "" : ((int)(TIMD)property.Value).ToString();
            return property.State == HeaderPropertyState.IsEmpty ? "" : property.Value.ToString();
        }

        private static IEnumerable<XElement> CreateDlvElements(TIMHeader timHeader)
        {
            if (timHeader.DLVs != null)
            {
                return timHeader.DLVs.Select(x =>
                {
                    var dlv = new XElement("DLV");
                    var ddi = GetAttributeValue(x.ProcessDataDDI);
                    if (String.IsNullOrEmpty(ddi))
                        dlv.SetAttributeValue("A", ddi);
                    else
                    {
                        var value = int.Parse(ddi).ToString("X4");
                        dlv.SetAttributeValue("A", value);
                    }
                    dlv.SetAttributeValue("B", GetAttributeValue(x.ProcessDataValue));
                    dlv.SetAttributeValue("C", GetAttributeValue(x.DeviceElementIdRef));
                    dlv.SetAttributeValue("D", GetAttributeValue(x.DataLogPGN));
                    dlv.SetAttributeValue("E", GetAttributeValue(x.DataLogPGNStartBit));
                    dlv.SetAttributeValue("F", GetAttributeValue(x.DataLogPGNStopBit));
                    return dlv;
                }).ToList();
            }
            return new List<XElement>();
        }

        public static T GetValue<T>(XAttribute attribute)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T) converter.ConvertFromString(attribute.Value);
            }
            catch (Exception)
            {
                if (!typeof(T).IsEnum)
                    return default(T);

                var values = Enum.GetValues(typeof (T));
                return (T)values.GetValue(0);
            }
        }
    }
}
