using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IPtnHeaderMapper
    {
        PTN Map();
    }

    public class PtnHeaderMapper : IPtnHeaderMapper
    {
        public PTN Map()
        {
            return new PTN
            {
                ASpecified = true,
                A = null,
                BSpecified = true,
                B = null,
                CSpecified = true,
                C = null,
                DSpecified = false,
                ESpecified = false,
                FSpecified = false,
                GSpecified = false,
                HSpecified = false
            };
        }
    }
}
