using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Post_KNV_Client.KinectManager
{
    /// <summary>
    /// manages the fps count for the kinect sensor
    /// </summary>
    class KinectFPStimer
    {
        //the amount of processed frames in the current interval
        private int processedFrameCount;

        //the last fps timestamp
        private DateTime lastFPStimestamp;

        //the timer instance to do the ticks
        private Timer timer;
        
        /// <summary>
        /// gets fired when a new fps value is ready
        /// </summary>
        /// <param name="fps">the fps value</param>
        public delegate void FpsTimerEventHandler(int fps);
        public event FpsTimerEventHandler FpsTimerReady;

        /// <summary>
        /// constructor
        /// </summary>
        public KinectFPStimer()
        {
            this.lastFPStimestamp = DateTime.UtcNow;

            //init
            this.timer = new Timer();
            this.timer.Elapsed += timer_Elapsed;
            this.timer.AutoReset = true;
            this.timer.Interval = 1000;
            this.timer.Start();
        }

        /// <summary>
        /// increases the current FPS count
        /// </summary>
        public void increaseProcessedFrameCount()
        {
            processedFrameCount++;
        }

        /// <summary>
        /// gets fired when the interval elapsed, updates the fps count
        /// </summary>
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            double intervalSeconds = (DateTime.UtcNow - this.lastFPStimestamp).TotalSeconds;
            int fps = ((int)((double)this.processedFrameCount / intervalSeconds));
            processedFrameCount = 0;
            lastFPStimestamp = DateTime.UtcNow;
            this.FpsTimerReady(fps);
        }


    }
}
