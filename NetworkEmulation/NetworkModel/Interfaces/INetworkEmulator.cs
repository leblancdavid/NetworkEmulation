using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public interface INetworkEmulator
    {
        List<ICoreNode> Nodes { get; set; }
        List<IEndpointNode> EndNodes { get; set; }

        event EventHandler<OnRoutingProbeReceivedArgs> OnRoutingProbeMovement;

        void Save(string xmlFile);
        void Load(string xmlFile);

        void AddNode(ICoreNode node);
        void AddEndNode(IEndpointNode endNode);
        void AddLink(IPAddress firstNodeAddress, int firstPort, IPAddress secondNodeAddress, int secondPort, double DropProbability, double ErrorProbability);

        void BeginEmulation();
        void StopEmulation();

        void SendRoutingProbe(IPAddress startNode, int id);
        void TransferFile(IPAddress from, string srcName, IPAddress to, string dstName);
    }
}
