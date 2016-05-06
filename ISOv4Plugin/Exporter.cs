using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ISOv4Plugin.ExportMappers;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin
{
    public interface IExporter
    {
        ISO11783_TaskData Export(ApplicationDataModel.ADM.ApplicationDataModel applicationDataModel, string datacardPath, ISO11783_TaskData isoTaskData);
    }

    public class Exporter : IExporter
    {
        private readonly IGrowerFarmFieldMapper _growerFarmFieldMapper;
        private readonly ICropZoneMapper _cropZoneMapper;
        private readonly ICropTypeMapper _cropTypeMapper;
        private readonly ITaskMapper _taskMapper;

        public Exporter()
            : this(new GrowerFarmFieldMapper(), new CropZoneMapper(), new CropTypeMapper(), new TaskMapper())
        {
        }

        public Exporter(IGrowerFarmFieldMapper growerFarmFieldMapper, ICropZoneMapper cropZoneMapper, ICropTypeMapper cropTypeMapper, ITaskMapper taskMapper)
        {
            _growerFarmFieldMapper = growerFarmFieldMapper;
            _cropZoneMapper = cropZoneMapper;
            _cropTypeMapper = cropTypeMapper;
            _taskMapper = taskMapper;
        }

        public ISO11783_TaskData Export(ApplicationDataModel.ADM.ApplicationDataModel applicationDataModel, string datacardPath, ISO11783_TaskData isoTaskData)
        {
            if (applicationDataModel == null)
                return isoTaskData;

            var keyToIsoId = new Dictionary<int, string>();
            var setupCatalog = applicationDataModel.Catalog;
            var items = new List<object>();
            if(applicationDataModel.Catalog != null)
            {
                var isoGrowers = _growerFarmFieldMapper.Map(setupCatalog.Growers, keyToIsoId);
                if(isoGrowers != null)
                    items.AddRange(isoGrowers);

                var isoFarms = _growerFarmFieldMapper.Map(setupCatalog.Farms, keyToIsoId);
                if(isoFarms != null)
                    items.AddRange(isoFarms);

                var isoFields = _growerFarmFieldMapper.Map(setupCatalog.Fields, keyToIsoId, setupCatalog);
                if(isoFields != null)
                    items.AddRange(isoFields);

                var isoCrops = _cropTypeMapper.Map(setupCatalog.Crops, keyToIsoId, setupCatalog);
                if(isoCrops != null)
                    items.AddRange(isoCrops);

                var cropZones = _cropZoneMapper.Map(setupCatalog.CropZones, isoFields, keyToIsoId, setupCatalog);
                if(cropZones != null)
                    items.AddRange(cropZones);
            }

            var numberOfExistingTasks = isoTaskData.Items != null
                ? isoTaskData.Items.Count(x => x.GetType() == typeof (TSK))
                : 0;
            var tasks = applicationDataModel.Documents == null ? null : _taskMapper.Map(applicationDataModel.Documents.LoggedData, applicationDataModel.Catalog, datacardPath, numberOfExistingTasks);
            if(tasks != null)
                items.AddRange(tasks);


            isoTaskData.DataTransferOrigin = ISO11783_TaskDataDataTransferOrigin.Item1;
            isoTaskData.ManagementSoftwareVersion = "0.1";
            isoTaskData.ManagementSoftwareManufacturer = "AgGateway ADAPT";
            isoTaskData.VersionMajor = 4;
            isoTaskData.VersionMinor = 1;

            isoTaskData.Items = isoTaskData.Items != null ? items.Concat(isoTaskData.Items).ToArray() : items.ToArray();

            return isoTaskData;
        }
    }
}