using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.PacketModels
{
    using HeaderFlag = System.Int32;
    public static class PacketHeaderFlags
    {
        public const HeaderFlag SourceAddress = 0;
        public const HeaderFlag DestinationAddress = 1;
        public const HeaderFlag HeaderSize = 2;
        public const HeaderFlag DataSize = 3;
        public const HeaderFlag PacketType = 4;
        public const HeaderFlag LifeTime = 5;
    }
}
