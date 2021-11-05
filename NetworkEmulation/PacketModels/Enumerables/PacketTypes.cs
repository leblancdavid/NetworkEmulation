using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    public enum PacketTypes
    {
        Basic=1,
        RoutingProbe=2,
        FileTransfer=3,
        Unknown=0
    }
}
