using System;
using System.Collections.Generic;
using System.Linq;
using AgGateway.ADAPT.ApplicationDataModel.Prescriptions;
using AgGateway.ADAPT.ISOv4Plugin.Models;

namespace AgGateway.ADAPT.ISOv4Plugin.ExportMappers
{
    public class PrescriptionMapper
    {
        public List<TZN> Map(List<Prescription> prescriptions)
        {
            if(prescriptions == null || !prescriptions.Any())
                return new List<TZN>();
            return prescriptions.SelectMany(Map).ToList();
        }

        private List<TZN> Map(Prescription prescription)
        {
            throw new NotImplementedException();
        } 
    }
}
