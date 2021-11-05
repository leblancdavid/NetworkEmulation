using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public interface IRoutingTable
    {
        event EventHandler<OnRoutingTableUpdatedArgs> OnRoutingTableUpdated;

        void Update(IPAddress address, RoutingInfo info);
        
    }
}
