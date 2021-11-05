using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class RoutingTable : Dictionary<IPAddress, RoutingInfo>, IRoutingTable 
    {
        #region Properties
        public event EventHandler<OnRoutingTableUpdatedArgs> OnRoutingTableUpdated;
        #endregion

        #region Constructors
        public RoutingTable()
        {

        }
        #endregion

        #region Public Methods
        public void Update(IPAddress address, RoutingInfo info)
        {
            RoutingInfo entry = null;
            if (!TryGetValue(address, out entry)) //if no entry exists for that address
            {
                Add(address, info); //Add it to the table
                NotifyOfRoutingTableUpdated(address, info);
            }
            else
            {
                if (entry.Cost > info.Cost) //found a better route
                {
                    this[address] = info;
                    NotifyOfRoutingTableUpdated(address, info);
                }
            }
        }
        #endregion

        #region Private Methods
        private void NotifyOfRoutingTableUpdated(IPAddress address, RoutingInfo info)
        {
            OnRoutingTableUpdatedArgs args = new OnRoutingTableUpdatedArgs(info, address);
            if (OnRoutingTableUpdated != null)
                OnRoutingTableUpdated.Invoke(this, args);
        }
        #endregion
    }
}
