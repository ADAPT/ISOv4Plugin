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
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.Representations;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IGuidanceShiftMapper
    {
        IEnumerable<ISOGuidanceShift> ExportGuidanceShifts(IEnumerable<GuidanceShift> adaptGuidanceShifts);
        ISOGuidanceShift ExportGuidanceShift(GuidanceShift adaptGuidanceShift);
        IEnumerable<GuidanceShift> ImportGuidanceShifts(IEnumerable<ISOGuidanceShift> isoGuidanceShifts);
        GuidanceShift ImportGuidanceShift(ISOGuidanceShift isoGuidanceShift);
    }

    public class GuidanceShiftMapper : BaseMapper, IGuidanceShiftMapper
    {
        public GuidanceShiftMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "GST")
        {
        }

        #region Export
        public IEnumerable<ISOGuidanceShift> ExportGuidanceShifts(IEnumerable<GuidanceShift> adaptGuidanceShifts)
        {
            List <ISOGuidanceShift> isoShifts = new List<ISOGuidanceShift>();
            foreach (GuidanceShift shift in adaptGuidanceShifts)
            {
                ISOGuidanceShift isoShift = ExportGuidanceShift(shift);
                isoShifts.Add(isoShift);
            }
            return isoShifts;
        }

        public ISOGuidanceShift ExportGuidanceShift(GuidanceShift adaptGuidanceShift)
        {
            ISOGuidanceShift gst = new ISOGuidanceShift();

            //Group ID
            gst.GuidanceGroupIdRef = TaskDataMapper.InstanceIDMap.GetISOID(adaptGuidanceShift.GuidanceGroupId);

            //Pattern ID
            gst.GuidancePatternIdRef = TaskDataMapper.InstanceIDMap.GetISOID(adaptGuidanceShift.GuidancePatterId);

            //Shifts
            gst.GuidanceEastShift = adaptGuidanceShift.EastShift.AsConvertedLong("mm");
            gst.GuidanceNorthShift = adaptGuidanceShift.NorthShift.AsConvertedLong("mm");
            gst.PropagationOffset = adaptGuidanceShift.PropagationOffset.AsConvertedLong("mm");

            //Allocation Stamp
            if (adaptGuidanceShift.TimeScopeIds.Any())
            {
                
                TimeScope scope = DataModel.Catalog.TimeScopes.FirstOrDefault(t => t.Id.ReferenceId == adaptGuidanceShift.TimeScopeIds.First()); //Mapping first timescope only
                if (scope != null)
                {
                    gst.AllocationStamp = AllocationStampMapper.ExportAllocationStamp(scope);
                }
            }

            return gst;
        }

        #endregion Export 

        #region Import

        public IEnumerable<GuidanceShift> ImportGuidanceShifts(IEnumerable<ISOGuidanceShift> isoGuidanceShifts)
        {
            //Import groups
            List<GuidanceShift> adaptGuidanceShifts = new List<GuidanceShift>();
            foreach (ISOGuidanceShift isoGuidanceShift in isoGuidanceShifts)
            {
                GuidanceShift adaptGuidanceShift = ImportGuidanceShift(isoGuidanceShift);
                adaptGuidanceShifts.Add(adaptGuidanceShift);
            }

            return adaptGuidanceShifts;
        }

        public GuidanceShift ImportGuidanceShift(ISOGuidanceShift isoGuidanceShift)
        {
            GuidanceShift adaptShift = new GuidanceShift();

            //Group ID
            int? groupID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoGuidanceShift.GuidanceGroupIdRef);
            if (groupID.HasValue)
            {
                adaptShift.GuidanceGroupId = groupID.Value;
            }

            //Pattern ID
            int? patternID = TaskDataMapper.InstanceIDMap.GetADAPTID(isoGuidanceShift.GuidancePatternIdRef);
            if (patternID.HasValue)
            {
                adaptShift.GuidancePatterId = patternID.Value;
            }

            //Shifts
            if (isoGuidanceShift.GuidanceEastShift.HasValue)
            {
                adaptShift.EastShift = isoGuidanceShift.GuidanceEastShift.Value.AsNumericRepresentationValue("mm");
            }

            if (isoGuidanceShift.GuidanceNorthShift.HasValue)
            {
                adaptShift.NorthShift = isoGuidanceShift.GuidanceNorthShift.Value.AsNumericRepresentationValue("mm");
            }

            if (isoGuidanceShift.PropagationOffset.HasValue)
            {
                adaptShift.PropagationOffset = isoGuidanceShift.PropagationOffset.Value.AsNumericRepresentationValue("mm");
            }

            //Allocation Stamp
            if (isoGuidanceShift.AllocationStamp != null)
            {
                TimeScope scope = AllocationStampMapper.ImportAllocationStamp(isoGuidanceShift.AllocationStamp);
                DataModel.Catalog.TimeScopes.Add(scope);
                adaptShift.TimeScopeIds = new List<int>() { scope.Id.ReferenceId };
            }

            return adaptShift;
        }
        #endregion Import
    }
}
