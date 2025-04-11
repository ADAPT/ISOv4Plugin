using System.Collections.Generic;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;

internal class ISOOperationData : OperationData
{
    /// <summary>
    /// An internal list of DeviceElementUses that are may be updated during import; not exposed on the public interface.
    /// </summary>
   internal List<DeviceElementUse> DeviceElementUses { get; set; } = new List<DeviceElementUse>();
}