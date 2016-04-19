using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IColourLegendMapper
    {
        //TODO
        //List<CLD> Export(ColourLegends colorLegends, Dictionary<Guid, string> keyToIsoId);
    }

    public class ColourLegendMapper : IColourLegendMapper
    {
        //TODO
        //public List<CLD> Export(ColourLegends colorLegends, Dictionary<Guid, string> keyToIsoId)
        //{
        //    if (colorLegends == null || !colorLegends.Any())
        //        return new List<CLD>();

        //    int colourIndex = 0;
        //    return colorLegends.Select(x => Export(x, keyToIsoId, colourIndex++)).ToList();
        //}

        //private CLD Export(ColourLegend colorLegend, Dictionary<Guid, string> keyToIsoId, int colourIndex)
        //{
        //    var isoColourLegend = new CLD
        //    {
        //        B = (byte) colorLegend.DefaultColour,
        //        Items = colorLegend.ColourRanges.Select(Export).ToArray()
        //    };
        //    isoColourLegend.A = isoColourLegend.GetIsoId(colourIndex);

        //    keyToIsoId.Add(colorLegend.Key, isoColourLegend.A);
        //    return isoColourLegend;
        //}

        //private CRG Export(ColourRange colorRange)
        //{
        //    return new CRG
        //    {
        //        A = (long) Math.Round(colorRange.MinValue, 0),
        //        B = (long) Math.Round(colorRange.MaxValue, 0),
        //        C = (byte) colorRange.Colour
        //    };
        //}
    }
}
