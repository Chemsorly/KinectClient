using Post_KNV_Client.Log;
using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Post_KNV_Client.WebService
{
    public class ClientSender
    {

        #region external
        bool helloWorking = false;

        /// <summary>
        /// constructor
        /// </summary>
        public ClientSender()
        {
            LogManager.writeLog("[Webservice] Sender initialized");
        }

        /// <summary>
        /// starts a hello request to the target IP with the HRO object
        /// </summary>
        /// <param name="pTargetIP">the target server</param>
        /// <param name="pHelloObject">data package containing the necessary information</param>
        public void startHelloRequest(String pTargetIP, HelloRequestObject pHelloObject)
        {
            //send data
            if (helloWorking) return;
            helloWorking = true;
            Task<String> t = new Task<String>(() => sendRequestThread(@"http://" + pTargetIP + @"/HELLO", pHelloObject,10000));
            t.ContinueWith(TaskFaultedHandler, TaskContinuationOptions.OnlyOnFaulted);
            t.ContinueWith(HelloRequestSuccessfulHandler, TaskContinuationOptions.OnlyOnRanToCompletion);
            t.ContinueWith(delegate { helloWorking = false; });
            t.Start();
        }

        /// <summary>
        /// starts a config request to change the configuration on the server
        /// </summary>
        /// <param name="clientConfigObject">the config object that is supposed to be written on the server</param>
        public void startConfigRequest(String pTargetIP, ClientConfigObject clientConfigObject)
        {
            //send data
            Task<String> t = new Task<String>(() => sendRequestThread(@"http://" + pTargetIP + @"/CONFIG", clientConfigObject, 10000));
            t.ContinueWith(TaskFaultedHandler, TaskContinuationOptions.OnlyOnFaulted);
            t.ContinueWith(ConfigRequestSuccessfulHandler, TaskContinuationOptions.OnlyOnRanToCompletion);
            t.Start();
        }

        /// <summary>
        /// starts a data request to the server
        /// </summary>
        /// <param name="pDataPackage">the data package</param>
        public void startDataRequest(String pTargetIP, KinectDataPackage pDataPackage)
        {
            Task<String> t = new Task<String>(() => sendRequestThread(@"http://" +
                pTargetIP + @"/KINECTDATA",
                pDataPackage,300000));
            t.ContinueWith(TaskFaultedHandler, TaskContinuationOptions.OnlyOnFaulted);
            //t.ContinueWith(ConfigRequestSuccessfulHandler, TaskContinuationOptions.OnlyOnRanToCompletion);
            t.Start();            
        }

        /// <summary>
        /// sets a status report to the server
        /// </summary>
        /// <param name="pTargetIP">the target ip</param>
        /// <param name="pStatusPackage">the status package</param>
        public void startStatusRequest(String pTargetIP, KinectStatusPackage pStatusPackage)
        {
            Task<String> t = new Task<string>(() => sendRequestThread(@"http://" + pTargetIP + @"/KINECTSTATUS", pStatusPackage, 5000));
            t.ContinueWith(TaskFaultedHandler, TaskContinuationOptions.OnlyOnFaulted);
            t.Start();
        }

        /// <summary>
        /// starts an apprequest towards the server
        /// </summary>
        /// <param name="pTargetIP">target ip</param>
        /// <param name="pAppRequestObject">app request object</param>
        public String startAppRequest(String pTargetIP, AppRequestObject pAppRequestObject)
        {
            Task<String> t = new Task<string>(() => sendRequestThreadARO(@"http://" + pTargetIP + @"/APPREQUEST", pAppRequestObject, 5000));
            t.ContinueWith(TaskFaultedHandler, TaskContinuationOptions.OnlyOnFaulted);
            t.Start();

            t.Wait();
            if (t.IsCompleted)
                return t.Result;
            else
                return t.Exception.InnerException.Message;           
        }

        #endregion

        #region task handlers
        public delegate void OnConfigRequestSuccessfullSent(String response);
        public event OnConfigRequestSuccessfullSent OnConfigSuccessfullEvent;
        /// <summary>
        /// task handler that gets thrown when a config request has been successful
        /// </summary>
        /// <param name="pTask">the task of the successful config request</param>
        private void ConfigRequestSuccessfulHandler(Task<String> pTask)
        {
            OnConfigSuccessfullEvent(pTask.Result);
        }

        public delegate void OnHelloSuccessfullSent(String response);
        public event OnHelloSuccessfullSent OnHelloSuccessfullEvent;
        /// <summary>
        /// task handler to broadcast a successfull hello request event
        /// </summary>
        /// <param name="pTask">the task that returns the successful hello request from the sender</param>
        private void HelloRequestSuccessfulHandler(Task<String> pTask)
        {
            OnHelloSuccessfullEvent(pTask.Result);
        }


        public delegate void OnErrorRecieved(Exception pEx);
        public event OnErrorRecieved OnErrorEvent;
        /// <summary>
        /// task handler to deal with errors
        /// </summary>
        /// <param name="pTask">the task that threw the error</param>
        private void TaskFaultedHandler(Task pTask)
        {
            OnErrorEvent(pTask.Exception.InnerException);
        }

        #endregion

        #region internal

        /// <summary>
        /// sends a HTTP request to the target IP using POST
        /// </summary>
        /// <param name="targetIP">the target IP</param>
        /// <param name="input">the message that is supposed to be send</param>
        /// <returns>the response from the server</returns>
        String sendRequestThread(String targetIP, Object input, int timeout)
        {
            try
            {
                WebRequest request = WebRequest.Create(targetIP);
                request.Timeout = timeout; //ToDo: dynamisch/abhängig von PING/HELLO interval
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(requestStream, input);
                requestStream.Close();

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                return reader.ReadToEnd();
            }
            catch (Exception ex) { throw; }
        }

        String sendRequestThreadARO(String pTargetIP,AppRequestObject pAro, int timeout)
        {
            try
            {
                WebRequest request = WebRequest.Create(pTargetIP);
                request.Method = "POST";
                request.Timeout = timeout;

                XmlSerializer serializer = new XmlSerializer(typeof(AppRequestObject));
                Stream requestStream = request.GetRequestStream();
                serializer.Serialize(requestStream, pAro);
                requestStream.Close();

                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                String s = reader.ReadToEnd();

                requestStream.Close();
                return s;
            }
            catch (Exception ex) { throw; }
        }

        #endregion
    }
}
