using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.NetworkModel
{
    public class OnRoutingProbeReceivedArgs : EventArgs
    {
        #region Properties
        public IPAddress CurrentAddress { get; set; }
        public IPAddress PreviousAddress { get; set; }
        public IPAddress NextAddress { get; set; }
        public int ProbeId { get; set; }
        public DateTime Time { get; set; }
        #endregion

        #region Constructors
        public OnRoutingProbeReceivedArgs(IPAddress pPrevious, IPAddress pCurrentAddress, IPAddress pNext, int pProbeId, DateTime pTime)
        {
            PreviousAddress = pPrevious;
            CurrentAddress = pCurrentAddress;
            NextAddress = pNext;
            ProbeId = pProbeId;
            Time = pTime;
        }
        #endregion
    }
}
