using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public interface ICoreNode : INetworkNode
    {
        RoutingTable RoutingTable { get; }
    }
}
