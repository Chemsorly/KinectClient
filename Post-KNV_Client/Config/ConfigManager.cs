using Post_KNV_Client.Log;
using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Post_KNV_Client.Config
{
    public class ConfigManager
    {
        //public static config object available to the entire project
        public static ClientConfigObject _ClientConfigObject;
        static String configFilePath = @"config\";
        static String configFileName = @"ClientConfig.dat";

        #region external

        /// <summary>
        /// constructor, loads the current config file
        /// </summary>
        public ConfigManager()
        {
            try
            {
                if (checkForConfigFile(configFilePath + configFileName))
                {
                    _ClientConfigObject = loadConfigFromFile(configFilePath + configFileName);
                    LogManager.writeLog("[ConfigManager] Config loaded from file");
                }
                else
                {
                    _ClientConfigObject = ClientConfigObject.createDefaultConfig();
                    //_ClientConfigObject.clientConnectionConfig.targetIP = "localhost:8999"; //ToDo: GATEWAY
                    LogManager.writeLog("[ConfigManager] Default config loaded");
                }
            }
            catch (Exception e)
            {
                LogManager.writeLog("[ConfigManager] " + e.ToString());
                _ClientConfigObject = ClientConfigObject.createDefaultConfig();
                //_ClientConfigObject.clientConnectionConfig.targetIP = "localhost:8999";
            }

            saveConfig();
            LogManager.writeLog("[ConfigManager] Successfully initialized");
        }

        /// <summary>
        /// updates the current config and saves it to a file
        /// </summary>
        /// <param name="inputConfig">the config object recieved from the server</param>
        public void writeConfig(ClientConfigObject inputConfig)
        {
            _ClientConfigObject = inputConfig;
            saveConfig();
        }

        #endregion

        #region internal

        /// <summary>
        /// saves the config to a file
        /// </summary>
        static void saveConfig()
        {
            saveConfigToFile(_ClientConfigObject, configFilePath, configFileName);
        }

        /// <summary>
        /// checks if a path contains a config file
        /// </summary>
        /// <param name="pConfigFilePath">the filepath</param>
        /// <returns>true if yes, else false</returns>
        static bool checkForConfigFile(String pConfigFilePath)
        {
            if (File.Exists(pConfigFilePath))
                return true;
            return false;
        }

        /// <summary>
        /// loads a config object from the configfile
        /// </summary>
        /// <param name="inputPath">config file path</param>
        /// <returns>config object</returns>
        static ClientConfigObject loadConfigFromFile(String inputPath)
        {
            FileStream stream = new FileStream(inputPath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            ClientConfigObject _returnObject = (ClientConfigObject)formatter.Deserialize(stream);
            stream.Close();

            _returnObject.ownIP = getOwnIP();
            return _returnObject;
        }

        /// <summary>
        /// saves the config to a file
        /// </summary>
        /// <param name="inputObject">the config object to save</param>
        /// <param name="configFilePath">the filepath</param>
        /// <param name="configFileName">the filename</param>
        static void saveConfigToFile(ClientConfigObject inputObject, String configFilePath, String configFileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (!checkForConfigFile(configFilePath + configFileName)) Directory.CreateDirectory(configFilePath);
            FileStream stream = new FileStream(configFilePath + configFileName, FileMode.Create);            

            formatter.Serialize(stream, inputObject);
            stream.Flush();
            stream.Close();
        }

        /// <summary>
        /// returns the current ipv4 of the main network adapter
        /// </summary>
        /// <returns>the own ipv4</returns>
        static String getOwnIP()
        {
            string localIP = "";
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                IPHostEntry host;
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            return localIP;
        }

        #endregion
    }
}
