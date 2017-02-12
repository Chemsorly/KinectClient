using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post_KNV_Client.Log
{
    /// <summary>
    /// static class that can be called from everywhere. feeds the UI with status updates
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// writes a message into the UI log
        /// </summary>
        /// <param name="message">the log message</param>
        public delegate void OnLogMessageRecieved(String message);
        public static event OnLogMessageRecieved OnLogMessageEvent;
        public static void writeLog(String pMessage)
        {
            OnLogMessageEvent(pMessage);
        }

        /// <summary>
        /// writes a debugmessage into the ui log
        /// </summary>
        /// <param name="message">the debug message</param>
        public delegate void OnLogMessageDebugRecieved(String message);
        public static event OnLogMessageDebugRecieved OnLogMessageDebugEvent;
        public static void writeLogDebug(String pMessage)
        {
            OnLogMessageDebugEvent(pMessage);
        }

        /// <summary>
        /// updates the current ui with the current data
        /// </summary>
        /// <param name="StatusArgs">the client status</param>
        public delegate void OnClientStatusChangeRecieved(Post_KNV_Client.MainWindow.ClientStatus StatusArgs);
        public static event OnClientStatusChangeRecieved OnClientStatusChangeEvent;
        public static void updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus pStatusArgs)
        {
            OnClientStatusChangeEvent(pStatusArgs);
        }

        /// <summary>
        /// updates the current ui with the kinect status
        /// </summary>
        /// <param name="StatusArgs">the kinect status</param>
        public delegate void OnKinectStatusChangeRecieved(Post_KNV_Client.MainWindow.KinectStatus StatusArgs);
        public static event OnKinectStatusChangeRecieved OnKinectStatusChangeEvent;
        public static void updateKinectStatus(Post_KNV_Client.MainWindow.KinectStatus pStatusArgs)
        {
            OnKinectStatusChangeEvent(pStatusArgs);
        }

        /// <summary>
        /// updates the current ui with the fps status
        /// </summary>
        /// <param name="pFPS">current fps</param>
        public delegate void OnKinectFPSRecieved(int pFPS);
        public static event OnKinectFPSRecieved OnKinectFPSEvent;
        public static void updateFPSStatus(int pFPS)
        {
            OnKinectFPSEvent(pFPS);
        }

    }
}
