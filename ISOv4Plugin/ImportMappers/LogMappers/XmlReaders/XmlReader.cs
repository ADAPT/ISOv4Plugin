using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        List<TIM> ReadTlgXmlData(string dataPath, string fileName);
        void Write(string dataPath, ISO11783_TaskData taskData);
        XDocument WriteTlgXmlData(string datacardPath, string fileName, TIM timHeader);
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

            return _taskDataReader.Read(navigator, Path.GetDirectoryName(file));
        }

        public List<TIM> ReadTlgXmlData(string dataPath, string fileName)
        {
            var file = Path.Combine(dataPath, fileName);
            var xPathDocument = new XPathDocument(file);

            return _timReader.Read(xPathDocument);
        }

        public void Write(string dataPath, ISO11783_TaskData taskData)
        {

        }

        public XDocument WriteTlgXmlData(string datacardPath, string fileName, TIM timHeader)
        {
            var filePath = Path.Combine(datacardPath, fileName);
            var timElement = CreateTimElement(timHeader);
            var xdoc = new XDocument(timElement);

            xdoc.Save(filePath);

            return xdoc;
        }

        private static XElement CreateTimElement(TIM tim)
        {
            var dlvElements = CreateDlvElements(tim);

            var ptn = tim.Items.FirstOrDefault(x => x.GetType() == typeof (PTN)) as PTN;
            var ptnElement = CreatePtnElement(ptn);

            var timElement = new XElement("TIM", ptnElement, dlvElements);

            SetAttribute(timElement, tim.ASpecified, "A", tim.A);
            SetAttribute(timElement, tim.BSpecified, "B", tim.B);
            SetAttribute(timElement, tim.CSpecified, "C", tim.C);
            SetAttribute(timElement, tim.DSpecified, "D", tim.D == null ? null : (int?)tim.D);

            return timElement;
        }

        private static void SetAttribute<T>(XElement timElement, bool specified, string attributeName, T attributeValue)
        {
            if (specified)
            {
                if(attributeValue == null)
                    timElement.SetAttributeValue(attributeName, "");
                else
                    timElement.SetAttributeValue(attributeName, attributeValue.ToString());
            }
        }

        private static XElement CreatePtnElement(PTN ptn)
        {
            if(ptn == null)
                return null;

            var ptnElement = new XElement("PTN");

            SetAttribute(ptnElement, ptn.ASpecified, "A", ptn.A);
            SetAttribute(ptnElement, ptn.BSpecified, "B", ptn.B);
            SetAttribute(ptnElement, ptn.CSpecified, "C", ptn.C);
            SetAttribute(ptnElement, ptn.DSpecified, "D", ptn.D);
            SetAttribute(ptnElement, ptn.ESpecified, "E", ptn.E);
            SetAttribute(ptnElement, ptn.FSpecified, "F", ptn.F);
            SetAttribute(ptnElement, ptn.GSpecified, "G", ptn.G);
            SetAttribute(ptnElement, ptn.HSpecified, "H", ptn.H);
            SetAttribute(ptnElement, ptn.ISpecified, "I", ptn.I);

            return ptnElement;
        }

        private static IEnumerable<XElement> CreateDlvElements(TIM tim)
        {
            var dlvs = tim.Items.Where(x => x.GetType() == typeof (DLV)).Cast<DLV>().ToList();

            if (dlvs.Any())
            {
                return dlvs.Select(x =>
                {
                    var dlvElement = new XElement("DLV");
                    var ddi = x.A;
                    if (String.IsNullOrEmpty(ddi))
                        dlvElement.SetAttributeValue("A", ddi);
                    else
                    {
                        var value = int.Parse(ddi).ToString("X4");
                        dlvElement.SetAttributeValue("A", value);
                    }
                    dlvElement.SetAttributeValue("B", x.B);
                    dlvElement.SetAttributeValue("C", x.C);
                    dlvElement.SetAttributeValue("D", x.D);
                    dlvElement.SetAttributeValue("E", x.E);
                    dlvElement.SetAttributeValue("F", x.F);
                    return dlvElement;
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
