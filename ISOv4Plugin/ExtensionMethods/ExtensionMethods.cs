﻿/*
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

        public static string FindIsoId(this CompoundIdentifier adaptId, string prefixFilter = null)
        {
            var stringIds = adaptId.UniqueIds.Where(id => id.Id != null &&
                id.Source == UniqueIdMapper.IsoSource &&
                id.IdType == IdTypeEnum.String).ToList();

            var isoId = stringIds.FirstOrDefault(s => IsoIdPattern.IsMatch(s.Id) && (prefixFilter == null || s.Id.StartsWith(prefixFilter)));

            return isoId != null ? isoId.Id : null;
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
        /// 
        public static NumericRepresentationValue AsNumericRepresentationValue(this int n, int ddi, RepresentationMapper mapper, UnitOfMeasure userUnitOfMeasure = null)
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
        public static NumericRepresentationValue AsNumericRepresentationValue(this int n, string ddiHexString, RepresentationMapper mapper, UnitOfMeasure userUnitOfMeasure = null)
        {
            return n.AsNumericRepresentationValue(ddiHexString.AsInt32DDI(), mapper, userUnitOfMeasure);
        }

        public static NumericRepresentationValue AsNumericRepresentationValue(this ISOProcessDataVariable pdv, RepresentationMapper mapper, ISO11783_TaskData taskData)
        {
            return pdv.ProcessDataValue.AsNumericRepresentationValue(pdv.ProcessDataDDI, mapper, pdv.ToDisplayUnit(mapper, taskData));
        }

        public static NumericRepresentationValue AsNumericRepresentationValue(this int n, string uomCode)
        {
            NumericRepresentationValue returnValue = new NumericRepresentationValue();
            UnitOfMeasure uom = AgGateway.ADAPT.Representation.UnitSystem.UnitSystemManager.GetUnitOfMeasure(uomCode);
            returnValue.Value = new NumericValue(uom, (double)n);
            return returnValue;
        }

        public static UnitOfMeasure ToDisplayUnit(this ISOProcessDataVariable pdv, RepresentationMapper mapper, ISO11783_TaskData taskData)
        {
            ISOUnit userUnit = null;
            if (!string.IsNullOrEmpty(pdv.ValuePresentationIdRef))
            {
                ISOValuePresentation vpn = taskData.ChildElements.OfType<ISOValuePresentation>().FirstOrDefault(v => v.ValuePresentationID == pdv.ValuePresentationIdRef);
                if (vpn != null)
                {
                    userUnit = new ISOUnit(vpn);
                }
            }
            if (userUnit != null)
            {

                UnitOfMeasure adaptUnit = null;
                try
                {
                    adaptUnit = userUnit.ToAdaptUnit();
                }
                catch
                {
                    //Suppressing this as a non-critical exception
                }
                return adaptUnit;
            }
            return null;
        }

        public static UnitOfMeasure ToAdaptUnit(this ISOUnit isoUnit)
        {
            if (isoUnit == null)
                return null;

            return AgGateway.ADAPT.Representation.UnitSystem.UnitSystemManager.GetUnitOfMeasure(isoUnit.Code);
        }

        public static double ConvertFromIsoUnit(this ISOUnit unit, double value)
        {
            if (unit == null)
                return value;

            double newValue = (value + unit.Offset) * unit.Scale;
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
        /// Converts an ADAPT NumericRepresentation value with an included Representation mapped to a DDI to the appropriate int value for ISO
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static int AsIntViaMappedDDI(this NumericRepresentationValue value, RepresentationMapper mapper)
        {
            int? ddi = mapper.Map(value.Representation);
            if (ddi.HasValue)
            {
                ISOUnit unit = UnitFactory.Instance.GetUnitByDDI(ddi.Value);
                return (int) unit.ConvertToIsoUnit(value.Value.Value);
            }
            else if (value.Representation != null && value.Representation.CodeSource == RepresentationCodeSourceEnum.ISO11783_DDI)
            {
                //No need to convert if the value is natively a DDI
                return (int) value.Value.Value;
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

        public static int AsConvertedInt(this NumericRepresentationValue value, string targetUnitCode)
        {
            return (int)value.AsConvertedDouble(targetUnitCode);
        }
    }
}
