using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation
{
    public class UdpStateInfo
    {

        #region Properties

        public IPAddress Address { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public UdpClient Socket { get; set; }

        #endregion

        #region Constructor

        public UdpStateInfo(IPAddress pAddress, IPEndPoint pEndPoint, UdpClient pSocket)
        {
            Address = pAddress;
            EndPoint = pEndPoint;
            Socket = pSocket;
        }

        #endregion


    }
}
