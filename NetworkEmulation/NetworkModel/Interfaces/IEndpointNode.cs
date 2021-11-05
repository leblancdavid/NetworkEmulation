using NetworkEmulation.PacketModels;
using NetworkEmulation.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public interface IEndpointNode : INetworkNode
    {
        List<FileTransferProtocol> FileTransferTasks { get; set; }

        void RequestFileTransfer(IPAddress destination, string FileName, FileStream fs);
    }
}
