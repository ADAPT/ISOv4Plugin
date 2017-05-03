using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin.ImportMappers
{
    public interface IUniqueIdMapper
    {
        UniqueId Map(string id);
    }

    public class UniqueIdMapper : IUniqueIdMapper
    {
        public const string IsoSource = "http://dictionary.isobus.net/isobus/";

        public UniqueId Map(string id)
        {
            return new UniqueId
            {
                Id = id,
                Source = IsoSource,
                IdType = IdTypeEnum.String,
                SourceType = IdSourceTypeEnum.URI
            };
        }
    }
}
