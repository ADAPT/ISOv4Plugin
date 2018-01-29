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

        public void ReplaceADAPTID(string isoID, int newAdaptID)
        {
            if (_adaptIDs.ContainsKey(isoID))
            {
                _adaptIDs[isoID] = newAdaptID;
            }

            if (!_isoIDs.ContainsKey(newAdaptID))
            {
                _isoIDs.Add(newAdaptID, isoID);
            }
        }

            

        public void ReplaceISOID(int adaptID, string newISOID)
        {
            if (_isoIDs.ContainsKey(adaptID))
            {
                _isoIDs[adaptID] = newISOID;
            }

            if (!_adaptIDs.ContainsKey(newISOID))
            {
                _adaptIDs.Add(newISOID, adaptID);
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
