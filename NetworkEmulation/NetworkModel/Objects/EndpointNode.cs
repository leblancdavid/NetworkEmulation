using NetworkEmulation.PacketModels;
using NetworkEmulation.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class EndpointNode : IEndpointNode
    {
        #region Members
        private FileStream mFileStream;
        private long BlockSize = 4096;
        private int ThroughputDelay = 10;
        private double TimeoutDelay = 10000;
        private double ConnectionTimeout = 300000;//5min connection
        #endregion

        #region Properties
        public IPAddress NodeAddress { get; set; }
        public Dictionary<int, INodeConnection> ConnectionPoints { get; private set; }

        public double Delay { get; set; }

        public List<FileTransferProtocol> FileTransferTasks { get; set; }

        public event EventHandler<OnPacketReceivedArgs> OnPacketReceived;
        public event EventHandler<OnRoutingProbeReceivedArgs> OnRoutingProbeReceived;
        public event EventHandler<EventArgs> OnFileTransferCompleted;
        
        #endregion

        #region Constructors
        public EndpointNode()
        {
            NodeAddress = new IPAddress(0);
            ConnectionPoints = new Dictionary<int, INodeConnection>();
            FileTransferTasks = new List<FileTransferProtocol>();
            Delay = 0;
        }

        public EndpointNode(IPAddress pAddress, double pTimeout, double delay)
        {
            NodeAddress = pAddress;
            TimeoutDelay = pTimeout;
            ConnectionPoints = new Dictionary<int, INodeConnection>();
            FileTransferTasks = new List<FileTransferProtocol>();
            Delay = delay;
        }

        #endregion

        #region Public Methods

        public void AddConnectionPoint(INodeConnection pConnection)
        {
            if (ConnectionPoints.Count != 0)
            {
                throw new Exception("EndPoint Node '" + NodeAddress.ToString() + "' already has a connection. Cannot add more then one connection to an Endpoint Node.");
            }
            else
            {
                ConnectionPoints.Add(pConnection.LocalPort, pConnection);
                ConnectionPoints[pConnection.LocalPort].OnPacketReceived += EndpointNode_OnPacketReceived;
            }
        }

        public void RemoveConnectionPoint(int pPort)
        {
            if (ConnectionPoints.ContainsKey(pPort))
            {
                ConnectionPoints[pPort].OnPacketReceived -= EndpointNode_OnPacketReceived;
                ConnectionPoints.Remove(pPort);
            }
            else
            {
                throw new Exception("There is no connection on port '" + pPort.ToString() + "' for Node '" + NodeAddress.ToString() + "'.");
            }
        }

        public void Listen()
        {
            foreach (INodeConnection conn in ConnectionPoints.Values)
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
                ProcessRoutingProbe((RoutingProbePacket)packet);
            }
        }

        public void RequestFileTransfer(IPAddress destination, string FileName, FileStream fs)
        {
            mFileStream = fs;
            Guid newProtocolId = Guid.NewGuid();

            RequestFileTransferProtocol rftp = new RequestFileTransferProtocol(newProtocolId, fs, ThroughputDelay, BlockSize, TimeoutDelay, this.NodeAddress, destination, ConnectionTimeout);
            rftp.OnTransmitRequest += RFTP_OnReTransmitRequest;
            rftp.OnFileTransferCompleted += RFTP_OnFileTransferCompleted;
            FileTransferTasks.Add(rftp);

            //Thread receive = new Thread(() => BeginReceiveFileTransfer((RequestFileTransferProtocol)FileTransferTasks.Last(), fs));
            //receive.Start();

            IPacket FTRequest = new FileTransferPacket(this.NodeAddress,
                                                destination,
                                                FileTransferCommandCode.FileRequest,
                                                newProtocolId,
                                                0, 0,
                                                Encoding.ASCII.GetBytes(FileName));

            ConnectionPoints.First().Value.Send(FTRequest);
        }

        

        #endregion

        #region Private Methods

        //private void BeginReceiveFileTransfer(RequestFileTransferProtocol task, FileStream fs)
        //{
        //    bool done = false;
        //    while (!done)
        //    {
        //        FileBlock block = null;
        //        //If we are at the last block, then we are done
        //        if (task.GetNextFileBlock(out block))
        //            done = true;

        //        if (block != null)
        //        {
        //            foreach (byte b in block.Data)
        //            {
        //                fs.WriteByte(b);
        //            }
        //        }
        //        else
        //        {
        //            Thread.Sleep(ThroughputDelay);
        //        }
        //    }
        //    fs.Close();
        //}

        //private void BeginSendFileTransfer(SendFileTransferProtocol task)
        //{
        //    bool done = false;
        //    while (!done)
        //    {
        //        Thread.Sleep(ThroughputDelay);

        //        FileBlock b = null;
        //        if (task.GetNextFileBlock(out b))
        //            done = true;

        //        IPacket packet = new FileTransferPacket(this.NodeAddress,
        //                                task.Destination,
        //                                FileTransferCommandCode.FileData,
        //                                task.TaskId,
        //                                task.FileSize,
        //                                b.FileIndex,
        //                                b.Data);

        //        ConnectionPoints.First().Value.Send(packet);
        //    }
        //}

        private void EndpointNode_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            Thread.Sleep((int)Delay);
            if (e.Packet.Type == PacketModels.PacketTypes.RoutingProbe)
            {
                ProcessRoutingProbe((RoutingProbePacket)e.Packet);
            }
            else if (e.Packet.Type == PacketTypes.FileTransfer)
            {
                ProcessFileTransferPacket((FileTransferPacket)e.Packet);
                InvokeOnPacketReceived(e);
            }
        }

        private void ProcessFileTransferPacket(FileTransferPacket packet)
        {
            if (packet.Code == FileTransferCommandCode.FileRequest)
            {
                Guid newProtocolId = Guid.NewGuid();
                string fileName = Encoding.ASCII.GetString(packet.GetData());
                SendFileTransferProtocol sftp = new SendFileTransferProtocol(packet.TransferId, ThroughputDelay, BlockSize, fileName, this.NodeAddress, packet.SourceAddress, ConnectionTimeout);
                sftp.OnTransmitRequestProcessed += SFTP_OnReTransmitRequestProcessed;
                FileTransferTasks.Add(sftp);

                //Thread send = new Thread(() => BeginSendFileTransfer((SendFileTransferProtocol)FileTransferTasks.Last()));
                //send.Start();
            }

            foreach(FileTransferProtocol p in FileTransferTasks)
            {
                if (packet.TransferId == p.TaskId)
                {
                    p.ProcessPacket(packet);
                }
            }
             
        }

        

        private void ProcessRoutingProbe(RoutingProbePacket packet)
        {
            //Want to update the routing table, and forward it to some other random port
            List<int> connectionPorts = new List<int>();
            Random rand = new Random();
            List<int> ports = Enumerable.ToList(ConnectionPoints.Keys);
            int next = ports[rand.Next(ConnectionPoints.Count)];

            List<IPAddress> history = packet.GetHistory();

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
            //if (ttl < 500) //if it's not too many hops.
            //{
            //    packet.TimeToLive = (short)ttl;
            //    ConnectionPoints[next].Send(packet);
            //}

            ConnectionPoints[next].Send(packet);
        }

        private void NotifyOfRoutingPacketReceived(OnRoutingProbeReceivedArgs args)
        {
            if (OnRoutingProbeReceived != null)
            {
                OnRoutingProbeReceived.Invoke(this, args);
            }
        }

        private void RFTP_OnReTransmitRequest(object sender, TransmitFileBlockEventArgs e)
        {
            //Send the request for retransmission of lost packet
            ConnectionPoints.First().Value.Send(e.Packet);
        }

        void RFTP_OnFileTransferCompleted(object sender, EventArgs e)
        {
            if (OnFileTransferCompleted != null)
            {
                OnFileTransferCompleted(sender, e);
            }
        }

        private void SFTP_OnReTransmitRequestProcessed(object sender, TransmitFileBlockEventArgs e)
        {
            //Send the retransmission of lost packet
            ConnectionPoints.First().Value.Send(e.Packet);
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
