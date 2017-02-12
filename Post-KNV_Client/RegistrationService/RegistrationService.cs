using Post_KNV_Client.Config;
using Post_KNV_Client.Log;
using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Post_KNV_Client.RegistrationService
{
    /// <summary>
    /// class that manages registering with the server and keeps the connection alive
    /// </summary>
    public class RegService
    {
        //the configuration manager
        ConfigManager _ConfigManager;

        //variable to check if the client is connected or not

        //timer for hello checking
        internal Timer _timeoutTimer;
        int timerRounds = 0;

        #region external

        /// <summary>
        /// hello request event
        /// </summary>
        /// <param name="pHello">the hello request object</param>
        public delegate void RequestHello(HelloRequestObject pHello);
        public event RequestHello RequestHelloEvent;

        /// <summary>
        /// initializes the RegistrationServer
        /// </summary>
        internal void initialize()
        {
            _ConfigManager = new ConfigManager();

            //timer registration
            _timeoutTimer = new Timer(ConfigManager._ClientConfigObject.clientConnectionConfig.keepAliveInterval + 1000);
            _timeoutTimer.Elapsed += intervalTimer_Elapsed;
            _timeoutTimer.AutoReset = true;
            _timeoutTimer.Start();

            LogManager.updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus.disconnected);
            RequestHelloEvent(HelloRequestObject.createHelloFromClientConfig(ConfigManager._ClientConfigObject));
        }

        /// <summary>
        /// every time the RegistrationService recieved a PING, the time gets reset
        /// </summary>
        internal void OnPingRecieved()
        {
            timerRounds = 0;
            _timeoutTimer.Stop();
            _timeoutTimer.Start();
            ConfigManager._ClientConfigObject.clientRequestObject.isConnected = true;
            LogManager.updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus.connected);
        }

        /// <summary>
        /// changes the current ClientConfiguration
        /// </summary>
        /// <param name="pConfigData">the configuration object</param>
        internal void recieveConfiguration(ClientConfigObject pConfigData)
        {
            _ConfigManager.writeConfig(pConfigData);
            _timeoutTimer.Interval = pConfigData.clientConnectionConfig.keepAliveInterval + 1000;
            LogManager.writeLog("[RegistrationService] Configuration changed");
        }

        #endregion

        /// <summary>
        /// gets thrown every time the keepAliveInterval hits
        /// </summary>
        void intervalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ConfigManager._ClientConfigObject.clientRequestObject.isConnected) LogManager.updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus.waiting_for_ping);
            if (timerRounds < 2)
            {
                timerRounds++;
                return;
            }
            
            timerRounds = 0;
            LogManager.writeLog("[RegistrationService] TIMEOUT: Requesting new HELLO");
            ConfigManager._ClientConfigObject.clientRequestObject.isConnected = false;
            LogManager.updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus.disconnected);
            RequestHelloEvent(HelloRequestObject.createHelloFromClientConfig(ConfigManager._ClientConfigObject));
        }

    }
}
