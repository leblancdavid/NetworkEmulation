using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkEmulation.Protocols
{
    public class MissingBlockQueueManager
    {
        #region Member
        private List<MissingFileBlock> mQueue;
        private double mTimeout;
        #endregion

        #region Properties
        public EventHandler<EventArgs> OnTimeout;
        #endregion

        #region Constructors
        public MissingBlockQueueManager(double timeout)
        {
            mQueue = new List<MissingFileBlock>();
            mTimeout = timeout;
        }
        #endregion 

        #region Public Methods
        public void AddFileBlock(long index)
        {
            mQueue.Add(new MissingFileBlock(null, index, mTimeout));
            mQueue.Last().OnTimeout += FileBlock_Timeout;
        }

        public void RemoveFileBlock(long index)
        {
            foreach (MissingFileBlock b in mQueue)
            {
                if (b.FileIndex == index)
                {
                    b.StopTimer();
                }
            }

            mQueue.RemoveAll(fb => fb.FileIndex == index);
        }

        public bool Contains(long index)
        {
            MissingFileBlock mfb = mQueue.Find(fb => fb.FileIndex == index);
            if (mfb == null)
                return false;
            else
                return true;
        }
        #endregion

        #region Private Methods
        private void FileBlock_Timeout(object sender, EventArgs e)
        {
            if (OnTimeout != null)
            {
                OnTimeout.Invoke(sender, new EventArgs());
                ((MissingFileBlock)sender).ResetTimer();
                //mQueue.Remove((MissingFileBlock)sender);
            }
        }
        #endregion
    }
}
