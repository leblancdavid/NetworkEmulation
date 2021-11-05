using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class OnPacketReceivedArgs : EventArgs
    {
        #region Properties
        public IPacket Packet { get; set; }
        public IPAddress CurrentAddress { get; set; }
        public int Port { get; set; }
        public DateTime Time { get; set; }
        #endregion

        #region Constructors
        public OnPacketReceivedArgs(IPacket pPacket, IPAddress pAddress, int pPort, DateTime pTime)
        {
            Packet = pPacket;
            CurrentAddress = pAddress;
            Port = pPort;
            Time = pTime;
        }
        #endregion
    }
}
