using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ImportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.Extensions
{
    public static class ExtensionMethods
    {
        private static readonly Regex IsoIdPattern = new Regex("^[A-Z]{3,4}-?[0-9]+$", RegexOptions.Compiled);
        private static readonly Regex DigitsOnly = new Regex(@"[^\d]", RegexOptions.Compiled);

        public static TValue FindById<TKey, TValue>(this Dictionary<TKey, TValue> items, TKey id) where TValue : class
        {
            if (items == null || items.Count == 0 || id == null)
                return null;

            TValue value;
            if (items.TryGetValue(id, out value))
                return value;

            return null;
        }

        public static List<T> GetItemsOfType<T>(this object[] items)
        {
            if (items == null)
                return null;

            var itemsOfType = items.Where(x => x.GetType() == typeof(T)).Cast<T>().ToList();
            return itemsOfType;
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
    }
}
