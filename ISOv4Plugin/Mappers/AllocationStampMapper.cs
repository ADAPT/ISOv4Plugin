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
using AgGateway.ADAPT.ApplicationDataModel.Representations;
using AgGateway.ADAPT.ApplicationDataModel.Common;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{

    public static class AllocationStampMapper 
    {

        #region Export
        public static IEnumerable<ISOAllocationStamp> ExportAllocationStamps(IEnumerable<TimeScope> timeScopes)
        {
            List<ISOAllocationStamp> asps = new List<ISOAllocationStamp>();
            foreach (TimeScope timeScope in timeScopes)
            {
                ISOAllocationStamp isoAllocationStamp = ExportAllocationStamp(timeScope);
                asps.Add(isoAllocationStamp);
            }
            return asps;
        }

        public static ISOAllocationStamp ExportAllocationStamp(TimeScope timeScope)
        {
            ISOAllocationStamp isoAllocationStamp = new ISOAllocationStamp();

            isoAllocationStamp.Start = timeScope.TimeStamp1;
            isoAllocationStamp.Stop = timeScope.TimeStamp2;
            isoAllocationStamp.Positions = new List<ISOPosition>();
            if (timeScope.Location1 != null)
            {
                isoAllocationStamp.Positions.Add(ExportPosition(timeScope.Location1));

                //If no Location1, do not export Location2 
                if (timeScope.Location2 != null)
                {
                    isoAllocationStamp.Positions.Add(ExportPosition(timeScope.Location2));
                }
            }

            return isoAllocationStamp;
        }

        private static ISOPosition ExportPosition(Location location)
        {
            ISOPosition position = new ISOPosition();
            position.PositionEast = (decimal)location.Position.X;
            position.PositionNorth = (decimal)location.Position.Y;
            position.PositionUp = (int)location.Position.Z;
            if (location.GpsSource != null)
            {
                position.NumberOfSatellites = (byte?)location.GpsSource.NumberOfSatellites;
            }
            return position;
        }

        #endregion Export 

        #region Import

        public static  IEnumerable<TimeScope> ImportAllocationStamps(IEnumerable<ISOAllocationStamp> isoAllocationStamps)
        {
            //Import TimeScopes
            List<TimeScope> adaptTimeScopes = new List<TimeScope>();
            foreach (ISOAllocationStamp ISOAllocationStamp in isoAllocationStamps)
            {
                TimeScope adaptTimeScope = ImportAllocationStamp(ISOAllocationStamp);
                adaptTimeScopes.Add(adaptTimeScope);
            }

            return adaptTimeScopes;
        }

        public static TimeScope ImportAllocationStamp(ISOAllocationStamp isoAllocationStamp)
        {
            TimeScope adaptTimeScope = new TimeScope();
            adaptTimeScope.DateContext = DateContextEnum.ActualStart;
            adaptTimeScope.TimeStamp1 = isoAllocationStamp.Start;

            //[Check] AllocationStamp XML element: a recording with only the Start attribute defined is allowed
            if (isoAllocationStamp.Stop != null)
                adaptTimeScope.TimeStamp2 = isoAllocationStamp.Stop;
            //[Check] AllocationStamp XML element: a recording with only the Start attribute defined is allowed
            if (isoAllocationStamp.Duration != null)
            {
                if (isoAllocationStamp.Duration > int.MaxValue)
                    adaptTimeScope.Duration = TimeSpan.FromSeconds((double)isoAllocationStamp.Duration);
                else
                    adaptTimeScope.Duration = new TimeSpan(0, 0, (int) isoAllocationStamp.Duration);
            }
                

            if (isoAllocationStamp.Positions.Count == 1)
            {
                adaptTimeScope.Location1 = ImportPosition(isoAllocationStamp.Positions[0]);
            }
            else if (isoAllocationStamp.Positions.Count == 2)
            {
                adaptTimeScope.Location1 = ImportPosition(isoAllocationStamp.Positions[0]);
                adaptTimeScope.Location2 = ImportPosition(isoAllocationStamp.Positions[1]);
            }
            
            return adaptTimeScope;
        }

        private static Location ImportPosition(ISOPosition position)
        {
            Location location = new Location();
            location.Position = new Point();
            location.Position.X = (double)position.PositionEast;
            location.Position.Y = (double)position.PositionNorth;
            //[Check] if there is a PositionUp
            if(position.PositionUp != null)
                location.Position.Z = (double)position.PositionUp;

            if (position.HasNumberOfSatellites)
            {
                location.GpsSource = new GpsSource();
                location.GpsSource.NumberOfSatellites = (int?)position.NumberOfSatellites;
            }
            return location;
        }
        #endregion Import
    }
}
