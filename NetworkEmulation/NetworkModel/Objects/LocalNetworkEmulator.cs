using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class LocalNetworkEmulator : INetworkEmulator
    {
        #region Members
        string mEmulationWorkspacePath;
        #endregion

        #region Properties
        
        public List<ICoreNode> Nodes { get; set; }
        public List<IEndpointNode> EndNodes { get; set; }

        public event EventHandler<OnRoutingProbeReceivedArgs> OnRoutingProbeMovement;
        #endregion

        #region Constructors

        public LocalNetworkEmulator(string pWorkspacePath)
        {
            Nodes = new List<ICoreNode>();
            EndNodes = new List<IEndpointNode>();

            if (!Directory.Exists(pWorkspacePath))
            {
                //Create a new workspace
                Directory.CreateDirectory(pWorkspacePath);   
            }

            mEmulationWorkspacePath = pWorkspacePath;
        }

        #endregion

        #region Public Methods
        public void Save(string xmlFile)
        {

        }

        public void Load(string xmlFile)
        {

        }

        public void AddNode(ICoreNode node)
        {
            //Check for duplicate IPAddresses.
            foreach (ICoreNode n in Nodes)
            {
                if (n.NodeAddress == node.NodeAddress)
                    throw new Exception("Duplicate IPAddress for new added node. Address '" + node.NodeAddress.ToString() + "' already exists in the network.");
            }
            foreach (IEndpointNode n in EndNodes)
            {
                if (n.NodeAddress == node.NodeAddress)
                    throw new Exception("Duplicate IPAddress for new added node. Address '" + node.NodeAddress.ToString() + "' already exists in the network.");
            }

            Nodes.Add(node);
            Nodes.Last().OnRoutingProbeReceived += LocalNetworkEmulator_OnRoutingProbeReceived;
            Nodes.Last().OnPacketReceived += LocalNetworkEmulator_OnPacketReceived;
        }

        public void AddEndNode(IEndpointNode endNode)
        {
            //Check for duplicate IPAddresses.
            foreach (ICoreNode n in Nodes)
            {
                if (n.NodeAddress == endNode.NodeAddress)
                    throw new Exception("Duplicate IPAddress for new added node. Address '" + endNode.NodeAddress.ToString() + "' already exists in the network.");
            }
            foreach (IEndpointNode n in EndNodes)
            {
                if (n.NodeAddress == endNode.NodeAddress)
                    throw new Exception("Duplicate IPAddress for new added node. Address '" + endNode.NodeAddress.ToString() + "' already exists in the network.");
            }

            EndNodes.Add(endNode);
            EndNodes.Last().OnRoutingProbeReceived += LocalNetworkEmulator_OnRoutingProbeReceived_Endpoint;
            EndNodes.Last().OnPacketReceived += LocalNetworkEmulator_OnPacketReceived_Endpoint;

            Directory.CreateDirectory(mEmulationWorkspacePath + "/" + endNode.NodeAddress.ToString());
        }


        public void AddLink(IPAddress firstNodeAddress, int firstPort, IPAddress secondNodeAddress, int secondPort, double DropProbability, double ErrorProbability)
        {
            INetworkNode firstNode = null;
            INetworkNode secondNode = null;

            //Find the node in the network
            foreach (INetworkNode n in Nodes)
            {
                if (n.NodeAddress == firstNodeAddress)
                    firstNode = n;
                if (n.NodeAddress == secondNodeAddress)
                    secondNode = n;

                foreach (INodeConnection conn in n.ConnectionPoints.Values)
                {
                    if (conn.LocalPort == firstPort)
                        throw new Exception("Port " + firstPort.ToString() + " is already in use somewhere in the network. Ports must be unique.");
                    if (conn.LocalPort == secondPort)
                        throw new Exception("Port " + secondPort.ToString() + " is already in use somewhere in the network. Ports must be unique.");
                }
            }

            //Find if it's a endpoint
            foreach (IEndpointNode n in EndNodes)
            {
                if (n.NodeAddress == firstNodeAddress)
                    firstNode = n;
                if (n.NodeAddress == secondNodeAddress)
                    secondNode = n;

                foreach (INodeConnection conn in n.ConnectionPoints.Values)
                {
                    if (conn.LocalPort == firstPort)
                        throw new Exception("Port " + firstPort.ToString() + " is already in use somewhere in the network. Ports must be unique.");
                    if (conn.LocalPort == secondPort)
                        throw new Exception("Port " + secondPort.ToString() + " is already in use somewhere in the network. Ports must be unique.");
                }
            }

            if (firstNode == null || secondNode == null)
                throw new Exception("Link cannot be created, no node with IPAddress provided found.");

            INodeConnection firstConn = new NodeConnection(firstNode.NodeAddress,
                                "localhost",
                                firstPort,
                                secondPort,
                                new IPEndPoint(secondNodeAddress, secondPort),
                                System.Net.Sockets.AddressFamily.InterNetwork,
                                DropProbability,
                                ErrorProbability);
            firstNode.AddConnectionPoint(firstConn);

            INodeConnection secondConn = new NodeConnection(secondNode.NodeAddress,
                                "localhost",
                                secondPort,
                                firstPort,
                                new IPEndPoint(firstNodeAddress, firstPort),
                                System.Net.Sockets.AddressFamily.InterNetwork,
                                DropProbability,
                                ErrorProbability);
            secondNode.AddConnectionPoint(secondConn);
        }

        public void BeginEmulation()
        {
            foreach (ICoreNode node in Nodes)
            {
                node.Listen();
            }

            foreach (IEndpointNode node in EndNodes)
            {
                node.Listen();
            }
        }

        public void StopEmulation()
        {
            foreach (ICoreNode node in Nodes)
            {
                node.StopListen();
            }

            foreach (IEndpointNode node in EndNodes)
            {
                node.StopListen();
            }
        }

        public void SendRoutingProbe(IPAddress startNode, int id)
        {
            INetworkNode routingNode = Nodes.Find(n => n.NodeAddress == startNode);
            if (routingNode == null)
                routingNode = EndNodes.Find(n => n.NodeAddress == startNode);

            if (routingNode == null)
                throw new Exception("Node '" + startNode.ToString() + "' could not be found in the network.");

            routingNode.Send(new RoutingProbePacket(id));
        }

        public void TransferFile(IPAddress from, string srcName, IPAddress to, string dstName)
        {
            string srcFullPath = mEmulationWorkspacePath + "/" + from.ToString() + "/" + srcName;
            string dstFullPath = mEmulationWorkspacePath + "/" + to.ToString() + "/" + dstName;

            FileStream fs = new FileStream(dstFullPath, FileMode.Create, FileAccess.ReadWrite);

            foreach(IEndpointNode en in EndNodes)
            {
                if (en.NodeAddress == to)
                {
                    en.RequestFileTransfer(from, srcFullPath, fs);
                    break;
                }
            }
        }
        #endregion

        #region Private Methods
        private void LocalNetworkEmulator_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            throw new NotImplementedException();
        }

        private void LocalNetworkEmulator_OnRoutingProbeReceived(object sender, OnRoutingProbeReceivedArgs e)
        {
            if (OnRoutingProbeMovement != null)
            {
                OnRoutingProbeMovement.Invoke(this, e);
            }
        }

        private void LocalNetworkEmulator_OnPacketReceived_Endpoint(object sender, OnPacketReceivedArgs e)
        {
            throw new NotImplementedException();
        }

        private void LocalNetworkEmulator_OnRoutingProbeReceived_Endpoint(object sender, OnRoutingProbeReceivedArgs e)
        {
            if (OnRoutingProbeMovement != null)
            {
                OnRoutingProbeMovement.Invoke(this, e);
            }
        }
        #endregion

    }
}
