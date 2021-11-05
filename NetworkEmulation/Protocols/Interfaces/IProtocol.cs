using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.Protocols
{
    public interface IProtocol
    {
        Guid TaskId { get; set; }
    }
}
