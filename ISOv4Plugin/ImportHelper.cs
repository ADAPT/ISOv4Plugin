using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public static class ImportHelper
    {
        public static UniqueId CreateUniqueId(string isoId)
        {
            return new UniqueId
            {
                Id = isoId,
                IdType = IdTypeEnum.String,
                Source = "http://dictionary.isobus.net/isobus/"
            };
        }
    }
}
