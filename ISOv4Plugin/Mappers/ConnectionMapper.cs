/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ApplicationDataModel.Documents;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IConnectionMapper
    {
        IEnumerable<ISOConnection> ExportConnections(ISOTask task, IEnumerable<EquipmentConfiguration> adaptConnections);
        ISOConnection ExportConnection(ISOTask task, EquipmentConfiguration adaptConnection);

        IEnumerable<EquipmentConfiguration> ImportConnections(ISOTask isoTask);
        EquipmentConfiguration ImportConnection(ISOTask isoTask, ISOConnection isoConnection);
    }

    public class ConnectionMapper : BaseMapper, IConnectionMapper
    {
        public ConnectionMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CNN")
        {
        }

        #region Export
        public IEnumerable<ISOConnection> ExportConnections(ISOTask task, IEnumerable<EquipmentConfiguration> adaptConnections)
        {
            List <ISOConnection> connections = new List<ISOConnection>();
            foreach (EquipmentConfiguration adaptConnection in adaptConnections)
            {
                ISOConnection isoConnection = ExportConnection(task, adaptConnection);
                connections.Add(isoConnection);
            }
            return connections;
        }

        public ISOConnection ExportConnection(ISOTask task, EquipmentConfiguration adaptConnection)
        {
            ISOConnection isoConnection = new ISOConnection();

            //First Connector
            Connector connector1 = DataModel.Catalog.Connectors.FirstOrDefault(c => c.Id.ReferenceId == adaptConnection.Connector1Id);
            if (connector1 != null)
            {
                DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(c => c.Id.ReferenceId == connector1.DeviceElementConfigurationId);
                if (config != null)
                {
                    string isoDeviceElementID = TaskDataMapper.InstanceIDMap.GetISOID(config.DeviceElementId);
                    DeviceElement deviceElement = DataModel.Catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == config.DeviceElementId);
                    if (deviceElement != null)
                    {
                        string isoDeviceID = TaskDataMapper.InstanceIDMap.GetISOID(deviceElement.DeviceModelId);
                        if (!string.IsNullOrEmpty(isoDeviceElementID) && !string.IsNullOrEmpty(isoDeviceElementID))
                        {
                            isoConnection.DeviceIdRef_0 = isoDeviceID;
                            isoConnection.DeviceElementIdRef_0 = TaskDataMapper.InstanceIDMap.GetISOID(connector1.Id.ReferenceId); //We want to refer to the Connector DeviceElement, not its parent referred by the config element
                        }
                    }
                }
            }

            //Second Connector
            Connector connector2 = DataModel.Catalog.Connectors.FirstOrDefault(c => c.Id.ReferenceId == adaptConnection.Connector2Id);
            if (connector2 != null)
            {
                DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(c => c.Id.ReferenceId == connector2.DeviceElementConfigurationId);
                if (config != null)
                {
                    string isoDeviceElementID = TaskDataMapper.InstanceIDMap.GetISOID(config.DeviceElementId);
                    DeviceElement deviceElement = DataModel.Catalog.DeviceElements.FirstOrDefault(d => d.Id.ReferenceId == config.DeviceElementId);
                    if (deviceElement != null)
                    {
                        string isoDeviceID = TaskDataMapper.InstanceIDMap.GetISOID(deviceElement.DeviceModelId);
                        if (!string.IsNullOrEmpty(isoDeviceElementID) && !string.IsNullOrEmpty(isoDeviceElementID))
                        {
                            isoConnection.DeviceIdRef_1 = isoDeviceID;
                            isoConnection.DeviceElementIdRef_1 = TaskDataMapper.InstanceIDMap.GetISOID(connector2.Id.ReferenceId);
                        }
                    }
                }
            }

            //DataLogTriggers
            if (adaptConnection.DataLogTriggers.Any())
            {
                DataLogTriggerMapper dltMapper = new DataLogTriggerMapper(TaskDataMapper);
                task.DataLogTriggers = dltMapper.ExportDataLogTriggers(adaptConnection.DataLogTriggers).ToList();
            }

            return isoConnection;
        }

        #endregion Export 

        #region Import

        public IEnumerable<EquipmentConfiguration> ImportConnections(ISOTask isoTask)
        {
            List<EquipmentConfiguration> equipConfigs = new List<EquipmentConfiguration>();
            foreach (ISOConnection connection in isoTask.Connections)
            {
                EquipmentConfiguration equipConfig = ImportConnection(isoTask, connection);
                equipConfigs.Add(equipConfig);
            }
            return equipConfigs;
        }

        public EquipmentConfiguration ImportConnection(ISOTask task, ISOConnection isoConnection)
        {
            EquipmentConfiguration equipConfig = new EquipmentConfiguration();
            StringBuilder descriptionBuilder = new StringBuilder();

            //First Device Element
            int? connectorID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoConnection.DeviceElementIdRef_0);
            Connector adaptConnector1 = null;
            if (connectorID.HasValue)
            {
                adaptConnector1 = DataModel.Catalog.Connectors.SingleOrDefault(d => d.Id.ReferenceId == connectorID.Value);
                if (adaptConnector1 != null)
                {
                    equipConfig.Connector1Id = adaptConnector1.Id.ReferenceId;

                    ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(isoConnection.DeviceElementIdRef_0);
                    descriptionBuilder.Append(isoDeviceElement.Device.DeviceDesignator);
                    descriptionBuilder.Append(":");
                    descriptionBuilder.Append(isoDeviceElement.DeviceElementDesignator);
                }
            }
            if (adaptConnector1 == null)
            {
                descriptionBuilder.Append("Unknown");
            }

            descriptionBuilder.Append("<->");

            //Second Device Element
            Connector adaptConnector2 = null;
            connectorID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoConnection.DeviceElementIdRef_1);
            if (connectorID.HasValue)
            {
                adaptConnector2 = DataModel.Catalog.Connectors.SingleOrDefault(d => d.Id.ReferenceId == connectorID.Value);
                if (adaptConnector2 != null)
                {
                    equipConfig.Connector2Id = adaptConnector2.Id.ReferenceId;

                    ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(isoConnection.DeviceElementIdRef_1);
                    descriptionBuilder.Append(isoDeviceElement.Device.DeviceDesignator);
                    descriptionBuilder.Append(":");
                    descriptionBuilder.Append(isoDeviceElement.DeviceElementDesignator);
                }
            }
            if (adaptConnector2 == null)
            {
                descriptionBuilder.Append("Unknown");
            }

            equipConfig.Description = descriptionBuilder.ToString();

            //DataLogTriggers
            if (task.DataLogTriggers.Any())
            {
                DataLogTriggerMapper dltMapper = new DataLogTriggerMapper(TaskDataMapper);
                dltMapper.ImportDataLogTriggers(task.DataLogTriggers);
            }

            return equipConfig;
        }
        #endregion Import
    }
}
