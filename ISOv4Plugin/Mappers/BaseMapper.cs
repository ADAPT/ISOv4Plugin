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
        private UniqueIdMapper IDMapper { get; set; }
        protected RepresentationMapper RepresentationMapper { get; private set; }
        internal Dictionary<int, DdiDefinition> DDIs { get; private set; }

        protected BaseMapper(TaskDataMapper taskDataMapper, string xmlPrefix, int startId = 1)
        {
            TaskDataMapper = taskDataMapper;
            XmlPrefix = xmlPrefix;
            _itemId = startId;

            DataModel = TaskDataMapper.AdaptDataModel;
            ISOTaskData = TaskDataMapper.ISOTaskData;
            IDMapper = TaskDataMapper.UniqueIDMapper;

            RepresentationMapper = TaskDataMapper.RepresentationMapper;
            DDIs = taskDataMapper.DDIs;
        }

        protected string GenerateId(byte idLength = 0)
        {
            var formatString = string.Format(CultureInfo.InvariantCulture, "{{0}}{{1:D{0}}}", idLength == 0 ? 0 : idLength);
            return string.Format(CultureInfo.InvariantCulture, formatString, XmlPrefix, _itemId++);
        }

        protected void ExportUniqueIDs(CompoundIdentifier id, string isoIDRef)
        {
            IDMapper.ExportUniqueIDs(id, isoIDRef);
        }

        protected IEnumerable<UniqueId> ImportUniqueIDs(string isoObjectIdRef)
        {
            return IDMapper.ImportUniqueIDs(isoObjectIdRef);
        }
    }
}
