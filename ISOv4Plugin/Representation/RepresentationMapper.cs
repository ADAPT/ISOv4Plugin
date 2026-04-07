/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;
using AgGateway.ADAPT.Representation.UnitSystem;
using UnitOfMeasure = AgGateway.ADAPT.ApplicationDataModel.Common.UnitOfMeasure;
using AdaptRepresentation = AgGateway.ADAPT.ApplicationDataModel.Representations.Representation;
using EnumeratedRepresentation = AgGateway.ADAPT.Representation.RepresentationSystem.EnumeratedRepresentation;
using NumericRepresentation = AgGateway.ADAPT.Representation.RepresentationSystem.NumericRepresentation;
using System;

namespace AgGateway.ADAPT.ISOv4Plugin.Representation
{
    public interface IRepresentationMapper
    {
        AdaptRepresentation Map(int ddi);
        int? Map(AdaptRepresentation adapRepresentation);
        UnitOfMeasure GetUnitForDdi(int ddi);
    }

    public class RepresentationMapper : IRepresentationMapper
    {
        private readonly Dictionary<int, DdiDefinition> _ddis;
        private readonly Dictionary<int, AdaptRepresentation> _ddiToRepresentationCache;

        public RepresentationMapper()
        {
            _ddis = DdiLoader.Ddis;
            _ddiToRepresentationCache = BuildDdiToRepresentationCache();
        }

        private Dictionary<int, AdaptRepresentation> BuildDdiToRepresentationCache()
        {
            var cache = new Dictionary<int, AdaptRepresentation>();
            foreach (var kvp in _ddis)
            {
                var ddi = kvp.Key;
                var matchingDdi = kvp.Value;
                var representations = RepresentationManager.Instance.Representations.Where(x => x.Ddi.GetValueOrDefault() == matchingDdi.Id);
                if (representations.Any())
                {
                    var representation = representations.FirstOrDefault(r => r.IsDefaultRepresentationForDDI)
                                            ??
                                         representations.First();
                    var adaptRep = GetADAPTRepresentation(representation);
                    if (adaptRep != null)
                    {
                        cache[ddi] = adaptRep;
                    }
                }
            }
            return cache;
        }

        public AdaptRepresentation Map(int ddi)
        {
            if (_ddiToRepresentationCache.TryGetValue(ddi, out var cached))
            {
                return cached;
            }

            if (_ddis.ContainsKey(ddi))
            {
                return new ApplicationDataModel.Representations.NumericRepresentation { Code = ddi.ToString("X4"), CodeSource = RepresentationCodeSourceEnum.ISO11783_DDI };
            }
            return null;
        }

        private static AdaptRepresentation GetADAPTRepresentation(ADAPT.Representation.RepresentationSystem.Representation representation)
        {
            var numericRep = representation as NumericRepresentation;
            if (numericRep != null)
                return numericRep.ToModelRepresentation();

            var enumeratedRep = representation as EnumeratedRepresentation;
            if (enumeratedRep != null)
                return enumeratedRep.ToModelRepresentation();

            return null;
        }

        public static AdaptRepresentation GetRepresentation(string code)
        {
            var matchingRepresentation = RepresentationManager.Instance.Representations[code];
            if (matchingRepresentation != null)
            {
                return GetADAPTRepresentation(matchingRepresentation);
            }
            return null;
        }

        public int? Map(AdaptRepresentation adaptRepresentation)
        {
            if (adaptRepresentation == null)
                return null;

            var matchingRepresentation = RepresentationManager.Instance.Representations[adaptRepresentation.Code];

            return matchingRepresentation != null
                ? matchingRepresentation.Ddi
                : null;
        }

        public UnitOfMeasure GetUnitForDdi(int ddi)
        {
            if (!_ddis.ContainsKey(ddi))
                return null;
            var matchingDdi = _ddis[ddi];

            if (matchingDdi != null)
            {
                var uom = IsoUnitOfMeasureList.Mappings.Single(x => x.Unit == matchingDdi.Unit);
                return UnitSystemManager.GetUnitOfMeasure(uom.AdaptCode);
            }
            return null;
        }
    }
}
