using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Readers
{
    public interface ITaskDataReader
    {
        ISO11783_TaskData Read(XPathNavigator navigator, string path);
    }

    public class TaskDataReader : ITaskDataReader
    {
        private const string TaskData = "ISO11783_TaskData";
        private const string VersionMajor = "VersionMajor";
        private const string VersionMinor = "VersionMinor";
        private const string ManagementSoftwareManufacturer = "ManagementSoftwareManufacturer";
        private const string ManagementSoftwareVersion = "ManagementSoftwareVersion";
        private const string TaskControllerManufacturer = "TaskControllerManufacturer";
        private const string TaskControllerVersion = "TaskControllerVersion";
        private const string DataTransferOrigin = "DataTransferOrigin";
        private const string TSK = "TSK";
        private const string XFR = "XFR";
        private const string XFC = "XFC";

        private readonly ITsksReader _tsksReader;

        public TaskDataReader() :this(new TsksReader())
        {
            
        }

        public TaskDataReader(ITsksReader tsksReader)
        {
            _tsksReader = tsksReader;
        }

        public ISO11783_TaskData Read(XPathNavigator navigator, string path)
        {
            var taskDataNode = navigator.SelectSingleNode(TaskData);
            return new ISO11783_TaskData
            {
                ManagementSoftwareManufacturer = ReadValue(taskDataNode, ManagementSoftwareManufacturer),
                ManagementSoftwareVersion = ReadValue(taskDataNode, ManagementSoftwareVersion),
                TaskControllerManufacturer = ReadValue(taskDataNode, TaskControllerManufacturer),
                TaskControllerVersion = ReadValue(taskDataNode, TaskControllerVersion),
                VersionMajor = int.Parse(ReadValue(taskDataNode, VersionMajor)),
                VersionMinor = int.Parse(ReadValue(taskDataNode, VersionMinor)),
                DataTransferOrigin = ReadEnumValue(taskDataNode, DataTransferOrigin),
                Items = GetItems(taskDataNode, path),
            };
        }

        private ISO11783_TaskDataDataTransferOrigin ReadEnumValue(XPathNavigator taskDataNode, string attribute)
        {
            var value = taskDataNode.GetAttribute(attribute, taskDataNode.NamespaceURI);
            ISO11783_TaskDataDataTransferOrigin outValue;
            Enum.TryParse(value, out outValue);
            return outValue;
        }

        private IWriter[] GetItems(XPathNavigator navigator, string path)
        {
            var children = navigator.SelectChildren(XPathNodeType.Element);

            var items = new List<IWriter>();
            
            items.AddRange(ReadTsks(children));
            items.AddRange(ReadExternalTsks(children, path));

            return items.ToArray();
        }

        private IEnumerable<IWriter> ReadExternalTsks(XPathNodeIterator children, string path)
        {
            var returnItems = new List<IWriter>();

            var externalChildren = children.Current.Select("./" + XFR);
            if (externalChildren.Count != 0)
            {
                var externalTasks = GetExternalTasks(path, externalChildren);
                returnItems.AddRange(externalTasks);
            }

            return returnItems;
        }

        private IEnumerable<IWriter> ReadTsks(XPathNodeIterator children)
        {
            var tsks = children.Current.Select("./" + TSK);
            return _tsksReader.Read(tsks);
        }

        private List<TSK> GetExternalTasks(string path, XPathNodeIterator externalChildren)
        {
            var items = new List<TSK>();
            foreach (XPathNavigator node in externalChildren)
            {
                var attribute = node.GetAttribute("A", node.NamespaceURI);
                if (attribute.StartsWith("TSK"))
                {
                    var filename = Path.Combine(path, attribute + ".xml");
                    
                    if(!File.Exists(filename))
                        continue;

                    var externalTaskElements = new XPathDocument(filename).CreateNavigator()
                        .SelectSingleNode(XFC)
                        .SelectChildren(XPathNodeType.Element)
                        .Current.Select("./" + TSK);

                    var externalTsks = _tsksReader.Read(externalTaskElements);
                    items.AddRange(externalTsks);
                }
            }

            return items;
        }

        private string ReadValue(XPathNavigator navigator, string element)
        {
            return navigator.GetAttribute(element, navigator.NamespaceURI);
        }
    }
}
