using AgGateway.ADAPT.ApplicationDataModel.ADM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class Error : IError
    {
        public string Id { get; set; }

        public string Source { get; set; }

        public string Description { get; set; }

        public string StackTrace { get; set; }
    }
}
