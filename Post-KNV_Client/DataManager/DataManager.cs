using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Post_KNV_Client.DataManager
{
    class DataManager
    {
        //singleton instance of the data manager
        private static DataManager _DataManager;
        public static DataManager getInstance()
        {
            if (_DataManager == null)
                _DataManager = new DataManager();
            return _DataManager;
        }

        //webservice reference
        private Post_KNV_Client.WebService.ClientHandler _Webservice; 

        //kinect manager reference
        KinectManager.KinectHandler _KinectManager;
        public delegate void OnColorPictureReady(WriteableBitmap pColorPicture);
        public event OnColorPictureReady OnColorPictureEvent;
        public delegate void OnDepthPictureReady(WriteableBitmap pDepthPicture);
        public event OnDepthPictureReady OnDepthPictureEvent;

        /// <summary>
        /// constructor
        /// </summary>
        private DataManager()
        {
            //starts the webservice
            _Webservice = new WebService.ClientHandler();
            _Webservice.ScanRequestEvent += _Webservice_ScanRequestEvent;

            //creates a new kinect manager that starts the kinect sensor
            try
            {
                _KinectManager = new KinectManager.KinectHandler();
                _KinectManager.OnColorPictureEvent += _KinectManager_OnColorPictureEvent;
                _KinectManager.OnDepthPictureEvent += _KinectManager_OnDepthPictureEvent;
                _KinectManager.OnPackageFullEvent += _KinectManager_OnPackageFullEvent;
                _KinectManager.OnStatusChangeEvent += _KinectManager_OnStatusChangeEvent;
            }
            catch (Exception ex) { Log.LogManager.writeLog("[KinectManager] ERROR: " + ex.Message); _KinectManager = null; }
        }

        /// <summary>
        /// sends the status package generated from the kinect to the server
        /// </summary>
        /// <param name="pStatusPackage">the status package</param>
        void _KinectManager_OnStatusChangeEvent(Post_KNV_MessageClasses.KinectStatusPackage pStatusPackage)
        {
            if(Config.ConfigManager._ClientConfigObject.clientRequestObject.isConnected)
                _Webservice.sendStatusToServer(pStatusPackage);
        }

        /// <summary>
        /// saves the config
        /// </summary>
        internal void saveConfig()
        {
            _Webservice.saveConfig();
        }

        /// <summary>
        /// forwards the scan request from the webservice to the kinect manager
        /// </summary>
        /// <param name="configObject">the recieved config object that has the ClientKinectConfigObject</param>
        void _Webservice_ScanRequestEvent(Post_KNV_MessageClasses.ClientConfigObject configObject)
        {
            //if kinect manager didnt get initialized for any reason, throw error; otherwise start scan
            if(_KinectManager == null)
            {
                Log.LogManager.writeLog("[DataManager] KinectManager not initialized.");
                return;
            }
            _KinectManager.StartScan(configObject);            
        }
        
        /// <summary>
        /// gets fired when the kinect manager reports a full data package. gives it to the webservice to send it to the server
        /// </summary>
        /// <param name="pDataPackage">the data package</param>
        void _KinectManager_OnPackageFullEvent(Post_KNV_MessageClasses.KinectDataPackage pDataPackage)
        {
            _Webservice.sendDataToServer(pDataPackage);
        }

        /// <summary>
        /// forwards the depth picture from the kinect manager to the UI
        /// </summary>
        /// <param name="pDepthPicture"></param>
        void _KinectManager_OnDepthPictureEvent(WriteableBitmap pDepthPicture)
        {
            OnDepthPictureEvent(pDepthPicture);
        }

        /// <summary>
        /// forwards the color picture from the kinect manager to the ui
        /// </summary>
        /// <param name="pColorPicture"></param>
        void _KinectManager_OnColorPictureEvent(WriteableBitmap pColorPicture)
        {
            OnColorPictureEvent(pColorPicture);
        }

    }
}
