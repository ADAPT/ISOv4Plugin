/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Mappers;
using System;
using System.IO;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using RepresentationUnitSystem = AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;


namespace AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods
{
    public static class ExtensionMethods
    {
        private static readonly Regex IsoIdPattern = new Regex("^[A-Z]{3,4}-?[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex DigitsOnly = new Regex(@"[^\d]", RegexOptions.Compiled);

        public static TValue FindByADAPTId<TKey, TValue>(this Dictionary<TKey, TValue> items, TKey id) where TValue : class
        {
            if (items == null || items.Count == 0 || id == null)
                return null;

            TValue value;
            if (items.TryGetValue(id, out value))
                return value;

            return null;
        }

        public static Nullable<TValue> FindByISOId<TKey, TValue>(this Dictionary<TKey, Nullable<TValue>> items, TKey id) where TValue : struct
        {
            if (items == null || items.Count == 0 || id == null)
            { 
                return null;
            }

            if (items.ContainsKey(id))
            {
                return items[id];
            }
            else
            {
                return null;
            }
        }

        public static string WithTaskDataPath(this string dataPath)
        {
            return Path.Combine(dataPath, "TASKDATA");
        }

        public static List<DeviceElementUse> GetAllSections(this OperationData operationData)
        {
            if(operationData.GetDeviceElementUses == null)
                return new List<DeviceElementUse>();

            var allSections = new List<DeviceElementUse>();

            for (var i = 0; i <= operationData.MaxDepth; i++)
            {
                var sections = operationData.GetDeviceElementUses(i);
                if(sections != null)
                    allSections.AddRange(sections);
            }

            return allSections;
        }

        public static string FindIsoId(this CompoundIdentifier adaptId)
        {
            var stringIds = adaptId.UniqueIds.Where(id => id.Id != null &&
                id.Source == UniqueIdMapper.IsoSource &&
                id.IdType == IdTypeEnum.String).ToList();

            var isoId = stringIds.FirstOrDefault(s => IsoIdPattern.IsMatch(s.Id));

            return isoId != null ? isoId.Id : null;
        }

        public static int FindIntIsoId(this CompoundIdentifier adaptId)
        {
            var isoId = FindIsoId(adaptId);

            if (isoId == null)
                return -1;

            return int.Parse(DigitsOnly.Replace(isoId, ""));
        }

        public static int AsInt32DDI(this string ddiHexString)
        {
            return Convert.ToInt32(ddiHexString, 16);
        }

        public static string AsHexDDI(this int n)
        {
            return n.ToString("X4");
        }


        /// <summary>
        /// Looks up unit, converts and loads representation
        /// </summary>
        /// <param name="n"></param>
        /// <param name="ddi"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static NumericRepresentationValue AsNumericRepresentationValue(this long n, int ddi, RepresentationMapper mapper, UnitOfMeasure userUnitOfMeasure = null) 
        {
            //RepresentationValue
            NumericRepresentationValue returnValue = new ApplicationDataModel.Representations.NumericRepresentationValue();
            //Representation
            returnValue.Representation = mapper.Map(ddi) as NumericRepresentation;

            //Value
            double convertedValue = (double)n;
            UnitOfMeasure uom = null;
            ISOUnit isoUnit = UnitFactory.Instance.GetUnitByDDI(ddi);
            if (isoUnit != null)
            {
                convertedValue = isoUnit.ConvertFromIsoUnit(convertedValue);
                uom = isoUnit.ToAdaptUnit();
            }
            returnValue.Value = new ApplicationDataModel.Representations.NumericValue(uom, convertedValue);

            //User UOM
            if (userUnitOfMeasure != null)
            {
                returnValue.UserProvidedUnitOfMeasure = userUnitOfMeasure;
            }

            return returnValue;
        }

        /// <summary>
        /// Looks up unit, converts and loads representation
        /// </summary>
        /// <param name="n"></param>
        /// <param name="ddiHexString"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static NumericRepresentationValue AsNumericRepresentationValue(this long n, string ddiHexString, RepresentationMapper mapper, UnitOfMeasure userUnitOfMeasure = null)
        {
            return n.AsNumericRepresentationValue(ddiHexString.AsInt32DDI(), mapper, userUnitOfMeasure);
        }

        public static NumericRepresentationValue AsNumericRepresentationValue(this ISOProcessDataVariable pdv, RepresentationMapper mapper, ISO11783_TaskData taskData)
        {
            return pdv.ProcessDataValue.AsNumericRepresentationValue(pdv.ProcessDataDDI, mapper, pdv.ToUserAdaptUnit(mapper, taskData));
        }

        public static NumericRepresentationValue AsNumericRepresentationValue(this long n, string uomCode)
        {
            NumericRepresentationValue returnValue = new NumericRepresentationValue();
            UnitOfMeasure uom = AgGateway.ADAPT.Representation.UnitSystem.UnitSystemManager.GetUnitOfMeasure(uomCode);
            returnValue.Value = new NumericValue(uom, (double)n);
            return returnValue;
        }

        public static UnitOfMeasure ToAdaptUnit(this ISOProcessDataVariable pdv, RepresentationMapper mapper, ISO11783_TaskData taskData)
        {
            ISOUnit isoUnit = UnitFactory.Instance.GetUnitByDDI(pdv.ProcessDataDDI.AsInt32DDI());
            if (isoUnit != null)
            {
                return isoUnit.ToAdaptUnit();  //Unit based on DDI
            }
            else 
            {
                return pdv.ToUserAdaptUnit(mapper, taskData); //Presentation Unit
            }
        }

        public static ISOUnit ToISOUnit(this ISOProcessDataVariable pdv, RepresentationMapper mapper, ISO11783_TaskData taskData)
        {
            if (!string.IsNullOrEmpty(pdv.ValuePresentationIdRef))
            {
                ISOValuePresentation vpn = taskData.ChildElements.OfType<ISOValuePresentation>().FirstOrDefault(v => v.ValuePresentationID == pdv.ValuePresentationIdRef);
                if (vpn != null)
                {
                    return new ISOUnit(vpn);
                }
            }
            return null;
        }

        public static UnitOfMeasure ToUserAdaptUnit(this ISOProcessDataVariable pdv, RepresentationMapper mapper, ISO11783_TaskData taskData)
        {
            ISOUnit userUnit = pdv.ToISOUnit(mapper, taskData);
            //TODO map to an Adapt Unit based on code; non-matching codes will error.
            return null;
        }

        public static UnitOfMeasure ToAdaptUnit(this ISOUnit isoUnit)
        {
            if (isoUnit == null)
                return null;
            var adaptUnit = AgGateway.ADAPT.Representation.UnitSystem.UnitSystemManager.GetUnitOfMeasure(isoUnit.Code);

            return adaptUnit;
        }

        public static double ConvertFromIsoUnit(this ISOUnit unit, double value)
        {
            if (unit == null)
                return value;

            double newValue = (value + unit.Offset) * unit.Scale;
            return newValue;
        }

        public static double ConvertFromIsoUnit(this ISOUnit unit, long value)
        {
            if (unit == null)
                return (double)value;

            double newValue = ((double)value + unit.Offset) * unit.Scale;
            return newValue;
        }

        public static double ConvertToIsoUnit(this ISOUnit unit, double value)
        {
            if (unit == null)
                return value;

            double newValue = (value / unit.Scale) - unit.Offset;
            return newValue;
        }

        /// <summary>
        /// Converts an ADAPT NumericRepresentation value with an included Representation mapped to a DDI to the appropriate long value for ISO
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static long AsLongViaMappedDDI(this NumericRepresentationValue value, RepresentationMapper mapper)
        {
            int? ddi = mapper.Map(value.Representation);
            if (ddi.HasValue)
            {
                ISOUnit unit = UnitFactory.Instance.GetUnitByDDI(ddi.Value);
                return (long)unit.ConvertToIsoUnit(value.Value.Value);
            }
            else if (value.Representation.CodeSource == RepresentationCodeSourceEnum.ISO11783_DDI)
            {
                //No need to convert if the value is natively a DDI
                return (long)value.Value.Value;
            }
            return 0;
        }

        /// <summary>
        /// Converts an ADAPT NumericRepresentation value with an included UnitOfMeasure to the requested units
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static double AsConvertedDouble(this NumericRepresentationValue value, string targetUnitCode)
        {
            if (value.Value.UnitOfMeasure == null)
            {
                return value.Value.Value; //Return the unconverted value
            }
            else
            {
                RepresentationUnitSystem.UnitOfMeasure sourceUOM = RepresentationUnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures[value.Value.UnitOfMeasure.Code];
                RepresentationUnitSystem.UnitOfMeasure targetUOM = RepresentationUnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures[targetUnitCode];
                if (sourceUOM == null || targetUOM == null)
                {
                    return value.Value.Value; //Return the unconverted value
                }
                else
                {
                    RepresentationUnitSystem.UnitOfMeasureConverter converter = new RepresentationUnitSystem.UnitOfMeasureConverter();
                    return converter.Convert(sourceUOM, targetUOM, value.Value.Value);
                }
            }
        }

        public static long AsConvertedLong(this NumericRepresentationValue value, string targetUnitCode)
        {
            return (long)value.AsConvertedDouble(targetUnitCode);
        }
    }
}
