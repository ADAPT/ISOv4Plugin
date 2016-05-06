using System.Collections.Generic;
using System.Linq;
using System.Xml;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Loaders
{
    public class TaskLoader
    {
        private XmlNode _rootNode;
        private string _baseFolder;
        private TaskDataDocument _taskDocument;
        private List<LoggedData> _tasks;

        private TaskLoader(TaskDataDocument taskDocument)
        {
            _taskDocument = taskDocument;
            _rootNode = _taskDocument.RootNode;
            _baseFolder = _taskDocument.BaseFolder;
            _tasks = new List<LoggedData>();
        }

        public static List<LoggedData> Load(TaskDataDocument taskDocument)
        {
            var loader = new TaskLoader(taskDocument);

            return loader.Load();
        }

        private List<LoggedData> Load()
        {
            LoadTasks(_rootNode.SelectNodes("TSK"));
            ProcessExternalNodes();

            return _tasks;
        }

        private void ProcessExternalNodes()
        {
            var externalNodes = _rootNode.SelectNodes("XFR[starts-with(@A, 'TSK')]");
            foreach (XmlNode externalNode in externalNodes)
            {
                var inputNodes = externalNode.LoadActualNodes("XFR", _baseFolder);
                if (inputNodes == null)
                    continue;
                LoadTasks(inputNodes);
            }
        }

        private void LoadTasks(XmlNodeList inputNodes)
        {
            foreach (XmlNode inputNode in inputNodes)
            {
                var task = LoadTask(inputNode);
                if (task != null)
                    _tasks.Add(task);
            }
        }

        private LoggedData LoadTask(XmlNode inputNode)
        {
            var task = new LoggedData();

            // Required fields. Do not proceed if they are missing
            var taskId = inputNode.GetXmlNodeValue("@A");
            if (taskId == null)
                return null;

            // Optional fields
            task.Description = inputNode.GetXmlNodeValue("@B");

            LoadField(inputNode.GetXmlNodeValue("@E"), task);
            LoadFarm(inputNode.GetXmlNodeValue("@D"), task);
            LoadCustomer(inputNode.GetXmlNodeValue("@C"), task);

            LoadGuidanceAllocations(inputNode, task);
            LoadCommentAllocations(inputNode, task);
            task.Id.UniqueIds.Add(new UniqueId
            {
                Id = taskId,
                Source = UniqueIdMapper.IsoSource,
                CiTypeEnum = CompoundIdentifierTypeEnum.String,
            });

            _taskDocument.LoadLinkedIds(taskId, task.Id);
            return task;
        }

        private void LoadField(string fieldId, LoggedData task)
        {
            var field = _taskDocument.Fields.FindById(fieldId);
            if (field != null)
            {
                task.FieldId = field.Id.ReferenceId;
                task.FarmId = field.FarmId;
            }
        }

        private void LoadFarm(string farmId, LoggedData task)
        {
            var farm = _taskDocument.Farms.FindById(farmId);
            if (farm != null)
            {
                task.FarmId = farm.Id.ReferenceId;
                task.GrowerId = farm.GrowerId;
            }
        }

        private void LoadCustomer(string customerId, LoggedData task)
        {
            var customer = _taskDocument.Customers.FindById(customerId);
            if (customer != null)
                task.GrowerId = customer.Id.ReferenceId;
        }

        private void LoadGuidanceAllocations(XmlNode inputNode, LoggedData task)
        {
            var allocations = GuidanceAllocationLoader.Load(inputNode, _taskDocument);

            _taskDocument.GuidanceAllocations.AddRange(allocations);

            task.GuidanceAllocationIds = allocations.Select(x => x.Id.ReferenceId).ToList();
        }

        private void LoadCommentAllocations(XmlNode inputNode, LoggedData task)
        {
            task.Notes = CommentAllocationLoader.Load(inputNode, _taskDocument);
        }
    }
}
