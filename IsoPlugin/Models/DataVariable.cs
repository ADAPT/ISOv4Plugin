using AgGateway.ADAPT.ApplicationDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.Plugins
{
    internal class DataVariable
    {
        public NumericRepresentationValue Value { get; set; }
        public string ProductId { get; set; }
    }
}
