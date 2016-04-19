using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.ReferenceLayers;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface ICropZoneMapper
    {
        IEnumerable<PFD> Map(List<CropZone> cropZones, List<PFD> isoFields, Dictionary<int, string> keyToIsoId, Catalog setupCatalog);
    }

    public class CropZoneMapper : ICropZoneMapper
    {
        private readonly IPolygonMapper _boundaryMapper;

        public CropZoneMapper() : this(new PolygonMapper())
        {
        }

        public CropZoneMapper(IPolygonMapper boundaryMapper)
        {
            _boundaryMapper = boundaryMapper;
        }

        public IEnumerable<PFD> Map(List<CropZone> cropZones, List<PFD> isoFields, Dictionary<int, string> keyToIsoId, Catalog setupCatalog)
        {
            if(cropZones == null)
                return null;
            int cropZoneIndex = isoFields.Count;
            return cropZones.Select(x => Map(x, keyToIsoId, cropZoneIndex++, setupCatalog));
        }

        private PFD Map(CropZone cropZone, Dictionary<int, string> keyToIsoId, int cropZoneIndex, Catalog setupCatalog)
        {
            var pfd = new PFD
            {
                C = cropZone.Description,
                D = (ulong)Math.Round(cropZone.Area.Value.Value, 0)
            };
            pfd.A = pfd.GetIsoId(cropZoneIndex);
            if (cropZone.CropId != null)
            {
                pfd.G = keyToIsoId[cropZone.CropId.Value];
            }
            pfd.I = keyToIsoId[cropZone.FieldId];
            var field = setupCatalog.Fields.First(f => f.Id.ReferenceId == cropZone.FieldId);
            if (field.FarmId != null)
            {
                pfd.F = keyToIsoId[field.FarmId.Value];
                var farm = setupCatalog.Farms.First(f => f.Id.ReferenceId == field.FarmId.Value);
                if (farm.GrowerId != null)
                    pfd.E = keyToIsoId[farm.GrowerId.Value];
            }
            if (cropZone.BoundingRegion != null)
            {
                pfd.Items = new object[] { _boundaryMapper.Map(cropZone.BoundingRegion, BoundaryType.CropZone, cropZone.Description) };
            }

            keyToIsoId.Add(cropZone.Id.ReferenceId, pfd.A);
            return pfd;
        }
    }
}
