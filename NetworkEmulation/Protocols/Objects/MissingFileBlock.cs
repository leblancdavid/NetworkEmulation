using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkEmulation.Protocols
{
    public class MissingFileBlock : FileBlock
    {
        #region Members
        
        private double DEFAULT_WINDOW_TIMEOUT = 10000; //default 10sec
        private double time;
        //AutoResetEvent autoEvent = new AutoResetEvent(false);
        //private System.Threading.Timer mTimeout;

        private System.Timers.Timer mTimeout;
        #endregion

        #region Properties
        public EventHandler<EventArgs> OnTimeout;
        #endregion

        #region Constructor
        public MissingFileBlock()
        {
            Data = new byte[0];
            FileIndex = 0;
            Timestamp = DateTime.Now;
            time = DEFAULT_WINDOW_TIMEOUT;
            //mTimeout = new System.Threading.Timer(new System.Threading.TimerCallback(mTimeout_Elapsed), autoEvent, (int)time, (int)(time / 5.0));
            mTimeout = new System.Timers.Timer();
            mTimeout.Interval = time;
            mTimeout.Elapsed += mTimeout_Elapsed;
            mTimeout.Enabled = true;
            mTimeout.Start();
        }

        public MissingFileBlock(byte[] pData, long pFileIndex, double pTimeout)
        {
            Data = pData;
            FileIndex = pFileIndex;
            Timestamp = DateTime.Now;

            time = pTimeout;
            //mTimeout = new System.Threading.Timer(new System.Threading.TimerCallback(mTimeout_Elapsed), autoEvent, (int)time, (int)(time / 5.0));
            mTimeout = new System.Timers.Timer(time);
            mTimeout.Interval = time / 5.0;
            mTimeout.Elapsed += mTimeout_Elapsed;
            mTimeout.Enabled = true;
            mTimeout.Start();
        }

        public void ResetTimer()
        {
            //mTimeout.Change((int)time, (int)(time / 5.0));
            if (mTimeout != null)
            {
                mTimeout.Stop();
                mTimeout.Start();
            }
        }

        public void StopTimer()
        {
            //mTimeout.Change(Timeout.Infinite, Timeout.Infinite);
            mTimeout.Elapsed -= mTimeout_Elapsed;
            mTimeout.Stop();
            mTimeout = null;
        }

        #endregion

        #region Private Members

        //void mTimeout_Elapsed(object sender)
        //{
        //    if (OnTimeout != null)
        //    {
        //        OnTimeout.Invoke(this, new EventArgs());
        //    }
        //}

        void mTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (OnTimeout != null)
            {
                OnTimeout.Invoke(this, new EventArgs());
            }
        }

        #endregion
    }
}
