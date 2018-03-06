/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using System.Xml;
using System.Collections.Generic;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;
using System.Globalization;
using AgGateway.ADAPT.ApplicationDataModel.ADM;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOElement
    {
        public virtual XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            throw new NotImplementedException();
        }

        public virtual List<IError> Validate(List<IError> errors)
        {
            throw new NotImplementedException();
        }

        #region Validation
        //Elected to use this custom validation logic vs. the Attribute based logic in System.ComponentModel.DataAnnotations
        //as that is primarily geared at web apps and had other limitations here.
        //This Linq expression syntax allows for simplified method calls not requiring
        //the property name and its value to be separate parameters.

        protected bool Require<T, P>(T obj, Expression<Func<T, P>> expression, List<IError> errors, string attributeName = null)
        {
            string propertyName = (expression.Body as MemberExpression).Member.Name;
            Func<T, P> expressionDelegate = expression.Compile();
            P propertyValue = expressionDelegate(obj);
            return Require(propertyValue, typeof(T).Name, propertyName, errors, attributeName);
        }

        protected bool RequireString<T, P>(T obj, Expression<Func<T, P>> expression, int maxLength, List<IError> errors, string attributeName = null)
        {
            string propertyName = (expression.Body as MemberExpression).Member.Name;
            Func<T, P> expressionDelegate = expression.Compile();
            P propertyValue = expressionDelegate(obj);
            if (Require(propertyValue, typeof(T).ToString(), propertyName, errors, attributeName))
            {
                return ValidateString(propertyValue, maxLength, typeof(T).Name, propertyName, errors, attributeName);
            }
            return false;
        }

        protected bool ValidateString<T, P>(T obj, Expression<Func<T, P>> expression, int maxLength, List<IError> errors, string attributeName = null)
        {
            string propertyName = (expression.Body as MemberExpression).Member.Name;
            Func<T, P> expressionDelegate = expression.Compile();
            P propertyValue = expressionDelegate(obj);
            return ValidateString(propertyValue, maxLength, typeof(T).Name, propertyName, errors, attributeName);
        }

        protected bool RequireRange<T, P>(T obj, Expression<Func<T, P>> expression, P min, P max, List<IError> errors, string attributeName = null) where P : struct
        {
            string propertyName = (expression.Body as MemberExpression).Member.Name;
            Func<T, P> expressionDelegate = expression.Compile();
            P propertyValue = expressionDelegate(obj);
            if (Require(propertyValue, typeof(T).Name, propertyName, errors, attributeName))
            {
                return ValidateRange(propertyValue, typeof(T).Name, propertyName, min, max, errors, attributeName);
            }
            else
            {
                return false;
            }
        }

        protected bool ValidateRange<T, P>(T obj, Expression<Func<T, P>> expression, P min, P max, List<IError> errors, string attributeName = null) where P : struct
        {
            string propertyName = (expression.Body as MemberExpression).Member.Name;
            Func<T, P> expressionDelegate = expression.Compile();
            P propertyValue = expressionDelegate(obj);
            return ValidateRange(propertyValue, typeof(T).Name, propertyName, min, max, errors, attributeName);
        }

        protected bool ValidateEnumerationValue(Type enumType, int value, List<IError> errors)
        {
            if (!Enum.IsDefined(enumType, value))
            {
                errors.Add(new Error() { Description = $"{this.GetType().Name} contains an invalid value ({value.ToString()}) for the enumeration {enumType.GetType().ToString()}." });
                return false;
            }
            return true;
        }

        protected bool RequireNonZeroCount<T>(List<T> list, string requiredChildElementName, List<IError> errors)
        {
            if (list == null || list.Count == 0)
            {
                errors.Add(new Error() { Description = $"{this.GetType().Name} requires at least one {requiredChildElementName}." });
                return false;
            }
            return true;
        }

        protected bool RequireChildElement<T>(T obj, string requiredChildElementName, List<IError> errors)
        {
            if (obj == null)
            {
                errors.Add(new Error() { Description = $"{this.GetType().Name} requires the {requiredChildElementName}." });
                return false;
            }
            return true;
        }

        private bool Require<P>(P propertyValue, string className, string propertyName, List<IError> errors, string attributeName = null)
        {
            if (propertyValue == null)
            {
                string parenthesis = attributeName != null ? $" ({attributeName})" : string.Empty;
                errors.Add(new Error() { Description = $"{className}.{propertyName}{parenthesis} is required." });
                return false;
            }
            return true;
        }

        private bool ValidateString<P>(P propertyValue, int maxLength, string className, string propertyName, List<IError> errors, string attributeName = null)
        {
            if (propertyValue != null)
            {
                string stringValue = propertyValue.ToString();
                if (stringValue.Length > maxLength)
                {
                    string parenthesis = attributeName != null ? $" ({attributeName})" : string.Empty;
                    errors.Add(new Error() { Description = $"{stringValue} in {className}.{propertyName}{parenthesis} is too long.  Max Length: {maxLength}." });
                    return false;
                }
                return true;   
            }
            return true;
        }

        private bool ValidateRange<P>(P propertyValue, string className, string propertyName, P min, P max, List<IError> errors, string attributeName = null)
        {
            if (typeof(P) == typeof(Int32))
            {
                int value = Int32.Parse(propertyValue.ToString());
                int minVal = Int32.Parse(min.ToString());
                int maxVal = Int32.Parse(max.ToString());
                if (value < minVal || value > maxVal)
                {
                    FailRangeValidation(errors, value.ToString(), className, propertyName, attributeName, minVal.ToString(), maxVal.ToString());
                    return false;
                }
            }
            else if (typeof(P) == typeof(UInt32))
            {
                uint value = UInt32.Parse(propertyValue.ToString());
                uint minVal = UInt32.Parse(min.ToString());
                uint maxVal = UInt32.Parse(max.ToString());
                if (value < minVal || value > maxVal)
                {
                    FailRangeValidation(errors, value.ToString(), className, propertyName, attributeName, minVal.ToString(), maxVal.ToString());
                    return false;
                }
            }
            else if (typeof(P) == typeof(byte))
            {
                byte value = Byte.Parse(propertyValue.ToString());
                byte minVal = Byte.Parse(min.ToString());
                byte maxVal = Byte.Parse(max.ToString());
                if (value < minVal || value > maxVal)
                {
                    FailRangeValidation(errors, value.ToString(), className, propertyName, attributeName, minVal.ToString(), maxVal.ToString());
                    return false;
                }
            }
            else if (typeof(P) == typeof(double))
            {
                double value = Double.Parse(propertyValue.ToString());
                double minVal = Double.Parse(min.ToString());
                double maxVal = Double.Parse(max.ToString());
                if (value < minVal || value > maxVal)
                {
                    FailRangeValidation(errors, value.ToString(), className, propertyName, attributeName, minVal.ToString(), maxVal.ToString());
                    return false;
                }
            }
            else if (typeof(P) == typeof(decimal))
            {
                decimal value = Decimal.Parse(propertyValue.ToString());
                decimal minVal = Decimal.Parse(min.ToString());
                decimal maxVal = Decimal.Parse(max.ToString());
                if (value < minVal || value > maxVal)
                {
                    FailRangeValidation(errors, value.ToString(), className, propertyName, attributeName, minVal.ToString(), maxVal.ToString());
                    return false;
                }
            }
            return true;
        }

        private void FailRangeValidation(List<IError> errors, string value, string typeName, string propertyName, string attributeName, string minVal, string maxVal)
        {
            string parenthesis = attributeName != null ? $" ({attributeName})" : string.Empty;
            errors.Add(new Error() { Description = $"Value {value} is out of range for {typeName}.{propertyName}{parenthesis}. Required Range: {minVal}-{maxVal}" });
        }
        #endregion Validation
    }
}
