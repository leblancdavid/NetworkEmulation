using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    public enum FileTransferCommandCode
    {
        FileRequest=1,
        FileData=2,
        ReTransmition=3,
        Completed=4,
        Unknown = 0,
    }
}
