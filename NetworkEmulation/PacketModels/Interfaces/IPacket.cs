using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    public interface IPacket
    {
        byte[] PacketBytes { get; set; }
        uint HeaderSize { get; }
        uint DataSize { get; }
        IPAddress SourceAddress { get; }
        IPAddress DestinationAddress { get; }
        PacketTypes Type { get; }

        byte[] GetHeader();
        byte[] GetData();
        void SetData(byte[] data);

        Int32 GetFlagValue(Int32 flag);
        void SetFlagValue(Int32 flag, Int32 val);
    }
}
