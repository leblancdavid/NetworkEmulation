using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public interface INodeConnection
    {
        event EventHandler<OnPacketReceivedArgs> OnPacketReceived;
        event EventHandler<OnPacketReceivedArgs> OnPacketDropped;
        event EventHandler<OnPacketReceivedArgs> OnPacketError;

        int LocalPort { get; set; }
        int RemotePort { get; set; }
        string HostName { get; set; }
        IPAddress Address { get; set; }
        IPEndPoint LocalEndPoint { get; set; }
        AddressFamily AddressType { get; set; }
        bool IsOpen { get; }

        double DropProbability { get; set; }
        double ErrorProbability { get; set; }

        void BeginListen();
        void StopListen();
        void Send(IPacket packet);
    }
}
