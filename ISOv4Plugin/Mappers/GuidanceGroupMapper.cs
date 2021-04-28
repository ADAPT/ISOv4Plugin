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

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IGuidanceGroupMapper
    {
        IEnumerable<ISOGuidanceGroup> ExportGuidanceGroups(IEnumerable<GuidanceGroup> adaptGuidanceGroups);
        ISOGuidanceGroup ExportGuidanceGroup(GuidanceGroup adaptGuidanceGroup);
        IEnumerable<GuidanceGroup> ImportGuidanceGroups(IEnumerable<ISOGuidanceGroup> isoGuidanceGroups);
        GuidanceGroup ImportGuidanceGroup(ISOGuidanceGroup isoGuidanceGroup);
    }

    public class GuidanceGroupMapper : BaseMapper, IGuidanceGroupMapper
    {
        public GuidanceGroupMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "GGP")
        {
        }

        #region Export
        public IEnumerable<ISOGuidanceGroup> ExportGuidanceGroups(IEnumerable<GuidanceGroup> adaptGuidanceGroups)
        {
            List <ISOGuidanceGroup> groups = new List<ISOGuidanceGroup>();
            foreach (GuidanceGroup group in adaptGuidanceGroups)
            {
                ISOGuidanceGroup isoGroup = ExportGuidanceGroup(group);
                groups.Add(isoGroup);
            }
            return groups;
        }

        public ISOGuidanceGroup ExportGuidanceGroup(GuidanceGroup adaptGuidanceGroup)
        {
            ISOGuidanceGroup isoGroup = new ISOGuidanceGroup();

            //ID
            string id = adaptGuidanceGroup.Id.FindIsoId() ?? GenerateId();
            isoGroup.GuidanceGroupId = id;
            ExportIDs(adaptGuidanceGroup.Id, id);

            //Designator
            isoGroup.GuidanceGroupDesignator = adaptGuidanceGroup.Description;

            //Patterns
            if (adaptGuidanceGroup.GuidancePatternIds.Any())
            { 
                List<GuidancePattern> patterns = new List<GuidancePattern>();
                foreach (int adaptID in adaptGuidanceGroup.GuidancePatternIds)
                {
                    GuidancePattern adaptPattern = DataModel.Catalog.GuidancePatterns.SingleOrDefault(p => p.Id.ReferenceId == adaptID);
                    if (adaptPattern != null)
                    {
                        patterns.Add(adaptPattern);
                    }
                }
                GuidancePatternMapper patternMapper = TaskDataMapper.GuidancePatternMapper;
                isoGroup.GuidancePatterns = patternMapper.ExportGuidancePatterns(patterns).ToList();
            }

            //Boundaries
            if (adaptGuidanceGroup.BoundingPolygon != null)
            { 
                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                isoGroup.BoundaryPolygons = polygonMapper.ExportPolygons(adaptGuidanceGroup.BoundingPolygon.Polygons, ISOPolygonType.Other).ToList();
            }

            return isoGroup;
        }

        #endregion Export 

        #region Import

        public IEnumerable<GuidanceGroup> ImportGuidanceGroups(IEnumerable<ISOGuidanceGroup> isoGuidanceGroups)
        {
            //Import groups
            List<GuidanceGroup> adaptGuidanceGroups = new List<GuidanceGroup>();
            foreach (ISOGuidanceGroup isoGuidanceGroup in isoGuidanceGroups)
            {
                if (!TaskDataMapper.InstanceIDMap.GetADAPTID(isoGuidanceGroup.GuidanceGroupId).HasValue)
                {
                    GuidanceGroup adaptGuidanceGroup = ImportGuidanceGroup(isoGuidanceGroup);
                    adaptGuidanceGroups.Add(adaptGuidanceGroup);
                }
            }

            //Add the groups to the Catalog
            if (adaptGuidanceGroups.Any())
            { 
                if (DataModel.Catalog.GuidanceGroups == null)
                {
                    DataModel.Catalog.GuidanceGroups = new List<GuidanceGroup>();
                }
                DataModel.Catalog.GuidanceGroups.AddRange(adaptGuidanceGroups);
            }

            return adaptGuidanceGroups;
        }

        public GuidanceGroup ImportGuidanceGroup(ISOGuidanceGroup isoGuidanceGroup)
        {
            GuidanceGroup adaptGroup = new GuidanceGroup();

            //ID
            ImportIDs(adaptGroup.Id, isoGuidanceGroup.GuidanceGroupId);

            //Description
            adaptGroup.Description = isoGuidanceGroup.GuidanceGroupDesignator;

            //Patterns
            if (isoGuidanceGroup.GuidancePatterns.Any())
            { 
                GuidancePatternMapper patternMapper = new GuidancePatternMapper(TaskDataMapper);
                adaptGroup.GuidancePatternIds = patternMapper.ImportGuidancePatterns(isoGuidanceGroup.GuidancePatterns).Select(p => p.Id.ReferenceId).ToList();
            }

            //Boundaries
            if (isoGuidanceGroup.BoundaryPolygons != null)
            {
                PolygonMapper polygonMapper = new PolygonMapper(TaskDataMapper);
                adaptGroup.BoundingPolygon = new MultiPolygon();
                adaptGroup.BoundingPolygon.Polygons = new List<Polygon>();
                adaptGroup.BoundingPolygon.Polygons.AddRange(polygonMapper.ImportBoundaryPolygons(isoGuidanceGroup.BoundaryPolygons));
            }

            return adaptGroup;
        }
        #endregion Import
    }
}
