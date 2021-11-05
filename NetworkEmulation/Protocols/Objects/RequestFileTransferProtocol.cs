using NetworkEmulation.PacketModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NetworkEmulation.Protocols
{
    public class RequestFileTransferProtocol : FileTransferProtocol
    {
        #region Members
        private MissingBlockQueueManager mMissingBlockQueue;
        private SortedList<long, FileBlock> mBuffer;
        private Thread mReceiveThread;
        private FileStream mFileStream;
        private int mThroughputDelay;
        #endregion

        #region Properties
        public EventHandler<TransmitFileBlockEventArgs> OnTransmitRequest;
        new public event EventHandler OnFileTransferCompleted;
        new public event EventHandler OnConnectionTimeout;
        #endregion

        #region Constructors
        public RequestFileTransferProtocol(Guid pTaskId, FileStream pFileStream, int pThroughputDelay, long pBlockSize, double timeout, IPAddress pSource, IPAddress pDestination, double pConnectionTimeout)
            : base(pTaskId, pBlockSize, pSource, pDestination, pConnectionTimeout)
        {
            mMissingBlockQueue = new MissingBlockQueueManager(timeout);
            mMissingBlockQueue.OnTimeout += MissingBlockQueue_OnTimeout;
            mBuffer = new SortedList<long, FileBlock>();
            mFileSize = Int64.MaxValue; //Assume the file could be really large

            mFileStream = pFileStream;
            mThroughputDelay = pThroughputDelay;

            //Start receiving the file
            mReceiveThread = new Thread(() => BeginReceiveFileTransfer());
            mReceiveThread.Start();
        }

        #endregion

        #region Public Methods
        override public void ProcessPacket(IPacket packet)
        {
            if (packet.Type == PacketTypes.FileTransfer)
            {
                FileTransferPacket ftp = (FileTransferPacket)packet;
                if (mFileSize != ftp.FileSize)
                {
                    mFileSize = ftp.FileSize;
                }

                if (ftp.Code == FileTransferCommandCode.FileData)
                {
                    //Remove if it's missing
                    mMissingBlockQueue.RemoveFileBlock(ftp.BlockIndex);

                    //this is a duplicate
                    if (ftp.BlockIndex < CurrentFileIndex)
                    {
                        return;
                    }
                    else
                    {
                        //Add it to the buffer (if it hasn't been added yet)
                        FileBlock block = new FileBlock(ftp.GetData(), ftp.BlockIndex);
                        if (!mBuffer.ContainsKey(block.FileIndex))
                        {
                            mBuffer.Add(block.FileIndex, block);
                        }

                        //Add any missing blocks
                        long index = CurrentFileIndex;
                        while (index < ftp.BlockIndex)
                        {
                            if (!mMissingBlockQueue.Contains(index))
                            {
                                mMissingBlockQueue.AddFileBlock(index);
                            }
                            index += BlockSize;
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods
        private void BeginReceiveFileTransfer()
        {
            bool done = false;
            while (!done)
            {
                FileBlock block = null;
                //If we are at the last block, then we are done
                if (GetNextFileBlock(out block))
                    done = true;

                if (block != null)
                {
                    foreach (byte b in block.Data)
                    {
                        mFileStream.WriteByte(b);
                    }
                }
                else
                {
                    Thread.Sleep(mThroughputDelay);
                }
            }
            mFileStream.Close();

            InvokeOnFileTransferCompleted();
        }

        private bool GetNextFileBlock(out FileBlock fb)
        {
            if (mBuffer.Count != 0 && mBuffer.First().Key == CurrentFileIndex)
            {
                fb = mBuffer.First().Value;
                CurrentFileIndex += BlockSize;
                mBuffer.RemoveAt(0);
            }
            else
            {
                fb = null;
            }

            if (CurrentFileIndex >= mFileSize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void MissingBlockQueue_OnTimeout(object sender, EventArgs e)
        {
            if (OnTransmitRequest != null)
            {
                MissingFileBlock mfb = (MissingFileBlock)sender;

                //dont request stuff we already have
                if (!mBuffer.ContainsKey(mfb.FileIndex) &&
                    CurrentFileIndex <= mfb.FileIndex)
                {
                    TransmitFileBlockEventArgs args = new TransmitFileBlockEventArgs(
                        new FileTransferPacket(Source,
                            Destination,
                            FileTransferCommandCode.ReTransmition,
                            this.TaskId,
                            0,
                            mfb.FileIndex,
                            new byte[0]));
                    OnTransmitRequest(this, args);
                }
            }
        }

        new private void mConnectionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (OnConnectionTimeout != null)
            {
                OnConnectionTimeout(this, e);
                mReceiveThread.Abort();
            }
        }


        private void InvokeOnFileTransferCompleted()
        {
            //Notify the sender that we are done
            if (OnTransmitRequest != null)
            {
                TransmitFileBlockEventArgs args = new TransmitFileBlockEventArgs(
                            new FileTransferPacket(Source,
                                Destination,
                                FileTransferCommandCode.Completed,
                                this.TaskId,
                                0,
                                0,
                                new byte[0]));
                OnTransmitRequest(this, args);
            }

            if (OnFileTransferCompleted != null)
            {
                OnFileTransferCompleted(this, new EventArgs());
            }
        }
        #endregion
    }
}
