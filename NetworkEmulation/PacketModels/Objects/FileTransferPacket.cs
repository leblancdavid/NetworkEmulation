using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    public class FileTransferPacket : IPacket
    {
        #region Members
        private const uint TYPE_ADDR = 0;
        private const uint SRC_ADDR = 1;
        private const uint DST_ADDR = 5;
        private const uint HEADER_SIZE_ADDR = 9;
        private const uint DATA_SIZE_ADDR = 10;
        private const uint TRANSFER_ID_ADDR = 12;
        private const uint FILE_SIZE_ADDR = 28;
        private const uint BLOCK_INDEX_ADDR = 36;
        private const uint TRANSFER_CODE_ADDR = 44;
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

        public FileTransferCommandCode Code
        {
            get
            {
                if (PacketBytes != null)
                {
                    return (FileTransferCommandCode)PacketBytes[TRANSFER_CODE_ADDR];
                }
                else
                    return FileTransferCommandCode.Unknown;
            }
            set
            {
                PacketBytes[TRANSFER_CODE_ADDR] = (byte)value;
            }
        }

        public Guid TransferId
        {
            get
            {
                if (PacketBytes != null)
                {
                    byte[] id = new byte[16];
                    for (int i = 0; i < 16; ++i)
                    {
                        id[i] = PacketBytes[TRANSFER_ID_ADDR + i];
                    }
                    return new Guid(id);
                }
                else
                    return Guid.Empty;
            }
            private set { }
        }

        public long FileSize
        {
            get
            {
                if (PacketBytes != null)
                {
                    return BitConverter.ToInt64(PacketBytes, (int)FILE_SIZE_ADDR);
                }
                else
                    return 0;
            }
            private set { }
        }

        public long BlockIndex
        {
            get
            {
                if (PacketBytes != null)
                {
                    return BitConverter.ToInt64(PacketBytes, (int)BLOCK_INDEX_ADDR);
                }
                else
                    return 0;
            }
            private set { }
        }
        #endregion

        #region Constructors
        public FileTransferPacket()
        {
            PacketBytes = null;
            HeaderSize = 0;
            DataSize = 0;
            SourceAddress = IPAddress.Any;
            DestinationAddress = IPAddress.Any;
            Type = PacketTypes.FileTransfer;
        }

        public FileTransferPacket(byte[] data)
        {
            PacketBytes = data;
            Type = PacketTypes.FileTransfer;
        }

        public FileTransferPacket(IPAddress pSource, 
            IPAddress pDestination, 
            FileTransferCommandCode code,
            Guid pId,
            long pFileSize,
            long pBlockIndex,
            byte[] data)
        {
            short headerSize = 45;
            short dataSize = (short)data.Length;
            PacketBytes = new byte[headerSize + dataSize];
            byte[] field = pSource.GetAddressBytes();
            Array.Copy(field, 0, PacketBytes, (int)SRC_ADDR, field.Length);

            field = pDestination.GetAddressBytes();
            Array.Copy(field, 0, PacketBytes, (int)DST_ADDR, field.Length);

            PacketBytes[TYPE_ADDR] = (byte)PacketTypes.FileTransfer;
            PacketBytes[HEADER_SIZE_ADDR] = (byte)headerSize; //header size;

            field = pId.ToByteArray();
            Array.Copy(field, 0, PacketBytes, (int)TRANSFER_ID_ADDR, field.Length);

            field = BitConverter.GetBytes(pFileSize);
            Array.Copy(field, 0, PacketBytes, (int)FILE_SIZE_ADDR, field.Length);

            field = BitConverter.GetBytes(pBlockIndex);
            Array.Copy(field, 0, PacketBytes, (int)BLOCK_INDEX_ADDR, field.Length);

            PacketBytes[TRANSFER_CODE_ADDR] = (byte)code;

            SetData(data);
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
                data[i] = PacketBytes[hSize + i];
            }

            return data; 
        }

        public void SetData(byte[] data)
        {
            short dataSize = (short)data.Length;
            byte[] field = BitConverter.GetBytes(dataSize);
            Array.Copy(field, 0, PacketBytes, (int)DATA_SIZE_ADDR, field.Length);

            uint hSize = HeaderSize;
            for (uint i = 0; i < dataSize; ++i)
            {
                PacketBytes[hSize + i] = data[i];
            }
        }

        #endregion
    }
}
