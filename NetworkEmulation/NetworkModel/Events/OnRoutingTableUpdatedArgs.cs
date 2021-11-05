using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class OnRoutingTableUpdatedArgs : EventArgs
    {
        #region Properties
        public RoutingInfo Info { get; set; }
        public IPAddress Address { get; set; }
        #endregion

        #region Constructors
        public OnRoutingTableUpdatedArgs(RoutingInfo pInfo, IPAddress pAddress)
        {
            Info = pInfo;
            Address = pAddress;
        }
        #endregion
    }
}
