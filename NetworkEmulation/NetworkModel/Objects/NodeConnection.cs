using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Threading;

namespace NetworkEmulation.NetworkModel
{
    public class NodeConnection : INodeConnection
    {
        #region Members
        private UdpStateInfo mInfo;
        private UdpClient mSocket;
        private Random mRandomSeed = new Random();
        private int mDropProbability;
        private int mErrorProbability;
        private double MAX_RAND_LIMIT = 1000000;
        #endregion

        #region Properties

        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public string HostName { get; set; }
        public IPAddress Address { get; set; }
        public IPEndPoint LocalEndPoint { get; set; }
        public AddressFamily AddressType { get; set; }
        public double DropProbability 
        {
            get { return (double)mDropProbability / MAX_RAND_LIMIT; }
            set { mDropProbability = (int)(value * MAX_RAND_LIMIT); } 
        }
        public double ErrorProbability 
        {
            get { return (double)mErrorProbability / MAX_RAND_LIMIT; }
            set { mErrorProbability = (int)(value * MAX_RAND_LIMIT); } 
        }

        public bool IsOpen { get; private set; }

        public event EventHandler<OnPacketReceivedArgs> OnPacketReceived;
        public event EventHandler<OnPacketReceivedArgs> OnPacketDropped;
        public event EventHandler<OnPacketReceivedArgs> OnPacketError;
        #endregion

        #region Constructor

        public NodeConnection(IPAddress pNodeAddress,
                        string pHostName,
                        int pLocalPort,
                        int pRemotePort, 
                        IPEndPoint pLocalEndPoint,
                        AddressFamily pAddressType,
                        double pDropProbability,
                        double pErrorProbability)
        {
            try
            {
                Address = pNodeAddress;
                HostName = pHostName;
                LocalPort = pLocalPort;
                RemotePort = pRemotePort;
                LocalEndPoint = pLocalEndPoint;
                DropProbability = pDropProbability;
                ErrorProbability = pErrorProbability;

                //Instanciate the UDP socket locally
                mSocket = new UdpClient(LocalPort, pAddressType);

                //Setup the info
                mInfo = new UdpStateInfo(Address, LocalEndPoint, mSocket);

                //Connect to the remote port
                mSocket.Connect(HostName, RemotePort);

                //Don't open until user calls BeginListen
                IsOpen = false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Public Methods
        public new string ToString()
        {
            var json = new JavaScriptSerializer().Serialize(this);
            return json;
        }

        public static INodeConnection Parse(string strParse)
        {
            return new JavaScriptSerializer().Deserialize<NodeConnection>(strParse);
        }


        public void BeginListen()
        {
            try
            {
                IsOpen = true;
                mSocket.BeginReceive(new AsyncCallback(ReceiveCallback), mInfo);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void StopListen()
        {
            IsOpen = false;
        }

        public void Send(IPacket packet)
        {
            try
            {
                mSocket.Send(packet.PacketBytes, packet.PacketBytes.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Private Methods
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                UdpStateInfo state = (UdpStateInfo)ar.AsyncState;

                UdpClient udp = (UdpClient)((UdpStateInfo)ar.AsyncState).Socket;
                IPEndPoint ep = (IPEndPoint)((UdpStateInfo)ar.AsyncState).EndPoint;

                Byte[] receiveBytes = udp.EndReceive(ar, ref ep);

                if (IsOpen)
                {
                    //If it's Open, then let's get the data where it needs to go
                    //Assuming the packet was not dropped based on the drop probability
                    //(a random simulated event).
                    bool drop = DropPacket();
                    bool error = ErrorPacket();
                    OnPacketReceivedArgs args = new OnPacketReceivedArgs(PacketBuilder.GetPacketFromBytes(receiveBytes), Address, LocalPort, DateTime.UtcNow);
                    if ((!drop || !error))
                    {
                        InvokePacketReceived(args);
                    }
                    if (drop)
                    {
                        InvokePacketDropped(args);
                    }
                    if (error)
                    {
                        InvokePacketError(args);
                    }

                    //Continue to listen
                    BeginListen();
                }
                else
                {
                    //Otherwise ignore it?
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
                throw ex;
            }
        }

        private bool DropPacket()
        {
            int value = mRandomSeed.Next(0, (int)MAX_RAND_LIMIT);
            if (value < mDropProbability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ErrorPacket()
        {
            int value = mRandomSeed.Next(0, (int)MAX_RAND_LIMIT);
            if (value < mErrorProbability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InvokePacketReceived(OnPacketReceivedArgs args)
        {
            if (OnPacketReceived != null)
            {
                OnPacketReceived.Invoke(this, args);
            }
        }

        private void InvokePacketDropped(OnPacketReceivedArgs args)
        {
            if (OnPacketDropped != null)
            {
                OnPacketDropped.Invoke(this, args);
            }
        }

        private void InvokePacketError(OnPacketReceivedArgs args)
        {
            if (OnPacketError != null)
            {
                OnPacketError.Invoke(this, args);
            }
        }
        #endregion
    }
}
