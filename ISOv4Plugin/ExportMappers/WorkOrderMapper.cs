using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IWorkOrderMapper
    {

        //TODO
        //List<MSG> Export(WorkOrders workOrders);
    }

    public class WorkOrderMapper : IWorkOrderMapper
    {

        //TODO
//        private int _workItemIndex = 0;

//        public List<MSG> Export(WorkOrders workOrders)
//        {
//            if (workOrders == null || !workOrders.Any())
//                return new List<MSG>();

//            int workOrderIndex = 0;
//            return workOrders.Select(x => Export(x, workOrderIndex++)).ToList();
//        }

//        private MSG Export(WorkOrder workOrder, int messageNumber)
//        {
//            var message = new MSG
//            {
//                B = MSGB.Item3, //MessageType.WorkOrder
//                G = workOrder.Notes
//            };
//            message.A = message.GetIsoId(messageNumber);
////            message.Items = Export(workOrder.WorkItems);
//            return message;
//        }
    }
}
