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
    class Program
    {
        static void Main(string[] args)
        {
            NetworkWorker worker = new NetworkWorker();
            Thread emulation = new Thread(worker.StartNetworkEmulator);

            emulation.Start();
            Console.WriteLine("main thread: Starting emulation thread...");

            while (!emulation.IsAlive) ;

            Console.ReadLine();

            worker.StopNetworkEmulator();
            emulation.Join();

        }
    }
}
