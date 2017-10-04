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
using AgGateway.ADAPT.Representation.UnitSystem;
using AgGateway.ADAPT.Representation.UnitSystem.ExtensionMethods;
using AgGateway.ADAPT.ApplicationDataModel.FieldBoundaries;
using AgGateway.ADAPT.ApplicationDataModel.Shapes;
using AgGateway.ADAPT.ApplicationDataModel.Guidance;
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.Representation.RepresentationSystem;
using AgGateway.ADAPT.Representation.RepresentationSystem.ExtensionMethods;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IPartfieldMapper
    {
        IEnumerable<ISOPartfield> ExportFields(IEnumerable<Field> adaptFields);
        ISOPartfield ExportField(Field adaptField);
        IEnumerable<ISOPartfield> ExportCropZones(IEnumerable<CropZone> cropZones);
        ISOPartfield ExportCropZone(CropZone cropZone);

        IEnumerable<Field> ImportFields(IEnumerable<ISOPartfield> isoFields);
        Field ImportField(ISOPartfield isoField);
        IEnumerable<CropZone> ImportCropZones(IEnumerable<ISOPartfield> isoFields);
        CropZone ImportCropZone(ISOPartfield isoField);
    }

    public class PartfieldMapper : BaseMapper, IPartfieldMapper
    {
        public PartfieldMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "PFD")
        {
        }

        #region Export
        public IEnumerable<ISOPartfield> ExportFields(IEnumerable<Field> adaptFields)
        {
            List <ISOPartfield> isoFields = new List<ISOPartfield>();
            foreach (Field field in adaptFields)
            {
                ISOPartfield isoField = ExportField(field);
                isoFields.Add(isoField);
            }
            return isoFields;
        }

        public IEnumerable<ISOPartfield> ExportCropZones(IEnumerable<CropZone> cropZones)
        {
            List<ISOPartfield> isoFields = new List<ISOPartfield>();
            foreach (CropZone cropZone in cropZones)
            {
                ISOPartfield isoField = ExportCropZone(cropZone);
                isoFields.Add(isoField);
            }
            return isoFields;
        }

        public ISOPartfield ExportField(Field adaptField)
        {
            ISOPartfield isoField = new ISOPartfield();

            //Field ID
            string fieldID = adaptField.Id.FindIsoId() ?? GenerateId();
            isoField.PartfieldID = fieldID;
            ExportUniqueIDs(adaptField.Id, fieldID);
            TaskDataMapper.ISOIdMap.Add(adaptField.Id.ReferenceId, fieldID);

            //Customer & Farm ID
            if (adaptField.FarmId.HasValue)
            { 
                isoField.FarmIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(adaptField.FarmId.Value);
                Farm adaptFarm = DataModel.Catalog.Farms.FirstOrDefault(f => f.Id.ReferenceId == adaptField.FarmId);
                if (adaptFarm != null)
                {
                    isoField.CustomerIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(adaptFarm.GrowerId.Value);
                }
            }

            //isoField.PartfieldCode = ?  

            //Area
            if (adaptField.Area != null)
            { 
                isoField.PartfieldArea = (long)(adaptField.Area.Value.ConvertToUnit(new CompositeUnitOfMeasure("m2")));
            }

            //Name
            isoField.PartfieldDesignator = adaptField.Description;

            //Boundary
            PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
            FieldBoundary boundary = DataModel.Catalog.FieldBoundaries.SingleOrDefault(b => b.FieldId == adaptField.Id.ReferenceId);
            if (boundary != null)
            {
                IEnumerable<ISOPolygon> isoPolygons = polygonMapper.ExportPolygons(boundary.SpatialData.Polygons, ISOEnumerations.ISOPolygonType.PartfieldBoundary);
                isoField.Polygons.AddRange(isoPolygons);
            }

            //Guidance
            if (DataModel.Catalog.GuidanceGroups != null)
            {
                List<GuidanceGroup> groups = new List<GuidanceGroup>();
                foreach (int groupID in adaptField.GuidanceGroupIds)
                {
                    GuidanceGroup group = DataModel.Catalog.GuidanceGroups.SingleOrDefault(g => g.Id.ReferenceId == groupID);
                    if (group != null)
                    {
                        groups.Add(group);

                    }
                }
                GuidanceGroupMapper ggpMapper = new GuidanceGroupMapper(TaskDataMapper);
                isoField.GuidanceGroups = ggpMapper.ExportGuidanceGroups(groups).ToList();
            }

            //TODO any obstacle, flag, entry, etc. data

            return isoField;
        }

        public ISOPartfield ExportCropZone(CropZone cropZone)
        {
            ISOPartfield isoField = new ISOPartfield();

            //Field ID
            string fieldID = cropZone.Id.FindIsoId() ?? GenerateId();
            isoField.PartfieldID = fieldID;
            ExportUniqueIDs(cropZone.Id, fieldID);
            TaskDataMapper.ISOIdMap.Add(cropZone.Id.ReferenceId, fieldID);

            //Parent Field ID
            isoField.FieldIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(cropZone.FieldId);

            //Area
            if(cropZone.Area != null)
            { 
                isoField.PartfieldArea = (long)(cropZone.Area.Value.ConvertToUnit(new CompositeUnitOfMeasure("m2")));
            }

            //Name
            isoField.PartfieldDesignator = cropZone.Description;

            //Boundary
            if (cropZone.BoundingRegion != null)
            {
                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                isoField.Polygons = polygonMapper.ExportPolygons(cropZone.BoundingRegion.Polygons, ISOEnumerations.ISOPolygonType.PartfieldBoundary).ToList();
            }

            //Guidance
            if (DataModel.Catalog.GuidanceGroups != null)
            {
                List<GuidanceGroup> groups = new List<GuidanceGroup>();
                foreach (int groupID in cropZone.GuidanceGroupIds)
                {
                    GuidanceGroup group = DataModel.Catalog.GuidanceGroups.SingleOrDefault(g => g.Id.ReferenceId == groupID);
                    if (group != null)
                    {
                        groups.Add(group);

                    }
                }
                GuidanceGroupMapper ggpMapper = new GuidanceGroupMapper(TaskDataMapper);
                isoField.GuidanceGroups = ggpMapper.ExportGuidanceGroups(groups).ToList();
            }

            if (cropZone.CropId.HasValue && TaskDataMapper.ISOIdMap.ContainsKey(cropZone.CropId.Value))
            {
                isoField.CropTypeIdRef = TaskDataMapper.ISOIdMap[cropZone.CropId.Value];
            }

            return isoField;
        }
        #endregion Export 

        #region Import

        public IEnumerable<Field> ImportFields(IEnumerable<ISOPartfield> ISOPartfields)
        {
            List<Field> adaptFields = new List<Field>();
            foreach (ISOPartfield isoPartField in ISOPartfields)
            {
                if (string.IsNullOrEmpty(isoPartField.FieldIdRef))
                {
                    Field adaptField = ImportField(isoPartField);
                    adaptFields.Add(adaptField);
                }
            }
            return adaptFields;
        }

        public IEnumerable<CropZone> ImportCropZones(IEnumerable<ISOPartfield> ISOPartfields)
        {
            List<CropZone> adaptCropzones = new List<CropZone>();
            foreach (ISOPartfield isoPartField in ISOPartfields)
            {
                if (!string.IsNullOrEmpty(isoPartField.FieldIdRef) || !String.IsNullOrEmpty(isoPartField.CropTypeIdRef))
                {
                    CropZone cropZone = ImportCropZone(isoPartField);
                    adaptCropzones.Add(cropZone);
                }
            }
            return adaptCropzones;
        }

        public Field ImportField(ISOPartfield isoPartfield)
        { 
            Field field = new Field();

            //Field ID
            field.Id.UniqueIds.AddRange(ImportUniqueIDs(isoPartfield.PartfieldID));
            TaskDataMapper.ADAPTIdMap.Add(isoPartfield.PartfieldID, field.Id.ReferenceId);

            //Farm ID
            field.FarmId = TaskDataMapper.ADAPTIdMap.FindByISOId(isoPartfield.FieldIdRef);

            //Area
            if (isoPartfield.PartfieldArea.HasValue)
            { 
                var numericValue = new NumericValue(new CompositeUnitOfMeasure("m2").ToModelUom(), (double)(isoPartfield.PartfieldArea.Value));
                field.Area = new NumericRepresentationValue(RepresentationInstanceList.vrReportedFieldArea.ToModelRepresentation(), numericValue.UnitOfMeasure, numericValue);
            }

            //Name
            field.Description = isoPartfield.PartfieldDesignator;

            //Boundary
            PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
            IEnumerable<Polygon> boundaryPolygons = polygonMapper.ImportPolygons(isoPartfield.Polygons).ToList();
            if (boundaryPolygons.Any())
            {
                MultiPolygon boundary = new MultiPolygon();
                boundary.Polygons = boundaryPolygons.ToList();
                FieldBoundary fieldBoundary = new FieldBoundary
                {
                    FieldId = field.Id.ReferenceId,
                    SpatialData = boundary,
                };

                //Add the boundary to the Catalog
                if (DataModel.Catalog.FieldBoundaries == null)
                {
                    DataModel.Catalog.FieldBoundaries = new List<FieldBoundary>();
                }
                DataModel.Catalog.FieldBoundaries.Add(fieldBoundary);

                field.ActiveBoundaryId = fieldBoundary.Id.ReferenceId;
            }

            //Guidance
            GuidanceGroupMapper guidanceGroupMapper = new GuidanceGroupMapper(TaskDataMapper);
            IEnumerable<GuidanceGroup> groups = guidanceGroupMapper.ImportGuidanceGroups(isoPartfield.GuidanceGroups);
            if (groups.Any())
            {
                field.GuidanceGroupIds = groups.Select(g => g.Id.ReferenceId).ToList();
            }

            //TODO any obstacle, flag, entry, etc. data

            return field;
        }

        public CropZone ImportCropZone(ISOPartfield isoPartfield)
        {
            CropZone cropZone = new CropZone();

            //Cropzone ID
            cropZone.Id.UniqueIds.AddRange(ImportUniqueIDs(isoPartfield.PartfieldID));
            if (!TaskDataMapper.ADAPTIdMap.ContainsKey(isoPartfield.PartfieldID)) 
            {
                TaskDataMapper.ADAPTIdMap.Add(isoPartfield.PartfieldID, cropZone.Id.ReferenceId);
            }

            //Field ID
            if (!string.IsNullOrEmpty(isoPartfield.FieldIdRef))
            {
                cropZone.FieldId = TaskDataMapper.ADAPTIdMap.FindByISOId(isoPartfield.FieldIdRef).Value;  //Cropzone has a defined parent field in the ISO XML
            }
            else
            {
                cropZone.FieldId = TaskDataMapper.ADAPTIdMap[isoPartfield.PartfieldID].Value;  //Field had a crop assigned and we created a single cropzone
            }

            //Area
            if (isoPartfield.PartfieldArea.HasValue)
            {
                var numericValue = new NumericValue(new CompositeUnitOfMeasure("m2").ToModelUom(), (double)(isoPartfield.PartfieldArea.Value));
                cropZone.Area = new NumericRepresentationValue(RepresentationInstanceList.vrReportedFieldArea.ToModelRepresentation(), numericValue.UnitOfMeasure, numericValue);
            }

            //Name
            cropZone.Description = isoPartfield.PartfieldDesignator;

            //Boundary
            PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
            IEnumerable<Polygon> boundaryPolygons = polygonMapper.ImportPolygons(isoPartfield.Polygons).ToList();
            if (boundaryPolygons.Any())
            {
                MultiPolygon boundary = new MultiPolygon();
                boundary.Polygons = boundaryPolygons.ToList();
                cropZone.BoundingRegion = boundary;
            }

            //Guidance
            GuidanceGroupMapper guidanceGroupMapper = new GuidanceGroupMapper(TaskDataMapper);
            IEnumerable<GuidanceGroup> groups = guidanceGroupMapper.ImportGuidanceGroups(isoPartfield.GuidanceGroups);
            if (groups.Any())
            { 
                if (DataModel.Catalog.GuidanceGroups == null)
                {
                    DataModel.Catalog.GuidanceGroups = new List<GuidanceGroup>();
                }
                DataModel.Catalog.GuidanceGroups.AddRange(groups);
                cropZone.GuidanceGroupIds = groups.Select(g => g.Id.ReferenceId).ToList();
            }

            //Crop
            if (!string.IsNullOrEmpty(isoPartfield.CropTypeIdRef) && TaskDataMapper.ADAPTIdMap.ContainsKey(isoPartfield.CropTypeIdRef))
            {
                cropZone.CropId = TaskDataMapper.ADAPTIdMap[isoPartfield.CropTypeIdRef];
            }

            return cropZone;
        }

        #endregion Import
    }
}
