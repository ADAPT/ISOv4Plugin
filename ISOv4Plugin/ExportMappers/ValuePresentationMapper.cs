using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IValuePresentationMapper
    {
        //IEnumerable<VPN> Export(ValuePresentations valuePresentations, Dictionary<Guid, string> keyToIsoId);
    }

    public class ValuePresentationMapper : IValuePresentationMapper
    {
        //TODO
        //public IEnumerable<VPN> Export(ValuePresentations valuePresentations, Dictionary<Guid, string> keyToIsoId)
        //{
        //    if (valuePresentations == null || !valuePresentations.Any())
        //        return new List<VPN>();

        //    int valuePresentationIndex = 0;
        //    return valuePresentations.Select(x => Export(x, keyToIsoId, valuePresentationIndex++)).ToList();
        //}

        //private VPN Export(ValuePresentation valuePresentation, Dictionary<Guid, string> keyToIsoId, int valuePresentationIndex)
        //{
        //    var isoValuePresentation = new VPN
        //    {
        //        D = (byte) valuePresentation.NumberOfDecimals
        //    };
        //    isoValuePresentation.A = isoValuePresentation.GetIsoId(valuePresentationIndex);
        //    if (valuePresentation.Unit != null)
        //    {
        //        isoValuePresentation.B = (long) Math.Round(valuePresentation.Unit.Offset, 0);
        //        isoValuePresentation.C = new decimal(valuePresentation.Unit.Scale);
        //        isoValuePresentation.E = valuePresentation.Unit.Name;
        //    }

        //    keyToIsoId.Add(valuePresentation.Key, isoValuePresentation.A);
        //    return isoValuePresentation;
        //}
    }
}
