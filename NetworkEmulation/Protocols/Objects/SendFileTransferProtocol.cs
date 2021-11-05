using NetworkEmulation.PacketModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkEmulation.Protocols
{
    public class SendFileTransferProtocol : FileTransferProtocol
    {
        #region Members
        private string mFileName;
        private FileStream mFileStream;
        private FileStream mFileSeeker;
        private Thread mSendingThread;
        private int mThroughputDelay;

        private System.Threading.Timer mConnectionTimer;
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        #endregion

        #region Properties
        public long FileSize { get; set; }
        #endregion

        #region Constructors
        public SendFileTransferProtocol(Guid pTaskId, int pThroughputDelay, long pBlockSize, string pFileName, IPAddress pSource, IPAddress pDestination, double pConnectionTimeout)
            : base(pTaskId, pBlockSize, pSource, pDestination, pConnectionTimeout)
        {
            mFileName = pFileName;
            mFileStream = new FileStream(mFileName, FileMode.Open, FileAccess.Read);
            mFileSeeker = new FileStream(mFileName, FileMode.Open, FileAccess.Read);
            FileSize = mFileStream.Length;
            mThroughputDelay = pThroughputDelay;

            mConnectionTimer = new System.Threading.Timer(new System.Threading.TimerCallback(mConnectionTimer_Elapsed), autoEvent, (int)ConnectionTimeout, (int)(ConnectionTimeout / 5.0));

            mSendingThread = new Thread(() => BeginSendFileTransfer());
            mSendingThread.Start();
        }
        #endregion

        #region Public Methods

        public EventHandler<TransmitFileBlockEventArgs> OnTransmitRequestProcessed;
        new public event EventHandler OnFileTransferCompleted;

        override public void ProcessPacket(IPacket packet)
        {
            if (packet.Type == PacketTypes.FileTransfer)
            {
                FileTransferPacket ftp = (FileTransferPacket)packet;

                if (ftp.Code == FileTransferCommandCode.ReTransmition)
                {
                    //Go to the file index we are missing
                    mFileSeeker.Seek(ftp.BlockIndex, SeekOrigin.Begin);

                    long size = mFileSeeker.Length - ftp.BlockIndex;
                    if (size > BlockSize)
                        size = BlockSize;
                    FileBlock fb = new FileBlock();
                    fb.Data = new byte[size];
                    fb.FileIndex = CurrentFileIndex;

                    for (int i = 0; i < size; ++i)
                    {
                        fb.Data[i] = (byte)mFileSeeker.ReadByte();
                    }

                    FileTransferPacket retransmitPacket = new FileTransferPacket(Source,
                        Destination,
                        FileTransferCommandCode.FileData,
                        this.TaskId,
                        FileSize,
                        ftp.BlockIndex,
                        fb.Data);
                    //Notify that we got the requested packet
                    InvokeTransmitRequestProcessed(new TransmitFileBlockEventArgs(retransmitPacket));

                    ResetConnectionTimer();
                }
                else if(ftp.Code == FileTransferCommandCode.Completed)
                {
                    mFileSeeker.Close();
                }
            }
        }

        
        #endregion

        #region Private Method
        private void BeginSendFileTransfer()
        {
            bool done = false;
            while (!done)
            {
                Thread.Sleep(mThroughputDelay);

                FileBlock b = null;
                if (GetNextFileBlock(out b))
                    done = true;

                FileTransferPacket packet = new FileTransferPacket(this.Source,
                                        this.Destination,
                                        FileTransferCommandCode.FileData,
                                        this.TaskId,
                                        FileSize,
                                        b.FileIndex,
                                        b.Data);

                InvokeTransmitRequestProcessed(new TransmitFileBlockEventArgs(packet));

                ResetConnectionTimer();
            }
        }

        private bool GetNextFileBlock(out FileBlock fb)
        {
            bool lastBlock = false;
            long size = mFileStream.Length - CurrentFileIndex;
            if (size > BlockSize)
                size = BlockSize;
            else
                lastBlock = true;

            fb = new FileBlock();
            fb.Data = new byte[size];
            fb.FileIndex = CurrentFileIndex;

            for (int i = 0; i < size; ++i)
            {
                fb.Data[i] = (byte)mFileStream.ReadByte();
            }

            CurrentFileIndex += size;

            return lastBlock;
        }



        private void InvokeTransmitRequestProcessed(TransmitFileBlockEventArgs args)
        {
            if (OnTransmitRequestProcessed != null)
            {
                OnTransmitRequestProcessed(this, args);
            }
        }

        private void InvokeOnFileTransferCompleted()
        {
            if (OnFileTransferCompleted != null)
            {
                OnFileTransferCompleted(this, new EventArgs());
            }
        }

        private void ResetConnectionTimer()
        {
            mConnectionTimer.Change((int)ConnectionTimeout, (int)(ConnectionTimeout / 5.0));
        }

        void mConnectionTimer_Elapsed(object sender)
        {
            mFileSeeker.Close();
        }
        #endregion
    }
}
