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
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;

namespace AgGateway.ADAPT.ISOv4Plugin.Mappers
{
    public interface IDataLogTriggerMapper
    {
        IEnumerable<ISODataLogTrigger> ExportDataLogTriggers(IEnumerable<DataLogTrigger> adaptDataLogTriggers);
        ISODataLogTrigger ExportDataLogTrigger(DataLogTrigger adaptDataLogTrigger);

        IEnumerable<DataLogTrigger> ImportDataLogTriggers(IEnumerable<ISODataLogTrigger> isoDataLogTriggers);
        DataLogTrigger ImportDataLogTrigger(ISODataLogTrigger isoDataLogTrigger);
    }

    public class DataLogTriggerMapper : BaseMapper, IDataLogTriggerMapper
    {
        public DataLogTriggerMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "DLT")
        {
        }

        #region Export
        public IEnumerable<ISODataLogTrigger> ExportDataLogTriggers(IEnumerable<DataLogTrigger> adaptDataLogTriggers)
        {
            List <ISODataLogTrigger> dataLogTriggers = new List<ISODataLogTrigger>();
            foreach (DataLogTrigger trigger in adaptDataLogTriggers)
            {
                ISODataLogTrigger isoTrigger = ExportDataLogTrigger(trigger);
                dataLogTriggers.Add(isoTrigger);
            }
            return dataLogTriggers;
        }

        public ISODataLogTrigger ExportDataLogTrigger(DataLogTrigger adaptDataLogTrigger)
        {
            ISODataLogTrigger isoDataLogTrigger = new ISODataLogTrigger();

            int? ddi = RepresentationMapper.Map(adaptDataLogTrigger.Representation);
            if (ddi.HasValue) //Need DDI for a valid DLT
            {
                isoDataLogTrigger.DataLogDDI = ddi.Value.AsHexDDI();
                isoDataLogTrigger.DataLogMethod = ExportDataLogMethod(adaptDataLogTrigger.DataLogMethod);
                isoDataLogTrigger.DataLogDistanceInterval = adaptDataLogTrigger.DataLogDistanceInterval.AsLongViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogTimeInterval = adaptDataLogTrigger.DataLogTimeInterval.AsLongViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogThresholdMinimum = adaptDataLogTrigger.DataLogThresholdMinimum.AsLongViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogThresholdMaximum = adaptDataLogTrigger.DataLogThresholdMaximum.AsLongViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogThresholdChange = adaptDataLogTrigger.DataLogThresholdChange.AsLongViaMappedDDI(RepresentationMapper);
                if (adaptDataLogTrigger.DeviceElementId.HasValue)
                {
                    isoDataLogTrigger.DeviceElementIdRef = TaskDataMapper.InstanceIDMap.GetISOID(adaptDataLogTrigger.DeviceElementId.Value);
                }

                //Not yet implemented
                //isoDataLogTrigger.ValuePresentationIdRef = "";
                //isoDataLogTrigger.DataLogPGN = null;
                //isoDataLogTrigger.DataLogPGNStartBit = null;
                //isoDataLogTrigger.DataLogPGNStopBit = null;
            }

            return isoDataLogTrigger;
        }

        private byte ExportDataLogMethod(LoggingMethodEnum loggingMethod)
        {
            switch (loggingMethod)
            {
                case LoggingMethodEnum.TimeInterval:
                    return (byte)1;
                case LoggingMethodEnum.DistanceInterval:
                    return (byte)2;
                case LoggingMethodEnum.ThresholdLimits:
                    return (byte)4;
                case LoggingMethodEnum.OnChange:
                    return (byte)8;
                case LoggingMethodEnum.Total:
                default:
                    return (byte)16; 
            }
        }

        #endregion Export 

        #region Import

        public IEnumerable<DataLogTrigger> ImportDataLogTriggers(IEnumerable<ISODataLogTrigger> isoDataLogTriggers)
        {
            //Import DLTs
            List<DataLogTrigger> adaptDLTs = new List<DataLogTrigger>();
            foreach (ISODataLogTrigger isoDataLogTrigger in isoDataLogTriggers)
            {
                DataLogTrigger adaptDataLogTrigger = ImportDataLogTrigger(isoDataLogTrigger);
                if (adaptDataLogTrigger != null)
                {
                    adaptDLTs.Add(adaptDataLogTrigger);
                }
            }

            return adaptDLTs;
        }

        public DataLogTrigger ImportDataLogTrigger(ISODataLogTrigger isoDataLogTrigger)
        {
            int ddi = isoDataLogTrigger.DataLogDDI.AsInt32DDI();
            ApplicationDataModel.Representations.Representation representation = RepresentationMapper.Map(ddi);
            if (representation != null)
            {
                DataLogTrigger adaptTrigger = new DataLogTrigger();
                adaptTrigger.Representation = representation;
                adaptTrigger.DataLogMethod = ImportDataLogMethod(isoDataLogTrigger.DataLogMethod.Value);
                if (isoDataLogTrigger.DataLogDistanceInterval.HasValue)
                {
                    adaptTrigger.DataLogDistanceInterval = isoDataLogTrigger.DataLogDistanceInterval.Value.AsNumericRepresentationValue(ddi, RepresentationMapper);
                }
                if (isoDataLogTrigger.DataLogTimeInterval.HasValue)
                {
                    adaptTrigger.DataLogTimeInterval = isoDataLogTrigger.DataLogTimeInterval.Value.AsNumericRepresentationValue(ddi, RepresentationMapper);
                }
                if (isoDataLogTrigger.DataLogThresholdMinimum.HasValue)
                {
                    adaptTrigger.DataLogThresholdMinimum = isoDataLogTrigger.DataLogThresholdMinimum.Value.AsNumericRepresentationValue(ddi, RepresentationMapper);
                }
                if (isoDataLogTrigger.DataLogThresholdMaximum.HasValue)
                {
                    adaptTrigger.DataLogThresholdMaximum = isoDataLogTrigger.DataLogThresholdMaximum.Value.AsNumericRepresentationValue(ddi, RepresentationMapper);
                }
                if (isoDataLogTrigger.DataLogThresholdChange.HasValue)
                {
                    adaptTrigger.DataLogThresholdChange = isoDataLogTrigger.DataLogThresholdChange.Value.AsNumericRepresentationValue(ddi, RepresentationMapper);
                }
                adaptTrigger.DeviceElementId = TaskDataMapper.InstanceIDMap.GetADAPTID(isoDataLogTrigger.DeviceElementIdRef);

                //Not yet implemented
                //adaptTrigger.LoggingLevel = null;

                return adaptTrigger;
            }
            return null;
        }

        private LoggingMethodEnum ImportDataLogMethod(byte dataLogMethod)
        {
            switch (dataLogMethod)
            {
                case 1:
                    return LoggingMethodEnum.TimeInterval;
                case 2:
                    return LoggingMethodEnum.DistanceInterval;
                case 4:
                    return LoggingMethodEnum.ThresholdLimits;
                case 8:
                    return LoggingMethodEnum.OnChange;
                case 16:
                default:
                    return LoggingMethodEnum.Total;
            }
        }
        #endregion Import
    }
}
