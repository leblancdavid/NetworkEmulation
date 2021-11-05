using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class CoreNode : ICoreNode
    {
        #region Properties
        public RoutingTable RoutingTable { get; private set; }
        public IPAddress NodeAddress { get; set; }
        public Dictionary<int, INodeConnection> ConnectionPoints { get; private set; }

        public double Delay { get; set; }

        public event EventHandler<OnPacketReceivedArgs> OnPacketReceived;
        public event EventHandler<OnRoutingProbeReceivedArgs> OnRoutingProbeReceived;
        #endregion

        #region Constructors
        public CoreNode()
        {
            RoutingTable = new RoutingTable();
            NodeAddress = new IPAddress(0);
            ConnectionPoints = new Dictionary<int, INodeConnection>();
            Delay = 0;
        }

        public CoreNode(IPAddress pAddress, double delay)
        {
            RoutingTable = new RoutingTable();
            NodeAddress = pAddress;
            ConnectionPoints = new Dictionary<int, INodeConnection>();
            Delay = delay;
        }

        #endregion

        #region Public Methods

        public void AddConnectionPoint(INodeConnection pConnection)
        {
            if (ConnectionPoints.ContainsKey(pConnection.LocalPort))
            {
                throw new Exception("Exception Duplicate port number on node '" + NodeAddress.ToString() + "'.");
            }
            else
            {
                ConnectionPoints.Add(pConnection.LocalPort, pConnection);
                ConnectionPoints[pConnection.LocalPort].OnPacketReceived += NetworkNode_OnPacketReceived;
            }
        }

        public void RemoveConnectionPoint(int pPort)
        {
            if (ConnectionPoints.ContainsKey(pPort))
            {
                ConnectionPoints[pPort].OnPacketReceived -= NetworkNode_OnPacketReceived;
                ConnectionPoints.Remove(pPort);
            }
            else
            {
                throw new Exception("There is no connection on port '" + pPort.ToString() + "' for Node '" + NodeAddress.ToString() + "'.");
            }
        }

        public void Listen()
        {
            foreach(INodeConnection conn in ConnectionPoints.Values)
            {
                conn.BeginListen();
            }
        }

        public void StopListen()
        {
            foreach (INodeConnection conn in ConnectionPoints.Values)
            {
                conn.StopListen();
            }
        }

        public void Send(IPacket packet)
        {
            if (packet.Type == PacketModels.PacketTypes.RoutingProbe)
            {
                ProcessRoutingProbe((RoutingProbePacket)packet, 0);
            }
        }

        #endregion

        #region Private Methods

        private void NetworkNode_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            Thread.Sleep((int)Delay);
            if (e.Packet.Type == PacketModels.PacketTypes.RoutingProbe)
            {
                ProcessRoutingProbe((RoutingProbePacket)e.Packet, e.Port);
            }
            else
            {
                //If the path to destination is posible
                if (RoutingTable.ContainsKey(e.Packet.DestinationAddress))
                {
                    //Forward the packet.
                    ConnectionPoints[RoutingTable[e.Packet.DestinationAddress].ForwardPort].Send(e.Packet);
                }
                else
                {
                    //Drop the packet?
                }
                InvokeOnPacketReceived(e);

            }
        }

        private void ProcessRoutingProbe(RoutingProbePacket packet, int port)
        {
            //Want to update the routing table, and forward it to some other random port
            List<int> connectionPorts = new List<int>();
            Random rand = new Random();
            List<int> ports = Enumerable.ToList(ConnectionPoints.Keys);
            int next = ports[rand.Next(ConnectionPoints.Count)];

            List<IPAddress> history = packet.GetHistory();
            UpdateRoutingTable(history, port);

            IPAddress previousAddr = new IPAddress(0);
            if (history.Count > 0)
                previousAddr = history[0];
            IPAddress currentAddr = NodeAddress;
            IPAddress nextAddr = ConnectionPoints[next].LocalEndPoint.Address;

            history.Insert(0, new IPAddress(NodeAddress.GetAddressBytes())); //Insert the current Address at the beginning of the list
            packet.SetHistory(history);

            OnRoutingProbeReceivedArgs args = new OnRoutingProbeReceivedArgs(previousAddr, currentAddr, nextAddr, packet.PacketId, DateTime.UtcNow);
            NotifyOfRoutingPacketReceived(args);

            //Thread.Sleep(100);

            //Forward the Routing Probe to the next node
            //int ttl = (int)packet.TimeToLive + 1;
            //if(ttl < 500) //if it's not too many hops.
            //{
            //    packet.TimeToLive = (short)ttl;
            //    ConnectionPoints[next].Send(packet);
            //}

            ConnectionPoints[next].Send(packet);
        }

        private void UpdateRoutingTable(List<IPAddress> history, int port)
        {
            int cost = 0;
            foreach (IPAddress address in history)
            {
                if (address != this.NodeAddress)
                {
                    RoutingTable.Update(address, new RoutingInfo(port, cost));
                    cost++;
                }
            }
        }

        private void NotifyOfRoutingPacketReceived(OnRoutingProbeReceivedArgs args)
        {
            if (OnRoutingProbeReceived != null)
            {
                OnRoutingProbeReceived.Invoke(this, args);
            }
        }

        private void InvokeOnPacketReceived(OnPacketReceivedArgs args)
        {
            if (OnPacketReceived != null)
            {
                OnPacketReceived(this, args);
            }
        }

        #endregion
    }
}
