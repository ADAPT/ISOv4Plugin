using System;
using System.Xml.Linq;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class HeaderProperty
    {
        public HeaderProperty()
        {
            
        }

        public HeaderProperty(XAttribute attribute)
        {
            State = GetState(attribute);
            Value = attribute != null ? attribute.Value : null;
        }

        public HeaderPropertyState State { get; set; }

        public object Value { get; set;  }

        private HeaderPropertyState GetState(XAttribute attribute)
        {
            if (attribute == null)
                return HeaderPropertyState.IsNull;

            if (String.IsNullOrEmpty(attribute.Value))
                return HeaderPropertyState.IsEmpty;

            return HeaderPropertyState.HasValue;
        }
    }
}