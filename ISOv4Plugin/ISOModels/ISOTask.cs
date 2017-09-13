/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOTask : ISOElement
    {
        public ISOTask()
        {
            TreatmentZones = new List<ISOTreatmentZone>();
            Times = new List<ISOTime>();
            WorkerAllocations = new List<ISOWorkerAllocation>();
            DeviceAllocations = new List<ISODeviceAllocation>();
            Connections = new List<ISOConnection>();
            ProductAllocations = new List<ISOProductAllocation>();
            DataLogTriggers = new List<ISODataLogTrigger>();
            CommentAllocations = new List<ISOCommentAllocation>();
            TimeLogs = new List<ISOTimeLog>();
            GuidanceAllocations = new List<ISOGuidanceAllocation>();
        }

        //Attributes
        public string TaskID { get; set; }
        public string TaskDesignator { get; set; }
        public string CustomerIdRef { get; set; }
        public string FarmIdRef { get; set; }
        public string PartFieldIdRef { get; set; }
        public string ResponsibleWorkerIdRef { get; set; }
        public ISOTaskStatus TaskStatus { get; set; }
        public int? DefaultTreatmentZoneCode { get; set; }
        public int? PositionLostTreatmentZoneCode { get; set; }
        public int? OutOfFieldTreatmentZoneCode { get; set; }

        //Child Elements
        public ISOGrid Grid { get; set; }
        public List<ISOTreatmentZone> TreatmentZones { get; set; }
        public List<ISOTime> Times { get; set; }
        public List<ISOWorkerAllocation> WorkerAllocations { get; set; }
        public List<ISODeviceAllocation> DeviceAllocations { get; set; }
        public List<ISOConnection> Connections { get; set; }
        public List<ISOProductAllocation> ProductAllocations { get; set; }
        public List<ISODataLogTrigger> DataLogTriggers { get; set; }
        public List<ISOCommentAllocation> CommentAllocations { get; set; }
        public List<ISOTimeLog> TimeLogs { get; set; }
        public List<ISOGuidanceAllocation> GuidanceAllocations { get; set; }

        public bool IsLoggedDataTask { get { return TaskStatus == ISOTaskStatus.Completed || TaskStatus == ISOTaskStatus.Paused || TaskStatus == ISOTaskStatus.Running; } }
        public bool IsWorkItemTask { get { return !IsLoggedDataTask; } }
        public bool HasPrescription { get { return HasRasterPrescription || HasVectorPrescription || HasManualPrescription; } }
        public bool HasRasterPrescription { get { return Grid != null; } }
        public bool HasVectorPrescription { get { return TreatmentZones.Any(tz => tz.Polygons.Any()) ; } }
        public bool HasManualPrescription { get { return !HasRasterPrescription && !HasVectorPrescription && TreatmentZones.Any(tz => tz.ProcessDataVariables.Any()); } }
        public ISOTreatmentZone DefaultTreatmentZone { get { return GetTreatmentZone(DefaultTreatmentZoneCode); } }
        public ISOTreatmentZone PositionLostTreatmentZone { get { return GetTreatmentZone(PositionLostTreatmentZoneCode); } }
        public ISOTreatmentZone OutOfFieldTreatmentZone { get { return GetTreatmentZone(OutOfFieldTreatmentZoneCode); } }


        public ISOTreatmentZone GetTreatmentZone(int? treatmentZoneCode)
        {
            if (treatmentZoneCode.HasValue && TreatmentZones.Count > 0)
            {
                return TreatmentZones.FirstOrDefault(tz => tz.TreatmentZoneCode == treatmentZoneCode);
            }
            return null;
        }



        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TSK");
            xmlBuilder.WriteXmlAttribute("A", TaskID);
            xmlBuilder.WriteXmlAttribute("B", TaskDesignator);
            xmlBuilder.WriteXmlAttribute("C", CustomerIdRef);
            xmlBuilder.WriteXmlAttribute("D", FarmIdRef);
            xmlBuilder.WriteXmlAttribute("E", PartFieldIdRef);
            xmlBuilder.WriteXmlAttribute("F", ResponsibleWorkerIdRef);
            xmlBuilder.WriteXmlAttribute("G", ((int)TaskStatus).ToString());
            xmlBuilder.WriteXmlAttribute<long>("H", DefaultTreatmentZoneCode);
            xmlBuilder.WriteXmlAttribute<long>("I", PositionLostTreatmentZoneCode);
            xmlBuilder.WriteXmlAttribute<long>("J", OutOfFieldTreatmentZoneCode);

            if (Grid != null)
            {
                Grid.WriteXML(xmlBuilder);
            }

            foreach (ISOTreatmentZone item in TreatmentZones) { item.WriteXML(xmlBuilder); }
            foreach (ISOTime item in Times) { item.WriteXML(xmlBuilder); }
            foreach (ISOWorkerAllocation item in WorkerAllocations) { item.WriteXML(xmlBuilder); }
            foreach (ISODeviceAllocation item in DeviceAllocations) { item.WriteXML(xmlBuilder); }
            foreach (ISOConnection item in Connections) { item.WriteXML(xmlBuilder); }
            foreach (ISOProductAllocation item in ProductAllocations) { item.WriteXML(xmlBuilder); }
            foreach (ISODataLogTrigger item in DataLogTriggers) { item.WriteXML(xmlBuilder); }
            foreach (ISOCommentAllocation item in CommentAllocations) { item.WriteXML(xmlBuilder); }
            foreach (ISOTimeLog item in TimeLogs) { item.WriteXML(xmlBuilder); }
            foreach (ISOGuidanceAllocation item in GuidanceAllocations) { item.WriteXML(xmlBuilder); }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOTask ReadXML(XmlNode taskNode)
        {
            ISOTask task = new ISOTask();
            task.TaskID = taskNode.GetXmlNodeValue("@A");
            task.TaskDesignator = taskNode.GetXmlNodeValue("@B");
            task.CustomerIdRef = taskNode.GetXmlNodeValue("@C");
            task.FarmIdRef = taskNode.GetXmlNodeValue("@D");
            task.PartFieldIdRef = taskNode.GetXmlNodeValue("@E");
            task.ResponsibleWorkerIdRef = taskNode.GetXmlNodeValue("@F");
            task.TaskStatus = (ISOTaskStatus)(Int32.Parse(taskNode.GetXmlNodeValue("@G"))); 
            task.DefaultTreatmentZoneCode = taskNode.GetXmlNodeValueAsNullableInt("@H");
            task.PositionLostTreatmentZoneCode = taskNode.GetXmlNodeValueAsNullableInt("@I");
            task.OutOfFieldTreatmentZoneCode = taskNode.GetXmlNodeValueAsNullableInt("@J");

            //Treatment Zones
            XmlNodeList tznNodes = taskNode.SelectNodes("TZN");
            if (tznNodes != null)
            {
                task.TreatmentZones.AddRange(ISOTreatmentZone.ReadXML(tznNodes));
            }

            //Times
            XmlNodeList timNodes = taskNode.SelectNodes("TIM");
            if (timNodes != null)
            {
                task.Times.AddRange(ISOTime.ReadXML(timNodes));
            }

            //Worker Allocations
            XmlNodeList wanNodes = taskNode.SelectNodes("WAN");
            if (wanNodes != null)
            {
                task.WorkerAllocations.AddRange(ISOWorkerAllocation.ReadXML(wanNodes));
            }

            //Device Allocations
            XmlNodeList danNodes = taskNode.SelectNodes("DAN");
            if (danNodes != null)
            {
                task.DeviceAllocations.AddRange(ISODeviceAllocation.ReadXML(danNodes));
            }

            //Connections
            XmlNodeList cnnNodes = taskNode.SelectNodes("CNN");
            if (cnnNodes != null)
            {
                task.Connections.AddRange(ISOConnection.ReadXML(cnnNodes));
            }

            //Product Allocations
            XmlNodeList panNodes = taskNode.SelectNodes("PAN");
            if (panNodes != null)
            {
                task.ProductAllocations.AddRange(ISOProductAllocation.ReadXML(panNodes));
            }

            //Data Log Triggers
            XmlNodeList dltNodes = taskNode.SelectNodes("DLT");
            if (dltNodes != null)
            {
                task.DataLogTriggers.AddRange(ISODataLogTrigger.ReadXML(dltNodes));
            }

            //Comment Allocations
            XmlNodeList canNodes = taskNode.SelectNodes("CAN");
            if (canNodes != null)
            {
                task.CommentAllocations.AddRange(ISOCommentAllocation.ReadXML(canNodes));
            }

            //Grid
            XmlNode grdNode = taskNode.SelectSingleNode("GRD");
            if (grdNode != null)
            {
                task.Grid = ISOGrid.ReadXML(grdNode);
            }

            //TimeLogs
            XmlNodeList tlgNodes = taskNode.SelectNodes("TLG");
            if (tlgNodes != null)
            {
                task.TimeLogs.AddRange(ISOTimeLog.ReadXML(tlgNodes));
            }

            //Guidance Allocations
            XmlNodeList ganNodes = taskNode.SelectNodes("GAN");
            if (ganNodes != null)
            {
                task.GuidanceAllocations.AddRange(ISOGuidanceAllocation.ReadXML(ganNodes));
            }

            return task;
        }

        public static List<ISOTask> ReadXML(XmlNodeList taskNodes)
        {
            List<ISOTask> tasks = new List<ISOTask>();
            foreach (XmlNode taskNode in taskNodes)
            {
                tasks.Add(ISOTask.ReadXML(taskNode));
            }
            return tasks;
        }
    }
}