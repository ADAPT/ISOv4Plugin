using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.Models
{
    public class TCError : IError
    {
        public string Description { get; set; }

        public string Id { get; set; }

        public string Source { get; set; }

        public string StackTrace { get; set; }
    }
}
