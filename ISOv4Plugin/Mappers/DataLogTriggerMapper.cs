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

        public const int DefaultSet = 57343; //DFFF

        public DataLogTriggerMapper(TaskDataMapper taskDataMapper) : base(taskDataMapper, "DLT")
        {
        }

        #region Export
        public IEnumerable<ISODataLogTrigger> ExportDataLogTriggers(IEnumerable<DataLogTrigger> adaptDataLogTriggers)
        {
            List<ISODataLogTrigger> dataLogTriggers = new List<ISODataLogTrigger>();
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

            int? ddi = null;
            if (adaptDataLogTrigger.RequestDefaultProcessData)
            {
                ddi = DefaultSet; //DFFF
            }
            else if (adaptDataLogTrigger.Representation != null)
            {
                ddi = RepresentationMapper.Map(adaptDataLogTrigger.Representation);
            }
             
            if (ddi.HasValue) //Need DDI for a valid DLT
            {
                isoDataLogTrigger.DataLogDDI = ddi.Value.AsHexDDI();
                var loggingMethods = adaptDataLogTrigger.DataLogMethods.Any() ? adaptDataLogTrigger.DataLogMethods : new List<LoggingMethodEnum> { adaptDataLogTrigger.DataLogMethod };
                isoDataLogTrigger.DataLogMethod = ExportDataLogMethods(loggingMethods);
                isoDataLogTrigger.DataLogDistanceInterval = adaptDataLogTrigger.DataLogDistanceInterval?.AsIntViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogTimeInterval = adaptDataLogTrigger.DataLogTimeInterval?.AsIntViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogThresholdMinimum = adaptDataLogTrigger.DataLogThresholdMinimum?.AsIntViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogThresholdMaximum = adaptDataLogTrigger.DataLogThresholdMaximum?.AsIntViaMappedDDI(RepresentationMapper);
                isoDataLogTrigger.DataLogThresholdChange = adaptDataLogTrigger.DataLogThresholdChange?.AsIntViaMappedDDI(RepresentationMapper);
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

        private byte ExportDataLogMethods(List<LoggingMethodEnum> methods)
        {
            byte output = 0;
            foreach (LoggingMethodEnum loggingMethod in methods)
            {
                switch (loggingMethod)
                {
                    case LoggingMethodEnum.TimeInterval:
                        output += 1;
                        break;
                    case LoggingMethodEnum.DistanceInterval:
                        output += 2;
                        break;
                    case LoggingMethodEnum.ThresholdLimits:
                        output += 4;
                        break;
                    case LoggingMethodEnum.OnChange:
                        output += 8;
                        break;
                    case LoggingMethodEnum.Total:
                        output += 16;
                        break;
                }
            }
            return output;
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
            ApplicationDataModel.Representations.Representation representation = null;
            if (ddi != DefaultSet)
            {
                representation = RepresentationMapper.Map(ddi);
            }
            if (ddi == DefaultSet || representation != null) //Check that we can map to something meaningful before creating the DLT
            {
                DataLogTrigger adaptTrigger = new DataLogTrigger();
                
                if (ddi == DefaultSet)
                {
                    adaptTrigger.RequestDefaultProcessData = true;
                }
                else
                {
                    adaptTrigger.Representation = representation;
                }
                adaptTrigger.DataLogMethods = ImportDataLogMethods(isoDataLogTrigger.DataLogMethod);

                //Obsolete behavior
                adaptTrigger.DataLogMethod = adaptTrigger.DataLogMethods.Any() ? adaptTrigger.DataLogMethods.First() : LoggingMethodEnum.Total;  

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

        private List<LoggingMethodEnum> ImportDataLogMethods(byte dataLogMethod)
        {
            List<LoggingMethodEnum> dataLogMethods = new List<LoggingMethodEnum>();
            if ((dataLogMethod & 1) != 0)
            {
                dataLogMethods.Add(LoggingMethodEnum.TimeInterval);
            }
            if ((dataLogMethod & 2) != 0)
            {
                dataLogMethods.Add(LoggingMethodEnum.DistanceInterval);
            }
            if ((dataLogMethod & 4) != 0)
            {
                dataLogMethods.Add(LoggingMethodEnum.ThresholdLimits);
            }
            if ((dataLogMethod & 8) != 0)
            {
                dataLogMethods.Add(LoggingMethodEnum.OnChange);
            }
            if ((dataLogMethod & 16) != 0)
            {
                dataLogMethods.Add(LoggingMethodEnum.Total);
            }
            return dataLogMethods;
        }
        #endregion Import
    }
}
