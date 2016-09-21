using AgGateway.ADAPT.ApplicationDataModel.Documents;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers.LogMappers
{
    public interface IStatusUpdateMapper
    {
        StatusUpdate Map(TSKG tskg);
    }

    public class StatusUpdateMapper : IStatusUpdateMapper
    {

        public StatusUpdate Map(TSKG tskg)
        {
            var status = WorkStatusEnum.Scheduled;

            if(tskg == TSKG.Item1)
                status = WorkStatusEnum.Scheduled;
            else if(tskg == TSKG.Item2)
                status = WorkStatusEnum.InProgress;
            else if (tskg == TSKG.Item3)
                status = WorkStatusEnum.Paused;
            else if (tskg == TSKG.Item4)
                status = WorkStatusEnum.Completed;
            else if (tskg == TSKG.Item5)
                status = WorkStatusEnum.Scheduled;

            return new StatusUpdate
            {
                Status = status
            };
        }
    }
}
