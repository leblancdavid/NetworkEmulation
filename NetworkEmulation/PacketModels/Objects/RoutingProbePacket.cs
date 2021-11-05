using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    using HeaderFlag = System.Int32;
    public class RoutingProbePacket :  IPacket
    {
        #region Members
        
        private const uint TYPE_ADDR = 0;
        private const uint HEADER_SIZE_ADDR = 1;
        private const uint DATA_SIZE_ADDR = 2;
        private const uint SRC_ADDR = 4;
        private const uint DST_ADDR = 8;
        private const uint PCK_ID = 12;
        private const uint HOPS_ADDR = 16;
        private const uint TTL_ADDR = 18;

        private Dictionary<string, int> dict;
        #endregion

        #region Properties

        public byte[] PacketBytes { get; set; }
        
        public uint HeaderSize 
        { 
            get
            {
                if (PacketBytes != null)
                {
                    return (uint)PacketBytes[HEADER_SIZE_ADDR];
                }
                else
                    return 0;
            }
            private set { } 
        }

        public uint DataSize 
        {
            get
            {
                if (PacketBytes != null)
                {
                    return (uint)BitConverter.ToUInt16(PacketBytes, (int)DATA_SIZE_ADDR);
                }
                else
                    return 0;
            }
            private set { }
        }

        public short TimeToLive
        {
            get
            {
                if (PacketBytes != null)
                {
                    return (short)BitConverter.ToUInt16(PacketBytes, (int)TTL_ADDR);
                }
                else
                    return 0;
            }
            set 
            {
                if (PacketBytes != null)
                {
                    byte[] field = BitConverter.GetBytes(value);
                    Array.Copy(field, 0, PacketBytes, (int)TTL_ADDR, field.Length);
                }
            }
        }

        public int PacketId
        {
            get
            {
                if (PacketBytes != null)
                {
                    return (int)BitConverter.ToInt32(PacketBytes, (int)PCK_ID);
                }
                else
                    return 0;
            }
            private set { }
        }

        public uint NumberOfHops
        {
            get
            {
                if (PacketBytes != null)
                {
                    return (uint)PacketBytes[HOPS_ADDR];
                }
                else
                    return 0;
            }
            private set
            {
                PacketBytes[HOPS_ADDR] = (byte)value;
            }
        }

        public IPAddress SourceAddress 
        {
            get
            {
                if (PacketBytes != null)
                {
                    return new IPAddress(BitConverter.ToInt32(PacketBytes, (int)SRC_ADDR));
                }
                else
                    return IPAddress.Any;
            }
            private set { } 
        }

        public IPAddress DestinationAddress 
        {
            get
            {
                if (PacketBytes != null)
                {
                    return new IPAddress(BitConverter.ToInt32(PacketBytes, (int)DST_ADDR));
                }
                else
                    return IPAddress.Any;
            }
            private set { }
        }

        public PacketTypes Type 
        { 
            get
            {
                if (PacketBytes != null)
                {
                    return (PacketTypes)PacketBytes[TYPE_ADDR];
                }
                else
                    return PacketTypes.Unknown;
            }
            private set { } 
        }
        
        #endregion

        #region Constructors

        public RoutingProbePacket(int id)
        {
            short headerSize = 19;
            short dataSize = 400;
            PacketBytes = new byte[headerSize + dataSize];
            //dont care about source
            byte[] field = IPAddress.Any.GetAddressBytes();
            Array.Copy(field, 0, PacketBytes, (int)SRC_ADDR, field.Length);
            //dont care about destination
            field = IPAddress.Any.GetAddressBytes();
            Array.Copy(field, 0, PacketBytes, (int)DST_ADDR, field.Length);

            PacketBytes[TYPE_ADDR] = (byte)PacketTypes.RoutingProbe;
            PacketBytes[HEADER_SIZE_ADDR] = (byte)headerSize; //header size;

            field = BitConverter.GetBytes(dataSize);
            Array.Copy(field, 0, PacketBytes, (int)DATA_SIZE_ADDR, field.Length);

            field = BitConverter.GetBytes(id);
            Array.Copy(field, 0, PacketBytes, (int)PCK_ID, field.Length);

            field = BitConverter.GetBytes((short)0);
            Array.Copy(field, 0, PacketBytes, (int)TTL_ADDR, field.Length);

            Type = PacketTypes.RoutingProbe;
        }

        public RoutingProbePacket(byte[] data)
        {
            PacketBytes = data;
            Type = PacketTypes.RoutingProbe;
        }
        #endregion

        #region Public Methods

        public System.Int32 GetFlagValue(Int32 flag)
        {
            return 0;
        }

        public void SetFlagValue(Int32 flag, System.Int32 val)
        {
            return;
        }

        public byte[] GetHeader()
        {
            uint size = HeaderSize;
            byte[] header = new byte[size];
            for(uint i = 0; i < size; ++i)
            {
                header[i] = PacketBytes[i];
            }

            return header; 
        }

        public byte[] GetData()
        {
            uint size = DataSize;
            uint hSize = HeaderSize;
            byte[] data = new byte[size];
            for (uint i = 0; i < size; ++i)
            {
                data[hSize + i] = PacketBytes[hSize + i];
            }

            return data; 
        }

        public void SetData(byte[] data)
        {

        }

        public List<IPAddress> GetHistory()
        {
            List<IPAddress> history = new List<IPAddress>();

            uint hops = NumberOfHops;
            uint start = HeaderSize;
            uint end = start + hops * 4;
            byte[] addr = new byte[4];

            for (uint i = start; i < end; i += 4)
            {
                addr[0] = PacketBytes[i];
                addr[1] = PacketBytes[i + 1];
                addr[2] = PacketBytes[i + 2];
                addr[3] = PacketBytes[i + 3];

                history.Add(new IPAddress(addr));
            }

            return history;
        }

        public void SetHistory(List<IPAddress> history)
        {
            uint hops = (uint)history.Count;
            if (hops > DataSize / 4)
                hops = (uint)(DataSize / 4);

            //set the number of hops
            NumberOfHops = hops;

            byte[] addr;
            uint start = HeaderSize;
            uint end = start + hops * 4;
            uint i;
            int j;
            for (i = start, j = 0; i < end; i += 4, j++)
            {
                addr = history[j].GetAddressBytes();
                PacketBytes[i] = addr[0];
                PacketBytes[i + 1] = addr[1];
                PacketBytes[i + 2] = addr[2];
                PacketBytes[i + 3] = addr[3];
            }
        }

        #endregion

    }
}
