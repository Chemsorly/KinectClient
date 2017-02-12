using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Serialization;

namespace Post_KNV_Client.WebService
{
    public class ClientDefinition : WebserviceClientContract
    {
        //Error event
        public delegate void ErrorMessage(Exception e);
        public static event ErrorMessage ErrorMessageEvent;

        //ping event
        public delegate void OnPingRecieved();
        public static event OnPingRecieved OnPingEvent;

        /// <summary>
        /// response function for the PING event
        /// </summary>
        /// <returns>PONG</returns>
        public String responsePong()
        {
            OnPingEvent.BeginInvoke(null,null);
            return "PONG";
        }

        public delegate void OnScanRequestRecieved(ClientConfigObject configObject);
        public static event OnScanRequestRecieved OnScanRequestEvent;
        /// <summary>
        /// response function for the SCAN event
        /// </summary>
        /// <param name="message">parameters for scanning</param>
        public String responseScan(Stream message)
        {
            //convert message xml stream to object 
            ClientConfigObject recievedConfig;
            try 
            {
                BinaryFormatter formatter = new BinaryFormatter();
                recievedConfig = (ClientConfigObject)formatter.Deserialize(message);
            }
            catch (Exception ex)
            {
                ErrorMessageEvent(ex);
                return ex.Message;
            }

            //broadcast scan event
            OnScanRequestEvent(recievedConfig);
            return "SCAN REQUEST RECIEVED";
        }

        public delegate void OnConfigRequestRecieved(ClientConfigObject pConfig);
        public static event OnConfigRequestRecieved OnConfigRequestEvent;
        /// <summary>
        /// response function for CONFIG event
        /// </summary>
        /// <param name="message">config parameters; xml of KinectClientObject</param>
        public String responseConfig(Stream message)
        {
            //convert message xml stream to object
            ClientConfigObject recievedConfig;
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                recievedConfig = (ClientConfigObject)formatter.Deserialize(message);
            }
            catch (Exception ex)
            {                
                ErrorMessageEvent(ex);
                return ex.Message;
            }

            //broadcast config event
            OnConfigRequestEvent(recievedConfig);
            return "CONFIG REQUEST RECIEVED";
        }

        //shutdown event
        public delegate void OnShutdownRequestRecieved();
        public static event OnShutdownRequestRecieved OnShutdownRequestEvent;
        /// <summary>
        /// response function for SHUTDOWN event
        /// </summary>
        /// <returns>OK</returns>
        public String responseShutdown()
        {
            OnShutdownRequestEvent.BeginInvoke(null, null);
            return "OK";
        }
    }

    /// <summary>
    /// the webservice contract that defines how to handle the incoming messages
    /// </summary>
    [ServiceContract]
    public interface WebserviceClientContract
    {
        //in case of PING event; returns pong
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "PING")]
        String responsePong();

        //in case of SCAN event; starts scan process
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "SCAN")]
        String responseScan(Stream message);

        //in case of CONFIG event; requests config change
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "CONFIG")]
        String responseConfig(Stream message);

        //in case of SHUTDOWN event; requests client shutdown
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "SHUTDOWN")]
        String responseShutdown();

    }
}
