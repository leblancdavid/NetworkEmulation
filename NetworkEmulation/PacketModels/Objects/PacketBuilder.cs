using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    public static class PacketBuilder
    {
        public static IPacket GetPacketFromBytes(byte[] data)
        {
            PacketTypes type = (PacketTypes)((int)data[0]);

            IPacket packet = null;
            switch (type)
            {
                case PacketTypes.Basic:
                    packet = new BasicPacket(data);
                    break;
                case PacketTypes.RoutingProbe:
                    packet = new RoutingProbePacket(data);
                    break;
                case PacketTypes.FileTransfer:
                    packet = new FileTransferPacket(data);
                    break;
                default:
                    break;
            }

            return packet;
        }
    }
}
