using NetworkEmulation.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulatorApp
{
    public class NetworkWorker
    {
        #region Members
        
        private bool Stop = false;
        private bool ProbeLog = true;

        private DateTime mRoutingTimerStart;
        private int mRoutingCount;
        private double mAvgHopsPerSecond;
        private double timeoutWindow = 10000;
        private double dropProbability = 0.0;
        private double errorProbability = 0.0;
        private double delay = 0;
        #endregion

        #region Properties
        public INetworkEmulator Emulator;

        public event EventHandler<OnPacketReceivedArgs> OnPacketReceived;
        public event EventHandler<OnPacketReceivedArgs> OnPacketDropped;
        public event EventHandler<OnPacketReceivedArgs> OnPacketError;
        #endregion

        #region Constructors
        public NetworkWorker()
        {

        }

        public NetworkWorker(double timeoutWin, double dropProb, double errorProb)
        {
            timeoutWindow = timeoutWin;
            dropProbability = dropProb;
            errorProbability = errorProb;
        }
        #endregion

        #region Public Methods
        public void BuildDemoNetwork(string WorkspacePath)
        {
            Emulator = new LocalNetworkEmulator(WorkspacePath);
            Emulator.OnRoutingProbeMovement += emulator_OnRoutingProbeMovement;

            IPAddress addr1 = new IPAddress(11111);
            IPAddress addr2 = new IPAddress(22222);
            IPAddress addr3 = new IPAddress(33333);
            IPAddress addr4 = new IPAddress(44444);

            IPAddress ep1 = new IPAddress(55555);
            IPAddress ep2 = new IPAddress(66666);
            IPAddress ep3 = new IPAddress(77777);
            IPAddress ep4 = new IPAddress(88888);

            Emulator.AddNode(new CoreNode(addr1, delay));
            Emulator.AddNode(new CoreNode(addr2, delay));
            Emulator.AddNode(new CoreNode(addr3, delay));
            Emulator.AddNode(new CoreNode(addr4, delay));

            Emulator.AddEndNode(new EndpointNode(ep1, timeoutWindow, delay));
            Emulator.AddEndNode(new EndpointNode(ep2, timeoutWindow, delay));
            Emulator.AddEndNode(new EndpointNode(ep3, timeoutWindow, delay));
            Emulator.AddEndNode(new EndpointNode(ep4, timeoutWindow, delay));

            //Fully connected network
            Emulator.AddLink(addr1, 1000, addr2, 1001, dropProbability, errorProbability);
            Emulator.AddLink(addr1, 1002, addr3, 1003, dropProbability, errorProbability);
            Emulator.AddLink(addr1, 1004, addr4, 1005, dropProbability, errorProbability);

            Emulator.AddLink(addr2, 1006, addr3, 1007, dropProbability, errorProbability);
            Emulator.AddLink(addr2, 1008, addr4, 1009, dropProbability, errorProbability);


            Emulator.AddLink(addr3, 1010, addr4, 1011, dropProbability, errorProbability);

            //Add endpoint links
            Emulator.AddLink(ep1, 1100, addr1, 1101, dropProbability, errorProbability);
            Emulator.AddLink(ep2, 1102, addr2, 1103, dropProbability, errorProbability);
            Emulator.AddLink(ep3, 1104, addr3, 1105, dropProbability, errorProbability);
            Emulator.AddLink(ep4, 1106, addr4, 1107, dropProbability, errorProbability);

            foreach(INetworkNode node in Emulator.Nodes)
            {
                node.OnPacketReceived += NetworkWorker_OnPacketReceived;
                foreach(INodeConnection connection in node.ConnectionPoints.Values)
                {
                    connection.OnPacketReceived += Connection_OnPacketReceived;
                    connection.OnPacketDropped += Connection_OnPacketDropped;
                    connection.OnPacketError += Connection_OnPacketError;
                }
            }

            foreach(INetworkNode node in Emulator.EndNodes)
            {
                node.OnPacketReceived += NetworkWorker_OnPacketReceived;
                foreach(INodeConnection connection in node.ConnectionPoints.Values)
                {
                    connection.OnPacketReceived += Connection_OnPacketReceived;
                    connection.OnPacketDropped += Connection_OnPacketDropped;
                    connection.OnPacketError += Connection_OnPacketError;
                }
            }
        }

        public void StartNetworkEmulator()
        {
            Emulator.BeginEmulation();
        }

        public void StopNetworkEmulator()
        {
            Emulator.StopEmulation();
        }

        public void SendRoutingProbe()
        {
            Random rand = new Random();
            Emulator.SendRoutingProbe(Emulator.EndNodes.First().NodeAddress, rand.Next(0, 5000));
        }

        public void SetSimulationDelay(double delay)
        {
            foreach (INetworkNode node in Emulator.Nodes)
            {
                node.Delay = delay;
            }

            foreach (INetworkNode node in Emulator.EndNodes)
            {
                node.Delay = delay;
            }
        }

        public void SetErrorProbability(double ep)
        {
            foreach (INetworkNode node in Emulator.Nodes)
            {
                foreach (INodeConnection connection in node.ConnectionPoints.Values)
                {
                    connection.ErrorProbability = ep;
                }
            }

            foreach (INetworkNode node in Emulator.EndNodes)
            {
                foreach (INodeConnection connection in node.ConnectionPoints.Values)
                {
                    connection.ErrorProbability = ep;
                }
            }
        }

        public void SetDropProbability(double dp)
        {
            foreach (INetworkNode node in Emulator.Nodes)
            {
                foreach (INodeConnection connection in node.ConnectionPoints.Values)
                {
                    connection.DropProbability = dp;
                }
            }

            foreach (INetworkNode node in Emulator.EndNodes)
            {
                foreach (INodeConnection connection in node.ConnectionPoints.Values)
                {
                    connection.DropProbability = dp;
                }
            }
        }

        #endregion

        #region Private Methods
        private void emulator_OnRoutingProbeMovement(object sender, OnRoutingProbeReceivedArgs e)
        {
            if (mRoutingCount == 0)
            {
                mRoutingTimerStart = DateTime.UtcNow;
            }
            else if (mRoutingCount == 1000)
            {
                TimeSpan ts = DateTime.UtcNow - mRoutingTimerStart;
                mAvgHopsPerSecond = ts.TotalMilliseconds / 1000.0;
            }
            
            if(ProbeLog)
                Console.WriteLine(e.Time.ToString() + " - ID: " + e.ProbeId.ToString() + " - From: " + e.PreviousAddress.ToString() + ", At: " + e.CurrentAddress.ToString() + ", To: " + e.NextAddress.ToString());
        }

        private void NetworkWorker_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            if (OnPacketReceived != null)
            {
                OnPacketReceived(sender, e);
            }
        }

        private void Connection_OnPacketReceived(object sender, OnPacketReceivedArgs e)
        {
            if (OnPacketReceived != null)
            {
                OnPacketReceived(sender, e);
            }
        }

        private void Connection_OnPacketError(object sender, OnPacketReceivedArgs e)
        {
            if (OnPacketError != null)
            {
                OnPacketError(sender, e);
            }
        }

        private void Connection_OnPacketDropped(object sender, OnPacketReceivedArgs e)
        {
            if (OnPacketDropped != null)
            {
                OnPacketDropped(sender, e);
            }
        }
        #endregion
    }
}
