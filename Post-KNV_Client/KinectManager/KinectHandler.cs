using Microsoft.Kinect;
using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Post_KNV_Client.KinectManager
{
    /// <summary>
    /// class that handles everything kinect related
    /// </summary>
    public class KinectHandler
    {
        //the kinect sensor
        KinectSensor _KinectSensor;

        //the data collector
        KinectDataCollector _KinectDataCollector;

        //DEBUG: last recieved color bitmap
        WriteableBitmap lastColorBitmap;

        #region external

        /// <summary>
        /// gets fired whenever a color picture is ready
        /// </summary>
        /// <param name="pColorPicture">the color picture</param>
        public delegate void OnColorPictureReady(WriteableBitmap pColorPicture);
        public event OnColorPictureReady OnColorPictureEvent;

        /// <summary>
        /// gets fired whenever a depth picture is ready
        /// </summary>
        /// <param name="pDepthPicture">the depth picture</param>
        public delegate void OnDepthPictureReady(WriteableBitmap pDepthPicture);
        public event OnDepthPictureReady OnDepthPictureEvent;

        /// <summary>
        /// gets fired whenever a data package is ready
        /// </summary>
        /// <param name="pDataPackage">the data package</param>
        internal delegate void OnPackageFullEventReady(Post_KNV_MessageClasses.KinectDataPackage pDataPackage);
        internal event OnPackageFullEventReady OnPackageFullEvent;

        /// <summary>
        /// gets fired whenever a status change occured
        /// </summary>
        /// <param name="pDataPackage">the status package</param>
        internal delegate void OnStatusChangeEventReady(KinectStatusPackage pStatusPackage);
        internal event OnStatusChangeEventReady OnStatusChangeEvent;

        /// <summary>
        /// constructor, initialized all the subclasses
        /// </summary>
        public KinectHandler()
        {
            try
            {
                _KinectSensor = new KinectSensor();
                _KinectSensor.OnColorPictureEvent += _KinectSensor_OnColorPictureEvent;
                _KinectSensor.OnDepthDataEvent += _KinectSensor_OnDepthDataEvent;
                _KinectSensor.OnDepthPictureEvent += _KinectSensor_OnDepthPictureEvent;
                _KinectSensor.OnAvailabilityStatusChangedEvent += _KinectSensor_OnAvailabilityStatusChangedEvent;

                _KinectDataCollector = new KinectDataCollector();
                _KinectDataCollector.OnPackageFullEvent += _KinectDataCollector_OnPackageFullEvent;

                Log.LogManager.writeLog("[KinectManager] Manager successfully initialized");
            }catch(Exception ex)
            { Log.LogManager.writeLog("[KinectManager] ERROR: " + ex.Message); }
        }

        /// <summary>
        /// starts a scan with the incoming ClientConfigObject
        /// </summary>
        /// <param name="clientConfigObject">the config object</param>
        public void StartScan(Post_KNV_MessageClasses.ClientConfigObject clientConfigObject)
        {
            _KinectDataCollector.initializeDataPackage(clientConfigObject);
        }

        #endregion

        #region internal

        /// <summary>
        /// gets fired when the data collector has a full data package
        /// </summary>
        /// <param name="pDataPackage">the data package</param>
        void _KinectDataCollector_OnPackageFullEvent(Post_KNV_MessageClasses.KinectDataPackage pDataPackage)
        {
            if (lastColorBitmap != null)
            {
                exportScreenshot(lastColorBitmap);
                Log.LogManager.writeLog("[KinectManager] Color screenshot exported");
            }
            OnPackageFullEvent(pDataPackage);
            Log.LogManager.writeLog("[KinectManager] KinectDataPackage successfully created");
        }

        /// <summary>
        /// gets fired whenever the kinect availability status changes
        /// </summary>
        /// <param name="pStatus">new status</param>
        void _KinectSensor_OnAvailabilityStatusChangedEvent(bool pStatus)
        {
            if (pStatus)
                Log.LogManager.updateKinectStatus(MainWindow.KinectStatus.connected);
            else
                Log.LogManager.updateKinectStatus(MainWindow.KinectStatus.not_connected);

            OnStatusChangeEvent(new KinectStatusPackage
            {
                clientID = Config.ConfigManager._ClientConfigObject.ID,
                isKinectActive = pStatus
            });
        }

        /// <summary>
        /// gets fired whenever a color picture is ready
        /// </summary>
        /// <param name="pColorPicture"></param>
        void _KinectSensor_OnColorPictureEvent(WriteableBitmap pColorPicture)
        {
            lastColorBitmap = pColorPicture;
            OnColorPictureEvent(pColorPicture);
        }

        /// <summary>
        /// gets fired whenever a depth picture is ready
        /// </summary>
        /// <param name="pDepthPicture">the depth picture</param>
        void _KinectSensor_OnDepthPictureEvent(WriteableBitmap pDepthPicture)
        {
            OnDepthPictureEvent(pDepthPicture);
        }

        /// <summary>
        /// gets fired when the kinect has depth data available, forwards it to the data collector
        /// </summary>
        /// <param name="pDepthData">the depth data</param>
        void _KinectSensor_OnDepthDataEvent(ushort[] pDepthData)
        {
            _KinectDataCollector.addPictureToDataPackage(pDepthData);
        }

        /// <summary>
        /// tries to save a screenshot from a recently saved mesh
        /// </summary>
        /// <param name="pkdp">the data package containing the screenshot</param>
        /// <param name="pFileName">the filename</param>
        static void exportScreenshot(WriteableBitmap pColorPicture)
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\KinectSnapshots\\";
            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = System.IO.Path.Combine(dir, "KinectSnapshot Color-" + time + ".png");

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(pColorPicture));
            FileStream _file = new FileStream(path, FileMode.Create);
            try
            {
                encoder.Save(_file);
            }
            catch (IOException) { }
            _file.Close();
        }

        #endregion
    }
}
