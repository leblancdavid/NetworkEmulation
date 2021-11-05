using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NetworkEmulation.Protocols
{
    public class FileTransferProtocol
    {
        #region Members
        protected long mFileSize;
        protected Timer mConnectionTimer;
        #endregion

        #region Properties
        public Guid TaskId { get; set; }
        public IPAddress Source { get; set; }
        public IPAddress Destination { get; set; }
        public long CurrentFileIndex { get; set; }
        public long BlockSize { get; set; }
        public bool IsDone { get; set; }
        public double ConnectionTimeout { get; set; }
        #endregion

        #region Constructors
        public FileTransferProtocol(Guid pTaskId, long pBlockSize, IPAddress pSource, IPAddress pDestination, double pConnectionTimeout)
        {
            TaskId = pTaskId;
            CurrentFileIndex = 0;
            mFileSize = 0;
            BlockSize = pBlockSize;
            Source = pSource;
            Destination = pDestination;
            IsDone = false;

            ConnectionTimeout = pConnectionTimeout; 
            mConnectionTimer = new Timer(ConnectionTimeout);
            mConnectionTimer.Elapsed += mConnectionTimer_Elapsed;
        }

        
        #endregion

        #region Public Methods

        public event EventHandler OnConnectionTimeout;
        public event EventHandler OnFileTransferCompleted;

        virtual public void ProcessPacket(IPacket packet) { }
        #endregion

        #region Private Methods
        protected void ResetConnectionTimer()
        {
            mConnectionTimer.Elapsed -= mConnectionTimer_Elapsed;
            mConnectionTimer = new Timer(ConnectionTimeout);
            mConnectionTimer.Elapsed += mConnectionTimer_Elapsed;
        }

        private void mConnectionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (OnConnectionTimeout != null)
            {
                OnConnectionTimeout(this, e);
            }
        }
        #endregion
    }
}
