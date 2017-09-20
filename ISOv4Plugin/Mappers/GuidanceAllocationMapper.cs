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
    public interface IGuidanceAllocationMapper
    {
        IEnumerable<ISOGuidanceAllocation> ExportGuidanceAllocations(IEnumerable<GuidanceAllocation> adaptGuidanceAllocations);
        ISOGuidanceAllocation ExportGuidanceAllocation(GuidanceAllocation adaptGuidanceAllocation);
        IEnumerable<GuidanceAllocation> ImportGuidanceAllocations(IEnumerable<ISOGuidanceAllocation> isoGuidanceAllocations);
        GuidanceAllocation ImportGuidanceAllocation(ISOGuidanceAllocation isoGuidanceAllocation);
    }

    public class GuidanceAllocationMapper : BaseMapper, IGuidanceAllocationMapper
    {
        public GuidanceAllocationMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "GAN")
        {
        }

        #region Export
        public IEnumerable<ISOGuidanceAllocation> ExportGuidanceAllocations(IEnumerable<GuidanceAllocation> adaptGuidanceAllocations)
        {
            List <ISOGuidanceAllocation> groups = new List<ISOGuidanceAllocation>();
            foreach (GuidanceAllocation group in adaptGuidanceAllocations)
            {
                ISOGuidanceAllocation isoGroup = ExportGuidanceAllocation(group);
                groups.Add(isoGroup);
            }
            return groups;
        }

        public ISOGuidanceAllocation ExportGuidanceAllocation(GuidanceAllocation adaptGuidanceAllocation)
        {
            ISOGuidanceAllocation gan = new ISOGuidanceAllocation();

            //Group ID
            gan.GuidanceGroupIdRef = TaskDataMapper.ISOIdMap.FindByADAPTId(adaptGuidanceAllocation.GuidanceGroupId);

            //Allocation Stamps
            if (adaptGuidanceAllocation.TimeScopes.Any())
            {
                gan.AllocationStamps = AllocationStampMapper.ExportAllocationStamps(adaptGuidanceAllocation.TimeScopes).ToList();
            }

            //Guidance Shift
            if (adaptGuidanceAllocation.GuidanceShift != null)
            {
                GuidanceShiftMapper gstMapper = new GuidanceShiftMapper(TaskDataMapper);
                gan.GuidanceShifts = new List<ISOGuidanceShift>() { gstMapper.ExportGuidanceShift(adaptGuidanceAllocation.GuidanceShift) }; 
            }

            return gan;
        }

        #endregion Export 

        #region Import

        public IEnumerable<GuidanceAllocation> ImportGuidanceAllocations(IEnumerable<ISOGuidanceAllocation> isoGuidanceAllocations)
        {
            //Import groups
            List<GuidanceAllocation> adaptGuidanceAllocations = new List<GuidanceAllocation>();
            foreach (ISOGuidanceAllocation isoGuidanceAllocation in isoGuidanceAllocations)
            {
                GuidanceAllocation adaptGuidanceAllocation = ImportGuidanceAllocation(isoGuidanceAllocation);
                adaptGuidanceAllocations.Add(adaptGuidanceAllocation);
            }

            return adaptGuidanceAllocations;
        }

        public GuidanceAllocation ImportGuidanceAllocation(ISOGuidanceAllocation isoGuidanceAllocation)
        {
            GuidanceAllocation adaptGuidanceAllocation = new GuidanceAllocation();

            //Group ID
            if (TaskDataMapper.ADAPTIdMap.ContainsKey(isoGuidanceAllocation.GuidanceGroupIdRef))
            {
                 adaptGuidanceAllocation.GuidanceGroupId = TaskDataMapper.ADAPTIdMap[isoGuidanceAllocation.GuidanceGroupIdRef].Value;
            }

            //Allocation Stamps
            if (isoGuidanceAllocation.AllocationStamps.Any())
            {
                adaptGuidanceAllocation.TimeScopes = AllocationStampMapper.ImportAllocationStamps(isoGuidanceAllocation.AllocationStamps).ToList();
            }

            //Guidance Shift
            if (isoGuidanceAllocation.GuidanceShifts.Any())
            {
                GuidanceShiftMapper gstMapper = new GuidanceShiftMapper(TaskDataMapper);
                adaptGuidanceAllocation.GuidanceShift = gstMapper.ImportGuidanceShift(isoGuidanceAllocation.GuidanceShifts.First()); //Using first item only
            }

            return adaptGuidanceAllocation;
        }
        #endregion Import
    }
}
