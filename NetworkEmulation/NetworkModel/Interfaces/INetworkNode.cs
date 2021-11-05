using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public interface INetworkNode
    {
        event EventHandler<OnPacketReceivedArgs> OnPacketReceived;
        event EventHandler<OnRoutingProbeReceivedArgs> OnRoutingProbeReceived;

        IPAddress NodeAddress { get; set; }
        Dictionary<int, INodeConnection> ConnectionPoints { get; }

        double Delay { get; set; }

        void AddConnectionPoint(INodeConnection pConnection);
        void RemoveConnectionPoint(int pPort);
        void Listen();
        void StopListen();
        void Send(IPacket packet);
    }
}
