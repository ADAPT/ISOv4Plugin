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

        public static double ConvertToIsoUnit(this ISOUnit unit, double srcValue, string srcUnitCode)
        {
            if (unit == null)
                return srcValue;

            double convertedValue = srcValue.ConvertValue(srcUnitCode, unit.ToAdaptUnit().Code);
            double scaledValue = (convertedValue / unit.Scale) - unit.Offset;
            return scaledValue;
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
            if (ddi.HasValue &&
                value?.Value != null &&
                value?.Value?.UnitOfMeasure != null)
            {
                ISOUnit unit = UnitFactory.Instance.GetUnitByDDI(ddi.Value);
                return (int) unit.ConvertToIsoUnit(value.Value.Value, value.Value.UnitOfMeasure.Code);
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
        public static double? AsConvertedDouble(this NumericRepresentationValue value, string targetUnitCode)
        {
            if (value == null)
            {
                return null;
            }
            else if (value.Value.UnitOfMeasure == null)
            {
                return value.Value.Value; //Return the unconverted value
            }
            else
            {
                return value.Value.Value.ConvertValue(value.Value.UnitOfMeasure.Code, targetUnitCode);
            }
        }

        public static double ConvertValue(this double n, string srcUnitCode, string dstUnitCode)
        {
            RepresentationUnitSystem.UnitOfMeasure sourceUOM = RepresentationUnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures[srcUnitCode];
            RepresentationUnitSystem.UnitOfMeasure targetUOM = RepresentationUnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures[dstUnitCode];
            if (sourceUOM == null || targetUOM == null)
            {
                return n; //Return the unconverted value
            }
            else
            {
                //The plugin uses "count" instead of "seeds".  Alter the codes so that the conversion will succeed if there are mismatches
                if (srcUnitCode.StartsWith("seeds"))
                {
                    srcUnitCode = srcUnitCode.Replace("seeds", "count");
                    sourceUOM = RepresentationUnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures[srcUnitCode];
                }
                if (dstUnitCode.StartsWith("seeds"))
                {
                    dstUnitCode = dstUnitCode.Replace("seeds", "count");
                    targetUOM = RepresentationUnitSystem.InternalUnitSystemManager.Instance.UnitOfMeasures[dstUnitCode];
                }

                RepresentationUnitSystem.UnitOfMeasureConverter converter = new RepresentationUnitSystem.UnitOfMeasureConverter();
                return converter.Convert(sourceUOM, targetUOM, n);
            }
        }

        public static int? AsConvertedInt(this NumericRepresentationValue value, string targetUnitCode)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return (int)value.AsConvertedDouble(targetUnitCode).Value;
            }
        }

        public static IEnumerable<string> GetDirectoryFiles(this string dataPath, string searchPath, SearchOption searchOption)
        {
            if (Directory.Exists(dataPath))
            {
                //Note! We need to iterate through all files and do a ToLower for this to work in .Net Core in Linux since that filesystem
                //is case sensitive and the NetStandard interface for Directory.GetFiles doesn't account for that yet.
                var fileNameToFind = searchPath.ToLower();
                var allFiles = Directory.GetFiles(dataPath, "*.*", searchOption);
                var matchedFiles = allFiles.Where(file => file.ToLower().EndsWith(fileNameToFind));
                return matchedFiles;
            }
            return new List<string>();
        }

        /// <summary>
        /// Case-insensitive comparison of two strings
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string value1, string value2)
        {
            return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
