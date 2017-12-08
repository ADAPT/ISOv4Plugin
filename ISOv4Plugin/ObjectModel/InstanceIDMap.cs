using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgGateway.ADAPT.ISOv4Plugin.ObjectModel
{
    public class InstanceIDMap
    {
        private Dictionary<string, int> _adaptIDs;
        private Dictionary<int, string> _isoIDs;

		public InstanceIDMap()
        {
            _adaptIDs = new Dictionary<string, int>();
            _isoIDs = new Dictionary<int, string>();
        }

        public bool Add(int adaptID, string isoID)
        {
            if (!_adaptIDs.ContainsKey(isoID) && !_isoIDs.ContainsKey(adaptID))
            {
                _adaptIDs.Add(isoID, adaptID);
                _isoIDs.Add(adaptID, isoID);
                return true;
            }
            return false;
        }

        public void Replace(int oldAdaptID, string oldISOID, int newAdaptID, string newISOID)
        {
            if (_adaptIDs.ContainsKey(oldISOID) && _isoIDs.ContainsKey(oldAdaptID))
            {
                _adaptIDs[oldISOID] = newAdaptID;
                _isoIDs[oldAdaptID] = newISOID;
            }
        }

        public int? GetADAPTID(string isoID)
        {
            if (isoID != null && _adaptIDs.ContainsKey(isoID))
            {
                return _adaptIDs[isoID];
            }
            return null;                
        }

        public string GetISOID(int adaptID)
        {
            if (_isoIDs.ContainsKey(adaptID))
            {
                return _isoIDs[adaptID];
            }
            return null;
        }
    }
}
