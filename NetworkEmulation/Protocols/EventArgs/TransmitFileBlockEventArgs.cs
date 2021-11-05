
using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.Protocols
{
    public class TransmitFileBlockEventArgs : EventArgs
    {
        #region Properties
        public FileTransferPacket Packet { get; set; }
        #endregion

        #region Constructors

        public TransmitFileBlockEventArgs(FileTransferPacket pPacket)
        {
            Packet = pPacket;
        }

        #endregion
    }
}
