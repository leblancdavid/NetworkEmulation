using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.Protocols
{
    public class FileBlock
    {
        #region Properties
        public byte[] Data { get; set; }
        public long FileIndex { get; set; }
        public DateTime Timestamp { get; set; }
        #endregion

        #region Constructor
        public FileBlock()
        {
            Data = new byte[0];
            FileIndex = 0;
            Timestamp = DateTime.Now;
        }

        public FileBlock(byte[] pData, long pFileIndex)
        {
            Data = pData;
            FileIndex = pFileIndex;
            Timestamp = DateTime.Now;
        }
        #endregion
    }
}
