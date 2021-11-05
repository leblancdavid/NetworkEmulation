using NetworkEmulation.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkEmulationTestConsole
{
    public class NetworkWorker
    {
        private INetworkEmulator emulator;
        private bool Stop = false;
        private bool ProbeLog = true;

        private DateTime mRoutingTimerStart;
        private int mRoutingCount;
        private double mAvgHopsPerSecond;
        private double timeoutWindow = 10000;
        private double dropProbability = 0.01;
        private double errorProbability = 0.001;
        private double delay = 0;

        public void StartNetworkEmulator()
        {
            emulator = new LocalNetworkEmulator("C:/Users/David/Documents/School Work/CPE701/Workspace/TestWorkspace");
            emulator.OnRoutingProbeMovement += emulator_OnRoutingProbeMovement;

            IPAddress addr1 = new IPAddress(1);
            IPAddress addr2 = new IPAddress(2);
            IPAddress addr3 = new IPAddress(3);
            IPAddress addr4 = new IPAddress(4);

            IPAddress ep1 = new IPAddress(5);
            IPAddress ep2 = new IPAddress(6);
            IPAddress ep3 = new IPAddress(7);
            IPAddress ep4 = new IPAddress(8);

            emulator.AddNode(new CoreNode(addr1, delay));
            emulator.AddNode(new CoreNode(addr2, delay));
            emulator.AddNode(new CoreNode(addr3, delay));
            emulator.AddNode(new CoreNode(addr4, delay));

            emulator.AddEndNode(new EndpointNode(ep1, timeoutWindow, delay));
            emulator.AddEndNode(new EndpointNode(ep2, timeoutWindow, delay));
            emulator.AddEndNode(new EndpointNode(ep3, timeoutWindow, delay));
            emulator.AddEndNode(new EndpointNode(ep4, timeoutWindow, delay));

            //Fully connected network
            emulator.AddLink(addr1, 1000, addr2, 1001, dropProbability, errorProbability);
            emulator.AddLink(addr1, 1002, addr3, 1003, dropProbability, errorProbability);
            emulator.AddLink(addr1, 1004, addr4, 1005, dropProbability, errorProbability);

            emulator.AddLink(addr2, 1006, addr3, 1007, dropProbability, errorProbability);
            emulator.AddLink(addr2, 1008, addr4, 1009, dropProbability, errorProbability);


            emulator.AddLink(addr3, 1010, addr4, 1011, dropProbability, errorProbability);

            //Add endpoint links
            emulator.AddLink(ep1, 1100, addr1, 1101, dropProbability, errorProbability);
            emulator.AddLink(ep2, 1102, addr2, 1103, dropProbability, errorProbability);
            emulator.AddLink(ep3, 1104, addr3, 1105, dropProbability, errorProbability);
            emulator.AddLink(ep4, 1106, addr4, 1107, dropProbability, errorProbability);

            emulator.BeginEmulation();

            mRoutingCount = 0;

            ProbeLog = true;
            emulator.SendRoutingProbe(ep1, 42);

            Thread.Sleep(1000);

            ProbeLog = false;

            emulator.TransferFile(ep1, "TestDoc.pdf", ep4, "TestDoc_T.pdf");

            while (!Stop) ;
        }

        public void StopNetworkEmulator()
        {
            emulator.StopEmulation();
            Stop = true;
        }

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
    }
}
