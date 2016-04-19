using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ISOv4Plugin.Extensions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public interface IGrowerFarmFieldMapper
    {
        List<CTR> Map(List<Grower> growers, Dictionary<int, string> keyToIsoId);
        List<FRM> Map(List<Farm> farms, Dictionary<int, string> keyToIsoId);
        List<PFD> Map(List<Field> fields, Dictionary<int, string> keyToIsoId, Catalog setupCatalog);
    }

    public class GrowerFarmFieldMapper : IGrowerFarmFieldMapper
    {
        private readonly IPolygonMapper _boundaryMapper;

        public GrowerFarmFieldMapper() : this(new PolygonMapper())
        {
        }

        public GrowerFarmFieldMapper(IPolygonMapper boundaryMapper)
        {
            _boundaryMapper = boundaryMapper;
        }

        public List<CTR> Map(List<Grower> growers, Dictionary<int, string> keyToIsoId)
        {
            if (growers == null || !growers.Any())
                return new List<CTR>();

            int growerIndex = 0;
            return growers.Select(x => MapGrower(x, growerIndex++, keyToIsoId)).ToList();
        }

        public List<FRM> Map(List<Farm> farms, Dictionary<int, string> keyToIsoId)
        {
            if (farms == null || !farms.Any())
                return new List<FRM>();

            int farmIndex = 0;
            return farms.Select(x => MapFarm(x, keyToIsoId, farmIndex++)).ToList();
        }

        public List<PFD> Map(List<Field> fields, Dictionary<int, string> keyToIsoId, Catalog setupCatalog)
        {
            if (fields == null || !fields.Any())
                return new List<PFD>();

            int fieldIndex = 0;
            return fields.Select(x => MapField(x, keyToIsoId, fieldIndex++, setupCatalog)).ToList();
        }

        private CTR MapGrower(Grower grower, int growerIndex, Dictionary<int, string> keyToIsoId)
        {
            var isoGrower = new CTR
            {
                B = grower.Name
            };
            isoGrower.A = isoGrower.GetIsoId(growerIndex);

            keyToIsoId.Add(grower.Id.ReferenceId, isoGrower.A);
            return isoGrower;
        }

        private FRM MapFarm(Farm farm, Dictionary<int, string> keyToIsoId, int farmIndex)
        {
            var isoFarm = new FRM
            {
                B = farm.Description,
            };
            isoFarm.A = isoFarm.GetIsoId(farmIndex);
            if (farm.GrowerId != null)
                isoFarm.I = keyToIsoId[farm.GrowerId.Value];

            keyToIsoId.Add(farm.Id.ReferenceId, isoFarm.A);
            return isoFarm;
        }

        private PFD MapField(Field field, Dictionary<int, string> keyToIsoId, int fieldIndex, Catalog setupCatalog)
        {
            var isoField = new PFD
            {
                C = field.Description,
                D = (ulong) Math.Round(field.Area.Value.Value, 0)
            };
            isoField.A = isoField.GetIsoId(fieldIndex);
            if (field.FarmId != null)
            {
                isoField.F = keyToIsoId[field.FarmId.Value];
                var farm = setupCatalog.Farms.First(f => f.Id.ReferenceId == field.FarmId.Value);
                if (farm.GrowerId != null)
                {
                    isoField.E = keyToIsoId[farm.GrowerId.Value];
                }
            }
            if (field.ActiveBoundaryId != null)
            {
                var boundary = setupCatalog.FieldBoundaries.Single(b => b.Id.ReferenceId == field.ActiveBoundaryId.Value);
                isoField.Items = new object[] {_boundaryMapper.Map(boundary.SpatialData, BoundaryType.Field, boundary.Description)};
            }

            keyToIsoId.Add(field.Id.ReferenceId, isoField.A);
            return isoField;
        }
    }
}
