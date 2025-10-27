using System;
using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;



namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    internal class ISOOperationData : OperationData, IConvertible
    {
        /// <summary>
        /// An internal list of DeviceElementUses that are may be updated during import; not exposed on the public interface.
        /// </summary>
        internal List<DeviceElementUse> DeviceElementUses { get; set; } = new List<DeviceElementUse>();

        #region IConvertible impelementation
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider) => throw new NotImplementedException();

        public byte ToByte(IFormatProvider provider) => throw new NotImplementedException();

        public char ToChar(IFormatProvider provider) => throw new NotImplementedException();

        public DateTime ToDateTime(IFormatProvider provider) => throw new NotImplementedException();

        public decimal ToDecimal(IFormatProvider provider) => throw new NotImplementedException();

        public double ToDouble(IFormatProvider provider) => throw new NotImplementedException();

        public short ToInt16(IFormatProvider provider) => throw new NotImplementedException();

        public int ToInt32(IFormatProvider provider) => throw new NotImplementedException();

        public long ToInt64(IFormatProvider provider) => throw new NotImplementedException();

        public sbyte ToSByte(IFormatProvider provider) => throw new NotImplementedException();

        public float ToSingle(IFormatProvider provider) => throw new NotImplementedException();

        public string ToString(IFormatProvider provider) => throw new NotImplementedException();

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(ISOOperationData) || conversionType == typeof(OperationData))
            {
                return this;
            }
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider) => throw new NotImplementedException();

        public uint ToUInt32(IFormatProvider provider) => throw new NotImplementedException();

        public ulong ToUInt64(IFormatProvider provider) => throw new NotImplementedException();
        #endregion
    }
}
