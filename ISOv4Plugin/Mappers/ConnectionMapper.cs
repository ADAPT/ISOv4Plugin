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
                        DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceHierarchy.GetRelevantHierarchy(isoDeviceElementID);
                        isoConnection.DeviceIdRef_0 = deviceElementHierarchy.Device.DeviceId;
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
                        DeviceElementHierarchy deviceElementHierarchy = TaskDataMapper.DeviceHierarchy.GetRelevantHierarchy(isoDeviceElementID);
                        isoConnection.DeviceIdRef_1 = deviceElementHierarchy.Device.DeviceId;
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
            int? deviceElementID = TaskDataMapper.ADAPTIdMap.FindByISOId(isoConnection.DeviceElementIdRef_0);
            if (deviceElementID.HasValue)
            {
                DeviceElement adaptDeviceElement = DataModel.Catalog.DeviceElements.Single(d => d.Id.ReferenceId == deviceElementID.Value);
                ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceHierarchy.GetISODeviceElementFromID(isoConnection.DeviceElementIdRef_0);
                Connector adaptConnector1 = AddOrFindConnector(adaptDeviceElement, isoDeviceElement);
                if (adaptConnector1 != null)
                {
                    equipConfig.Connector1Id = adaptConnector1.Id.ReferenceId;
                }
                descriptionBuilder.Append(adaptDeviceElement.Description);
            }
            else
            {
                descriptionBuilder.Append("Unknown");
            }

            descriptionBuilder.Append("<->");

            //Second Device Element
            deviceElementID = TaskDataMapper.ADAPTIdMap.FindByISOId(isoConnection.DeviceElementIdRef_1);
            if (deviceElementID.HasValue)
            {
                DeviceElement adaptDeviceElement = DataModel.Catalog.DeviceElements.Single(d => d.Id.ReferenceId == deviceElementID.Value);
                ISODeviceElement isoDeviceElement = TaskDataMapper.DeviceHierarchy.GetISODeviceElementFromID(isoConnection.DeviceElementIdRef_1);
                Connector adaptConnector2 = AddOrFindConnector(adaptDeviceElement, isoDeviceElement);
                if (adaptConnector2 != null)
                {
                    equipConfig.Connector2Id = adaptConnector2.Id.ReferenceId;
                }
                descriptionBuilder.Append(adaptDeviceElement.Description);
            }
            else
            {
                descriptionBuilder.Append("Unknown");
            }

            //DataLogTriggers
            if (task.DataLogTriggers.Any())
            {
                DataLogTriggerMapper dltMapper = new DataLogTriggerMapper(TaskDataMapper);
                dltMapper.ImportDataLogTriggers(task.DataLogTriggers);
            }

            return equipConfig;
        }

        private Connector AddOrFindConnector(DeviceElement adaptDeviceElement, ISODeviceElement isoDeviceElement)
        {
            DeviceElementConfiguration config = DataModel.Catalog.DeviceElementConfigurations.FirstOrDefault(c => c.DeviceElementId == adaptDeviceElement.Id.ReferenceId);
            if (config == null)
            {
                config = DeviceElementMapper.AddDeviceElementConfiguration(isoDeviceElement, adaptDeviceElement, TaskDataMapper.DeviceHierarchy.GetRelevantHierarchy(isoDeviceElement.DeviceElementId), DataModel.Catalog);
            }
            Connector adaptConnector = DataModel.Catalog.Connectors.FirstOrDefault(c => c.DeviceElementConfigurationId == config.Id.ReferenceId);
            if (adaptConnector == null)
            {
                adaptConnector = new Connector() { DeviceElementConfigurationId = config.Id.ReferenceId };
                DataModel.Catalog.Connectors.Add(adaptConnector);
            }
            return adaptConnector;
        }
        #endregion Import
    }
}
