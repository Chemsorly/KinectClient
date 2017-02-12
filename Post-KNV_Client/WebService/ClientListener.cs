using Post_KNV_Client.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Post_KNV_Client.WebService
{
    /// <summary>
    /// class that listens to incoming messages
    /// </summary>
    public class ClientListener
    {
        //the webservice host
        WebServiceHost _WebServiceHost;

        /// <summary>
        /// initializes the webservicehost on the target port
        /// </summary>
        /// <param name="pPort">the port</param>
        public void initialize(int pPort)
        {
            try
            {
                Uri baseAddress = new Uri(@"http://localhost:" + pPort + @"/");
                _WebServiceHost = new WebServiceHost(typeof(ClientDefinition), baseAddress);
                _WebServiceHost.Open();
                LogManager.writeLog("[Webservice] Listener started");
            }
            catch (Exception ex) { Log.LogManager.writeLog("[Webservice] ERROR: " + ex.Message); }
        }

    }
}
