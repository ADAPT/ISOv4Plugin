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

        public RepresentationMapper()
        {
            _ddis = DdiLoader.Ddis;
        }

        public AdaptRepresentation Map(int ddi)
        {
            if (_ddis.ContainsKey(ddi))
            {
                var matchingDdi = _ddis[ddi];
                var representation = RepresentationManager.Instance.Representations.FirstOrDefault(x => x.Ddi.GetValueOrDefault() == matchingDdi.Id);

                var numericRep = representation as NumericRepresentation;
                if (numericRep != null)
                    return numericRep.ToModelRepresentation();

                var enumeratedRep = representation as EnumeratedRepresentation;

                if (enumeratedRep != null)
                    return enumeratedRep.ToModelRepresentation();

                return new ApplicationDataModel.Representations.NumericRepresentation { Code = ddi.ToString("X4"), CodeSource = RepresentationCodeSourceEnum.ISO11783_DDI };
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
