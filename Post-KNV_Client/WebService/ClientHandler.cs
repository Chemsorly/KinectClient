using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Post_KNV_Client.RegistrationService;
using System.Timers;
using Post_KNV_Client.Config;
using Post_KNV_MessageClasses;
using Post_KNV_Client.Log;

namespace Post_KNV_Client.WebService
{
    /// <summary>
    /// handles the subcomponents and incoming/outgoing messages
    /// </summary>
    class ClientHandler
    {
        //the listener
        private ClientListener _WebserviceListener;

        //the sender
        private ClientSender _WebserviceSender;

        //the registration service
        private RegService _RegistrationService;

        #region external

        //event that gets fired, when a SCAN request from the listener occured; does NOT change the current config
        public delegate void ScanRequestRecieved(ClientConfigObject configObject);
        public event ScanRequestRecieved ScanRequestEvent;

        /// <summary>
        /// constructor; initializes subcomponents
        /// </summary>
        public ClientHandler()
        {
            _WebserviceSender = new ClientSender();
            _WebserviceSender.OnHelloSuccessfullEvent += _WebserviceSender_OnHelloSuccessfullEvent;
            _WebserviceSender.OnConfigSuccessfullEvent += _WebserviceSender_OnConfigSuccessfullEvent;
            _WebserviceSender.OnErrorEvent += ErrorMessageHandler;

            _RegistrationService = new RegService();
            _RegistrationService.RequestHelloEvent += _RegistrationService_RequestHelloEvent;
            _RegistrationService.initialize();

            _WebserviceListener = new ClientListener();
            ClientDefinition.OnConfigRequestEvent += ClientDefinition_OnConfigRequestEvent;
            ClientDefinition.ErrorMessageEvent += ErrorMessageHandler;
            ClientDefinition.OnScanRequestEvent += ClientDefinition_OnScanRequestEvent;
            ClientDefinition.OnPingEvent += ClientDefinition_OnPingEvent;
            ClientDefinition.OnShutdownRequestEvent += ClientDefinition_OnShutdownRequestEvent;
            _WebserviceListener.initialize(ConfigManager._ClientConfigObject.clientConnectionConfig.listeningPort);

            LogManager.writeLog("[Webservice] Webservice fully initialized");
        }

        /// <summary>
        /// saves the config, both internally and on the server (if connected)
        /// </summary>
        internal void saveConfig()
        {
            _RegistrationService.recieveConfiguration(ConfigManager._ClientConfigObject);
            if (ConfigManager._ClientConfigObject.clientRequestObject.isConnected) 
                if(ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway != String.Empty)
                    _WebserviceSender.startConfigRequest(ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway,
                    ConfigManager._ClientConfigObject);
                else
                    _WebserviceSender.startConfigRequest(ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP,
                    ConfigManager._ClientConfigObject);
        }

        /// <summary>
        /// sends the kinect data package to the server
        /// </summary>
        /// <param name="pKinectDataPackage">the kinect data package</param>
        internal void sendDataToServer(KinectDataPackage pKinectDataPackage)
        {
            if (ConfigManager._ClientConfigObject.clientRequestObject.isConnected)
            {
                if (pKinectDataPackage.usedConfig.clientConnectionConfig.targetGateway != String.Empty)
                    _WebserviceSender.startDataRequest(pKinectDataPackage.usedConfig.clientConnectionConfig.targetGateway, pKinectDataPackage);
                else
                    _WebserviceSender.startDataRequest(pKinectDataPackage.usedConfig.clientConnectionConfig.targetIP, pKinectDataPackage);
            }
        }

        /// <summary>
        /// sends a status report of the kinect to the server
        /// </summary>
        /// <param name="pKinectStatusPackage">the status package</param>
        internal void sendStatusToServer(KinectStatusPackage pKinectStatusPackage)
        {
            if (ConfigManager._ClientConfigObject.clientRequestObject.isConnected)
            {
                if (Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway != String.Empty)
                    _WebserviceSender.startStatusRequest(Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway, pKinectStatusPackage);
                else
                    _WebserviceSender.startStatusRequest(Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP, pKinectStatusPackage);
            }
        }

        #endregion

        #region internal

        /// <summary>
        /// checks if the client is online/offline and then either starts or stops it
        /// </summary>
        void ClientDefinition_OnShutdownRequestEvent()
        {
            if (ConfigManager._ClientConfigObject.clientRequestObject.isConnected)
            {
                ConfigManager._ClientConfigObject.clientRequestObject.isConnected = false;
                _RegistrationService._timeoutTimer.Stop();
                LogManager.updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus.disconnected);
                LogManager.writeLog("[Webservice] Shutdown signal recieved");
            }
            else if (!ConfigManager._ClientConfigObject.clientRequestObject.isConnected)
            {
                _RegistrationService._timeoutTimer.Start();
                LogManager.writeLog("[Webservice] Shutdown signal recieved. Client will reconnect soon.");
            }
        }

        /// <summary>
        /// gets fired when the registration service requests a HELLO to the server (e.g. after a timeout)
        /// </summary>
        /// <param name="pHello"></param>
        void _RegistrationService_RequestHelloEvent(HelloRequestObject pHello)
        {
            //send hello
            if (Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway != String.Empty)
            {
                LogManager.writeLog("[Webservice] Sending HELLO to " + ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway);
                _WebserviceSender.startHelloRequest(ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway, pHello);
            }
            else
            {
                LogManager.writeLog("[Webservice] Sending HELLO to " + ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP);
                _WebserviceSender.startHelloRequest(ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP, pHello);
            }
        }

        /// <summary>
        /// gets fired when the webservice listener recieves the PING event
        /// </summary>
        void ClientDefinition_OnPingEvent()
        {
            _RegistrationService.OnPingRecieved();
            LogManager.writeLogDebug("[Webservice] PING recieved");
        }

        /// <summary>
        /// gets fired when the configuration change went through the server
        /// </summary>
        /// <param name="response">the response from the server</param>
        void _WebserviceSender_OnConfigSuccessfullEvent(string response)
        {
            LogManager.writeLog("[Webservice] Configuration sent to server");
        }

        /// <summary>
        /// gets fired when the webservice sender recieves the HELLO response
        /// </summary>
        /// <param name="response">the helloresponseobject</param>
        void _WebserviceSender_OnHelloSuccessfullEvent(String response)
        {
            LogManager.writeLog("[Webservice] HELLO successful");
            LogManager.updateClientStatus(Post_KNV_Client.MainWindow.ClientStatus.connected);
            ConfigManager._ClientConfigObject.clientRequestObject.isConnected = true;
        }
        
        /// <summary>
        /// gets fired when a SCAN request occured; ASYNC
        /// </summary>
        /// <param name="configObject">the config object which then will be used in the scan</param>
        void ClientDefinition_OnScanRequestEvent(Post_KNV_MessageClasses.ClientConfigObject configObject)
        {
            LogManager.writeLog("[Webservice] SCAN request recieved");
            ScanRequestEvent.BeginInvoke(configObject, null, null);
        }
        
        /// <summary>
        /// gets fired when an exception occurs
        /// </summary>
        /// <param name="e">the exception</param>
        void ErrorMessageHandler(Exception e)
        {
            //ErrorMessageEvent(e);
            LogManager.writeLogDebug("[Webservice] ERROR: " + e.Message);
        }

        /// <summary>
        /// gets fired when a CONFIG request occured
        /// </summary>
        /// <param name="pConfig">the config file</param>
        void ClientDefinition_OnConfigRequestEvent(Post_KNV_MessageClasses.ClientConfigObject pConfig)
        {
            _RegistrationService.recieveConfiguration(pConfig);
            //ConfigRequestEvent();
            LogManager.writeLog("[Webservice] Configuration recieved");
        }

        #endregion
    }
}
