using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    using HeaderFlag = System.Int32;
    public class BasicPacket : IPacket
    {
        #region Members
        private const uint SRC_ADDR = 0;
        private const uint DST_ADDR = 4;
        private const uint TYPE_ADDR = 8;
        private const uint HEADER_SIZE_ADDR = 9;
        private const uint DATA_SIZE_ADDR = 10;

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
        public BasicPacket()
        {
            PacketBytes = null;
            HeaderSize = 0;
            DataSize = 0;
            SourceAddress = IPAddress.Any;
            DestinationAddress = IPAddress.Any;
            Type = PacketTypes.Basic;
        }

        public BasicPacket(byte[] data)
        {
            PacketBytes = data;
            Type = PacketTypes.Basic;
        }

        public BasicPacket(IPAddress pSource, IPAddress pDestination, PacketTypes pType)
        {
            //For the basic packet, I'll just use source and destination address, and type.
            //Length = 4 (srcAddr) + 4 (dstAddr) + 1 (type) + 1 (hdrSize) + 2 (dataSize) = 12
            short headerSize = 12;
            short dataSize = 0;
            PacketBytes = new byte[headerSize + dataSize];
            byte[] field = pSource.GetAddressBytes();
            Array.Copy(field, 0, PacketBytes, (int)SRC_ADDR, field.Length);

            field = pDestination.GetAddressBytes();
            Array.Copy(field, 0, PacketBytes, (int)DST_ADDR, field.Length);

            PacketBytes[TYPE_ADDR] = (byte)pType;
            PacketBytes[HEADER_SIZE_ADDR] = (byte)headerSize; //header size;

            field = BitConverter.GetBytes(dataSize);
            Array.Copy(field, 0, PacketBytes, (int)DATA_SIZE_ADDR, field.Length);

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

        #endregion

        
    }
}
