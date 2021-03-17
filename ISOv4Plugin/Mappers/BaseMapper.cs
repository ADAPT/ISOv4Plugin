/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ISOv4Plugin.ISOModels;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using AgGateway.ADAPT.ISOv4Plugin.Representation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public class BaseMapper
    {
        private int _itemId;
        protected string XmlPrefix { get; private set; }
        protected TaskDataMapper TaskDataMapper { get; private set; }
        public string TaskDataPath { get { return TaskDataMapper.BaseFolder; } }

        protected ApplicationDataModel.ADM.ApplicationDataModel DataModel { get; set; }
        protected ISO11783_TaskData ISOTaskData { get; set; }
        private UniqueIdMapper UniqueIDMapper { get; set; }
        protected RepresentationMapper RepresentationMapper { get; private set; }
        internal Dictionary<int, DdiDefinition> DDIs { get; private set; }
        internal DeviceOperationTypes DeviceOperationTypes { get; private set; }

        protected BaseMapper(TaskDataMapper taskDataMapper, string xmlPrefix, int startId = 1)
        {
            TaskDataMapper = taskDataMapper;
            XmlPrefix = xmlPrefix;
            _itemId = startId;

            DataModel = TaskDataMapper.AdaptDataModel;
            ISOTaskData = TaskDataMapper.ISOTaskData;
            UniqueIDMapper = TaskDataMapper.UniqueIDMapper;

            RepresentationMapper = TaskDataMapper.RepresentationMapper;
            DDIs = taskDataMapper.DDIs;
            DeviceOperationTypes = taskDataMapper.DeviceOperationTypes;
        }

        protected string GenerateId(byte idLength = 0)
        {
            return GenerateId(idLength, XmlPrefix, _itemId++);
        }

        public static string GenerateId(byte idLength, string xmlPrefix, int itemID)
        {
            var formatString = string.Format(CultureInfo.InvariantCulture, "{{0}}{{1:D{0}}}", idLength == 0 ? 0 : idLength);
            return string.Format(CultureInfo.InvariantCulture, formatString, xmlPrefix, itemID);
        }

        protected bool ExportIDs(CompoundIdentifier id, string isoIDRef)
        {
            UniqueIDMapper.ExportUniqueIDs(id, isoIDRef);
            return TaskDataMapper.InstanceIDMap.Add(id.ReferenceId, isoIDRef);
        }

        protected bool ImportIDs(CompoundIdentifier id, string isoIDRef)
        {
            id.UniqueIds.AddRange(UniqueIDMapper.ImportUniqueIDs(isoIDRef));
            return TaskDataMapper.InstanceIDMap.Add(id.ReferenceId, isoIDRef);
        }

        protected void ExportContextItems(List<ContextItem> contextItems, string isoIDRef, string groupName, string prefix = "")
        {
            if (contextItems.Any())
            {
                List<string> errors = UniqueIDMapper.ExportContextItems(contextItems, isoIDRef, groupName, prefix);
                foreach (string error in errors)
                {
                    TaskDataMapper.AddError(error);
                }
            }
        }

        protected List<ContextItem> ImportContextItems(string isoIDRef, string linkGroupDescription)
        {
            return UniqueIDMapper.ImportContextItems(isoIDRef, linkGroupDescription);
        }
    }
}
