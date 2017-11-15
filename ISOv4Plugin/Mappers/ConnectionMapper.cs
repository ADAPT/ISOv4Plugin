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
        IEnumerable<ISOConnection> ExportConnections(int loggedDataOrWorkItemID, IEnumerable<EquipmentConfiguration> adaptConnections);
        ISOConnection ExportConnection(int loggedDataOrWorkItemID, EquipmentConfiguration adaptConnection);

        IEnumerable<EquipmentConfiguration> ImportConnections(ISOTask isoTask);
        EquipmentConfiguration ImportConnection(ISOTask isoTask, ISOConnection isoConnection);
    }

    public class ConnectionMapper : BaseMapper, IConnectionMapper
    {
        public ConnectionMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "CNN")
        {
        }

        #region Export
        public IEnumerable<ISOConnection> ExportConnections(int loggedDataOrWorkItemID, IEnumerable<EquipmentConfiguration> adaptConnections)
        {
            List <ISOConnection> connections = new List<ISOConnection>();
            foreach (EquipmentConfiguration adaptConnection in adaptConnections)
            {
                ISOConnection isoConnection = ExportConnection(loggedDataOrWorkItemID, adaptConnection);
                connections.Add(isoConnection);
            }
            return connections;
        }

        public ISOConnection ExportConnection(int loggedDataOrWorkItemID, EquipmentConfiguration adaptConnection)
        {
            ISOConnection isoConnection = new ISOConnection();

            //First Connector
            Connector connector1 = DataModel.Catalog.Connectors.FirstOrDefault(c => c.Id.ReferenceId == adaptConnection.Connector1Id);
            if (connector1 != null)
            {
                DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(c => c.Id.ReferenceId == connector1.DeviceElementConfigurationId);
                if (config != null)
                {
                    string isoDeviceElementID = TaskDataMapper.ISOIdMap.FindByADAPTId(config.DeviceElementId);
                    if (!string.IsNullOrEmpty(isoDeviceElementID))
                    {
                        isoConnection.DeviceElementIdRef_0 = isoDeviceElementID;
                        DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceElementHierarchies.GetRelevantHierarchy(isoDeviceElementID);
                        isoConnection.DeviceIdRef_0 = deviceElementHierarchy.DeviceElement.Device.DeviceId;
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
                    string isoDeviceElementID = TaskDataMapper.ISOIdMap.FindByADAPTId(config.DeviceElementId);
                    if (!string.IsNullOrEmpty(isoDeviceElementID))
                    {
                        isoConnection.DeviceElementIdRef_1 = isoDeviceElementID;
                        DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceElementHierarchies.GetRelevantHierarchy(isoDeviceElementID);
                        isoConnection.DeviceIdRef_1 = deviceElementHierarchy.DeviceElement.Device.DeviceId;
                    }
                }
            }

            //DataLogTriggers
            if (adaptConnection.DataLogTriggers.Any())
            {
                string taskID = TaskDataMapper.ISOIdMap.FindByADAPTId(loggedDataOrWorkItemID);
                ISOTask task = TaskDataMapper.ISOTaskData.ChildElements.OfType<ISOTask>().First(t => t.TaskID == taskID);
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
            int? connectorID = TaskDataMapper.ADAPTIdMap.FindByISOId(isoConnection.DeviceElementIdRef_0);
            if (connectorID.HasValue)
            {
                Connector adaptConnector1 = DataModel.Catalog.Connectors.Single(d => d.Id.ReferenceId == connectorID.Value);
                equipConfig.Connector1Id = adaptConnector1.Id.ReferenceId;

                ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(isoConnection.DeviceElementIdRef_0);
                descriptionBuilder.Append(isoDeviceElement.Device.DeviceDesignator);
                descriptionBuilder.Append(":");
                descriptionBuilder.Append(isoDeviceElement.DeviceElementDesignator);
            }
            else
            {
                descriptionBuilder.Append("Unknown");
            }

            descriptionBuilder.Append("<->");

            //Second Device Element
            connectorID = TaskDataMapper.ADAPTIdMap.FindByISOId(isoConnection.DeviceElementIdRef_1);
            if (connectorID.HasValue)
            {
                Connector adaptConnector2 = DataModel.Catalog.Connectors.Single(d => d.Id.ReferenceId == connectorID.Value);
                equipConfig.Connector2Id = adaptConnector2.Id.ReferenceId;

                ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceElementHierarchies.GetISODeviceElementFromID(isoConnection.DeviceElementIdRef_1);
                descriptionBuilder.Append(isoDeviceElement.Device.DeviceDesignator);
                descriptionBuilder.Append(":");
                descriptionBuilder.Append(isoDeviceElement.DeviceElementDesignator);
            }
            else
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
