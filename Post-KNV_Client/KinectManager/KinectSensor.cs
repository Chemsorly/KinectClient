using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows;
using System.Timers;

namespace Post_KNV_Client.KinectManager
{
    class KinectSensor
    {
        //the kinect sensor
        Microsoft.Kinect.KinectSensor _KinectSensor;

        //the frame readers
        DepthFrameReader _DepthFrameReader;
        ColorFrameReader _ColorFrameReader;

        //checking variables to ensure only one frame is processed at once
        bool ColorFrameProcessing = false;
        bool DepthFrameProcessing = false;

        //the fps timer
        KinectFPStimer _FPStimer;
        Timer updateTimer;

        #region external

        //gets fired whenever a color picture is ready
        internal delegate void OnColorPictureReady(WriteableBitmap pColorPicture);
        internal event OnColorPictureReady OnColorPictureEvent;

        //gets fired whenever a depth picture is ready
        internal delegate void OnDepthPictureReady(WriteableBitmap pDepthPicture);
        internal event OnDepthPictureReady OnDepthPictureEvent;

        //gets fired whenever depth data is ready
        internal delegate void OnDepthDataReady(ushort[] pDepthData);
        internal event OnDepthDataReady OnDepthDataEvent;

        //gets fired whenever the availability statuc changes
        internal delegate void OnAvailabilityStatusChanged(bool pStatus);
        internal event OnAvailabilityStatusChanged OnAvailabilityStatusChangedEvent;

        /// <summary>
        /// constructor
        /// </summary>
        internal KinectSensor()
        {
            _KinectSensor = Microsoft.Kinect.KinectSensor.GetDefault();
            _KinectSensor.IsAvailableChanged += _KinectSensor_IsAvailableChanged;

            updateTimer = new Timer(10000);
            updateTimer.AutoReset = true;
            updateTimer.Elapsed += updateTimer_Elapsed;

            _FPStimer = new KinectFPStimer();
            _FPStimer.FpsTimerReady += _FPStimer_FpsTimerReady;

            initialize();
        }

        /// <summary>
        /// throws a update in case of no update happened for a while
        /// </summary>
        void updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnAvailabilityStatusChangedEvent(this._KinectSensor.IsAvailable);
        }

        #endregion

        #region internal

        /// <summary>
        /// starts the kinect sensor
        /// </summary>
        void initialize()
        {
            this._ColorFrameReader = this._KinectSensor.ColorFrameSource.OpenReader();
            this._ColorFrameReader.FrameArrived += _ColorFrameReader_FrameArrived;

            this._DepthFrameReader = this._KinectSensor.DepthFrameSource.OpenReader();
            this._DepthFrameReader.FrameArrived += _DepthFrameReader_FrameArrived;
            _KinectSensor.Open();
        }

        /// <summary>
        /// gets fired when a new FPS count is ready. updates the log
        /// </summary>
        /// <param name="fps">the fps count</param>
        void _FPStimer_FpsTimerReady(int fps)
        {
            Log.LogManager.updateFPSStatus(fps);
        }

        /// <summary>
        /// gets fired whenever the kinect status changes
        /// </summary>
        /// <param name="sender">sender object(kinect sensor)</param>
        /// <param name="e">new status value</param>
        void _KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.updateTimer.Stop();
            this.updateTimer.Start();
            OnAvailabilityStatusChangedEvent(e.IsAvailable);
        }
                
        /// <summary>
        /// gets fired when the kinect sensor has a new depth frame
        /// </summary>
        /// <param name="sender">the kinect</param>
        /// <param name="e">the depth frame</param>
        void _DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            if (DepthFrameProcessing) return;
            DepthFrameProcessing = true;

            Task t = new Task(() => CalculateDepthPicture(e));
            t.ContinueWith((continuation) =>
                {
                    DepthFrameProcessing = false;
                    _FPStimer.increaseProcessedFrameCount();                    
                });
            t.Start();
        }

        /// <summary>
        /// gets fired when the kinect sensor has a new color frame
        /// </summary>
        /// <param name="sender">the kinect</param>
        /// <param name="e">the color frame</param>
        void _ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            if (ColorFrameProcessing) return; // || !Config.ConfigManager._ClientConfigObject.clientKinectConfig.calculateColor) return;
            ColorFrameProcessing = true;

            Task t = new Task(() => CalculateColorPicture(e));
            t.ContinueWith((continuation) => ColorFrameProcessing = false);         
            t.Start();
        }

        /// <summary>
        /// creates a color picture from the colorframe data package and broadcasts it
        /// </summary>
        /// <param name="e">the colorframe data package</param>
        void CalculateColorPicture(ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame cf = e.FrameReference.AcquireFrame())
            {
                if (cf != null)
                {
                    byte[] colorPixels = new byte[cf.FrameDescription.Width * cf.FrameDescription.Height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];
                    WriteableBitmap colorBitmap = new WriteableBitmap(
                        cf.FrameDescription.Width,
                        cf.FrameDescription.Height,
                        96.0, 96.0, PixelFormats.Bgr32, null);

                    if ((cf.FrameDescription.Width == colorBitmap.PixelWidth) && (cf.FrameDescription.Height == colorBitmap.PixelHeight))
                    {
                        if (cf.RawColorImageFormat == ColorImageFormat.Bgra)
                        {
                            cf.CopyRawFrameDataToArray(colorPixels);
                        }
                        else
                        {
                            cf.CopyConvertedFrameDataToArray(colorPixels, ColorImageFormat.Bgra);
                        }

                        colorBitmap.WritePixels(
                            new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
                            colorPixels,
                            colorBitmap.PixelWidth * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8),
                            0);
                        colorBitmap.Freeze();
                        OnColorPictureEvent.BeginInvoke(colorBitmap, null, null);
                    }

                }
            }
        }

        /// <summary>
        /// creates a depth picture from the depthframe data package and broadcasts it
        /// </summary>
        /// <param name="e">the depthframe data package</param>
        void CalculateDepthPicture(DepthFrameArrivedEventArgs e)
        {
            using (DepthFrame df = e.FrameReference.AcquireFrame())
            {
                if (df != null)
                {
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = df.LockImageBuffer())
                    {
                        WriteableBitmap depthBitmap = new WriteableBitmap(df.FrameDescription.Width, df.FrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

                        if (((df.FrameDescription.Width * df.FrameDescription.Height) == (depthBuffer.Size / df.FrameDescription.BytesPerPixel)) &&
                            (df.FrameDescription.Width == depthBitmap.PixelWidth) && (df.FrameDescription.Height == depthBitmap.PixelHeight))
                        {
                            depthReturnStruc dd = ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, df);
                            byte[] depthPixels = dd.pictureData;
                            depthBitmap.WritePixels(
                                new Int32Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight),
                                depthPixels,
                                depthBitmap.PixelWidth,
                                0);

                            depthBitmap.Freeze();
                            OnDepthPictureEvent.BeginInvoke(depthBitmap, null, null);
                            OnDepthDataEvent.BeginInvoke(dd.depthData, null, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// support function for the depth picture calculation
        /// </summary>
        unsafe depthReturnStruc ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, DepthFrame df)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;
            ushort minDepth = Config.ConfigManager._ClientConfigObject.clientKinectConfig.minDepth;
            ushort maxDepth = Config.ConfigManager._ClientConfigObject.clientKinectConfig.maxDepth;
            int width = df.FrameDescription.Width;

            byte[] depthPixels = new byte[width * df.FrameDescription.Height];
            ushort[] depthData = new ushort[width * df.FrameDescription.Height];

            // convert depth to a visual representation
            Parallel.For(0, (int)(depthFrameDataSize / df.FrameDescription.BytesPerPixel), i =>
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];
                int x = (int)(i % width);
                int y = (int)(i / width);

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                // Values outside the reliable depth range are mapped to 0 (black).                
                depthPixels[i] = (byte)(depth >= minDepth &&
                    depth <= maxDepth &&
                    x >= Config.ConfigManager._ClientConfigObject.clientKinectConfig.xMinDepth &&
                    x <= Config.ConfigManager._ClientConfigObject.clientKinectConfig.xMaxDepth &&
                    y >= Config.ConfigManager._ClientConfigObject.clientKinectConfig.yMinDepth &&
                    y <= Config.ConfigManager._ClientConfigObject.clientKinectConfig.yMaxDepth 
                    ? (depth / (maxDepth / 256)) : 0);
                depthData[i] = depth;
            });


            return new depthReturnStruc { pictureData = depthPixels, depthData = depthData };
        }
        struct depthReturnStruc { internal byte[] pictureData; internal ushort[] depthData;}

        #endregion
    }
}
