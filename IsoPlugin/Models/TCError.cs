using AgGateway.ADAPT.ApplicationDataModel;

namespace AgGateway.ADAPT.Plugins
{
    internal class TCError : IError
    {
        public string Description { get; internal set; }

        public string Id { get; internal set; }

        public string Source { get; internal set; }

        public string StackTrace { get; internal set; }
    }
}
