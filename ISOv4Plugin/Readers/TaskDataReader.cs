using System;
using System.Collections.Generic;
using System.Xml.XPath;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Readers
{
    public interface ITaskDataReader
    {
        ISO11783_TaskData Read(XPathNavigator navigator);
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

        private readonly ITsksReader _tsksReader;

        public TaskDataReader() :this(new TsksReader())
        {
            
        }

        public TaskDataReader(ITsksReader tsksReader)
        {
            _tsksReader = tsksReader;
        }

        public ISO11783_TaskData Read(XPathNavigator navigator)
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
                Items = GetItems(taskDataNode),
            };
        }

        private ISO11783_TaskDataDataTransferOrigin ReadEnumValue(XPathNavigator taskDataNode, string attribute)
        {
            var value = taskDataNode.GetAttribute(attribute, taskDataNode.NamespaceURI);
            ISO11783_TaskDataDataTransferOrigin outValue;
            Enum.TryParse(value, out outValue);
            return outValue;
        }

        private IWriter[] GetItems(XPathNavigator navigator)
        {
            var children = navigator.SelectChildren(XPathNodeType.Element);

            var tsks = _tsksReader.Read(children.Current.Select("./" + TSK));

            var items = new List<IWriter>();
            items.AddRange(tsks);

            return items.ToArray();
        }

        private string ReadValue(XPathNavigator navigator, string element)
        {
            return navigator.GetAttribute(element, navigator.NamespaceURI);
        }
    }
}
