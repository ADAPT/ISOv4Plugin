using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IWorkOrderMapper
    {
        List<WorkOrder> Map(List<TSK> tasks, ApplicationDataModel.ADM.ApplicationDataModel dataModel);
    }

    public class WorkOrderMapper : IWorkOrderMapper
    {
        private readonly UniqueIdMapper _uniqueIdMapper;
        private readonly IStatusUpdateMapper _statusUpdateMapper;

        public WorkOrderMapper()
            : this(new StatusUpdateMapper())
        {
        }

        public WorkOrderMapper(IStatusUpdateMapper statusUpdateMapper)
        {
            _uniqueIdMapper = new UniqueIdMapper();
            _statusUpdateMapper = statusUpdateMapper;
        }

        public List<WorkOrder> Map(List<TSK> tasks, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {
            return tasks.Where(t => t != null).Select(t => Map(t, dataModel)).ToList();
        }

        private WorkOrder Map(TSK task, ApplicationDataModel.ADM.ApplicationDataModel dataModel)
        {
            var workOrder = new WorkOrder();
            workOrder.Id.UniqueIds.Add(_uniqueIdMapper.Map(task.A));
            workOrder.Description = task.B;
            workOrder.GrowerId = GetGrower(dataModel.Catalog, task.C);
            workOrder.FarmIds = GetFarms(dataModel.Catalog, task.D);
            workOrder.FieldIds = GetFields(dataModel.Catalog, task.E);
            workOrder.StatusUpdates = new List<StatusUpdate>();
            workOrder.StatusUpdates.Add(_statusUpdateMapper.Map(task.G));

            if (!string.IsNullOrEmpty(task.F))
            {
                AssociatePersonWithWorkOrder(task, dataModel, workOrder);
            }
            AssociateWorkItemWithWorkOrder(task, dataModel, workOrder);

            return workOrder;
        }

        private void AssociatePersonWithWorkOrder(TSK task, ApplicationDataModel.ADM.ApplicationDataModel dataModel, WorkOrder workOrder)
        {
            var person = dataModel.Catalog.Persons.SingleOrDefault(p => p.Id.FindIsoId() == task.F);
            if (person != null)
            {
                var personRole = new PersonRole {PersonId = person.Id.ReferenceId, Role = new EnumeratedValue{Representation = RepresentationInstanceList.dtPersonRole.ToModelRepresentation(), Value = DefinedTypeEnumerationInstanceList.dtiPersonRoleOperator.ToModelEnumMember()}};
                dataModel.Catalog.PersonRoles.Add(personRole);
                workOrder.PersonRoleIds = new List<int> {personRole.Id.ReferenceId};
            }
        }

        private static void AssociateWorkItemWithWorkOrder(TSK task, ApplicationDataModel.ADM.ApplicationDataModel dataModel, WorkOrder workOrder)
        {
            var fieldId = workOrder.FieldIds.FirstOrDefault();
            var workItem = new WorkItem
            {
                GrowerId = workOrder.GrowerId,
                FarmId = workOrder.FarmIds.FirstOrDefault(),
                FieldId = fieldId,
            };
            if (fieldId != 0)
            {
                var field = dataModel.Catalog.Fields.Single(f => f.Id.ReferenceId == fieldId);
                if (field.ActiveBoundaryId != 0)
                    workItem.BoundaryId = field.ActiveBoundaryId;
            }

            dataModel.Documents.WorkItems = dataModel.Documents.WorkItems.Concat(new[] { workItem });
            workOrder.WorkItemIds = new List<int> { workItem.Id.ReferenceId };

            if (task.Items != null && task.Items.OfType<TZN>().Any())
            {
                AssociateRxWithWorkItem(task, dataModel, workItem);
            }
        }

        private static void AssociateRxWithWorkItem(TSK task, ApplicationDataModel.ADM.ApplicationDataModel dataModel, WorkItem workItem)
        {
            var matchingRx = dataModel.Catalog.Prescriptions.SingleOrDefault(p => p.Id.FindIsoId() == task.A);
            if (matchingRx != null)
            {
                var operation = new WorkItemOperation();
                operation.PrescriptionId = matchingRx.Id.ReferenceId;
                dataModel.Documents.WorkItemOperations = dataModel.Documents.WorkItemOperations.Concat(new[] {operation});
                workItem.WorkItemOperationIds = new List<int> {operation.Id.ReferenceId};
            }
        }

        private List<int> GetFields(Catalog catalog, string fieldIsoId)
        {
            var field = catalog.Fields.SingleOrDefault(f => f.Id.FindIsoId() == fieldIsoId);

            return field != null ? new List<int> {field.Id.ReferenceId} : new List<int>();
        }

        private List<int> GetFarms(Catalog catalog, string farmIsoId)
        {
            var farm = catalog.Farms.SingleOrDefault(f => f.Id.FindIsoId() == farmIsoId);
            
            return farm != null ? new List<int>{ farm.Id.ReferenceId } : new List<int>();
        }

        private int? GetGrower(Catalog catalog, string growerIsoId)
        {
            var grower = catalog.Growers.SingleOrDefault(g => g.Id.FindIsoId() == growerIsoId);

            if (grower == null)
                return null;
            return grower.Id.ReferenceId;
        }
    }
}
