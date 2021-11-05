using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class RoutingInfo
    {
        public int ForwardPort { get; set; }
        public int Cost { get; set; }

        public RoutingInfo(int pForwardPort, int pCost)
        {
            ForwardPort = pForwardPort;
            Cost = pCost;
        }
    }
}
