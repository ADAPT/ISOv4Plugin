/*
 * ISO standards can be purchased through the ANSI webstore at https://webstore.ansi.org
*/

using AgGateway.ADAPT.ISOv4Plugin.ExtensionMethods;
using AgGateway.ADAPT.ISOv4Plugin.ISOEnumerations;
using AgGateway.ADAPT.ISOv4Plugin.ObjectModel;
using System;
using System.Collections.Generic;
using System.Xml;

namespace AgGateway.ADAPT.ISOv4Plugin.ISOModels
{
    public class ISOTreatmentZone : ISOElement
    {
        public ISOTreatmentZone()
        {
            ProcessDataVariables = new List<ISOProcessDataVariable>();
            Polygons = new List<ISOPolygon>();
        }

        //Attributes
        public byte TreatmentZoneCode { get; set; }
        public string TreatmentZoneDesignator { get; set; }
        public byte? TreatmentZoneColour { get; set; }

        //Child Elements
        public List<ISOProcessDataVariable> ProcessDataVariables { get; set; }
        public List<ISOPolygon> Polygons { get; set; }

        public override XmlWriter WriteXML(XmlWriter xmlBuilder)
        {
            xmlBuilder.WriteStartElement("TZN");
            xmlBuilder.WriteXmlAttribute<byte>("A", TreatmentZoneCode);
            xmlBuilder.WriteXmlAttribute("B", TreatmentZoneDesignator);
            xmlBuilder.WriteXmlAttribute<byte>("C", TreatmentZoneColour);

            foreach (var item in ProcessDataVariables)
            {
                item.WriteXML(xmlBuilder);
            }

            foreach (var item in Polygons)
            {
                item.WriteXML(xmlBuilder);
            }

            xmlBuilder.WriteEndElement();

            return xmlBuilder;
        }

        public static ISOTreatmentZone ReadXML(XmlNode tznNode)
        {
            ISOTreatmentZone treatmentZone = new ISOTreatmentZone();
            treatmentZone.TreatmentZoneCode = tznNode.GetXmlNodeValueAsByte("@A");
            treatmentZone.TreatmentZoneDesignator = tznNode.GetXmlNodeValue("@B");
            treatmentZone.TreatmentZoneColour = tznNode.GetXmlNodeValueAsNullableByte("@C");

            XmlNodeList pdvNodes = tznNode.SelectNodes("PDV");
            if (pdvNodes != null)
            {
                treatmentZone.ProcessDataVariables.AddRange(ISOProcessDataVariable.ReadXML(pdvNodes));
            }

            XmlNodeList plnNodes = tznNode.SelectNodes("PLN");
            if (plnNodes != null)
            {
                treatmentZone.Polygons.AddRange(ISOPolygon.ReadXML(plnNodes));
            }
            return treatmentZone;
        }

        public static List<ISOTreatmentZone> ReadXML(XmlNodeList nodes)
        {
            List<ISOTreatmentZone> items = new List<ISOTreatmentZone>();
            foreach (XmlNode node in nodes)
            {
                items.Add(ISOTreatmentZone.ReadXML(node));
            }
            return items;
        }

        public override List<Error> Validate(List<Error> errors)
        {
            RequireRange<ISOTreatmentZone, byte>(this, x => x.TreatmentZoneCode, 0, 254, errors, "A");
            ValidateString(this, x => TreatmentZoneDesignator, 32, errors, "B");
            if (TreatmentZoneColour.HasValue) ValidateRange<ISOTreatmentZone, byte>(this, x => x.TreatmentZoneColour.Value, 0, 254, errors, "C");
            Polygons.ForEach(i => i.Validate(errors));
            ProcessDataVariables.ForEach(i => i.Validate(errors));
            return errors;
        }
    }
}