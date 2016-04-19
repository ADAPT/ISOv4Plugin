using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Extensions
{
    public static class ExtensionMethods
    {
        public static TValue FindById<TKey, TValue>(this Dictionary<TKey, TValue> items, TKey id) where TValue : class
        {
            if (items == null || items.Count == 0 || id == null)
                return null;

            TValue value;
            if (items.TryGetValue(id, out value))
                return value;

            return null;
        }

        public static List<Section> GetAllSections(this OperationData operationData)
        {
            if(operationData.GetSections == null)
                return new List<Section>();

            var allSections = new List<Section>();

            for (var i = 0; i <= operationData.MaxDepth; i++)
            {
                var sections = operationData.GetSections(i);
                if(sections != null)
                    allSections.AddRange(sections);
            }

            return allSections;
        }

        public static bool ParseValue(this string value, out long result)
        {
            result = default(long);

            if (string.IsNullOrEmpty(value))
                return false;

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static bool ParseValue(this string value, out int result)
        {
            result = default(int);

            if (string.IsNullOrEmpty(value))
                return false;

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static bool ParseValue(this string value, out double result)
        {
            result = default(double);

            if (string.IsNullOrEmpty(value))
                return false;

            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static double ConvertFromIsoUnit(this IsoUnit unit, double value)
        {
            if (unit == null)
                return value;

            return (value + unit.Offset) * unit.Scale;
        }

        public static double ConvertToIsoUnit(this IsoUnit unit, double value)
        {
            if (unit == null)
                return value;

            return value / unit.Scale - unit.Offset;
        }

        public static UnitOfMeasure ToAdaptUnit(this IsoUnit isoUnit)
        {
            if (isoUnit == null)
                return null;

            var adaptUnuit = ADAPT.Representation.UnitSystem.UnitSystemManager.GetUnitOfMeasure(isoUnit.Code);

            return adaptUnuit;
        }

        public static UnitOfMeasure ToAdaptUnit(this ValuePresentation unit)
        {
            if (unit == null)
                return null;

            return null;
        }

        public static string GetIsoId<T>(this T isoObject, int objectId)
        {
            var className = typeof(T).Name;
            return string.Format("{0}{1}", className, objectId);
        }

        public static string FindIsoId(this CompoundIdentifier adaptId)
        {
            var stringIds = adaptId.UniqueIds.Where(id => id.Source == UniqueIdMapper.IsoSource && id.CiTypeEnum == CompoundIdentifierTypeEnum.String).ToList();

            var isoIdPattern = new Regex("^[A-Z]{3,4}-?[0-9]+$");
            stringIds.RemoveAll(x => x.Id == null);
            var isoId = stringIds.Where(s => isoIdPattern.IsMatch(s.Id)).ToList();

            if (isoId.Count == 0)
                return null;

            return isoId.First().Id;
        }

        public static int FindIntIsoId(this CompoundIdentifier adaptId)
        {
            var isoId = FindIsoId(adaptId);

            if (isoId == null)
                return 0;

            var digitsOnly = new Regex(@"[^\d]");
            return int.Parse(digitsOnly.Replace(isoId, ""));
        }
    }
}
