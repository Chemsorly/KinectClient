using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Post_KNV_Client.Config
{
    public class ClientConnectionConfig
    {
        public static ClientConnectionConfigObject _ClientConnectionConfigObject { get; private set; }
        static String configFilePath = @"config\";
        static String configFileName = @"ClientConnectionConfig.xml";

        public ClientConnectionConfig()
        {
            try
            {
                if (checkForConfigFile(configFilePath + configFileName))
                    _ClientConnectionConfigObject = loadConfigFromFile(configFilePath + configFileName);
                else
                    _ClientConnectionConfigObject = loadDefaultConfig();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                _ClientConnectionConfigObject = loadDefaultConfig();
            }

            saveConfig();
        }

        public static void writeConfig(ClientConnectionConfigObject inputConfig)
        {
            _ClientConnectionConfigObject = inputConfig;
            saveConfig();
        }

        private static void saveConfig()
        {
            saveConfigToFile(_ClientConnectionConfigObject, configFilePath, configFileName);
        }

        static bool checkForConfigFile(String pConfigFilePath)
        {
            if (File.Exists(pConfigFilePath))
                return true;
            return false;
        }

        static ClientConnectionConfigObject loadConfigFromFile(String inputPath)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(ClientConnectionConfigObject));
            TextReader textReader = new StreamReader(inputPath);
            ClientConnectionConfigObject _returnObject = (ClientConnectionConfigObject)deserializer.Deserialize(textReader);
            textReader.Close();

            _returnObject.kinectClientConfig.ownIP = getOwnIP();
            return _returnObject;
        }

        static void saveConfigToFile(ClientConnectionConfigObject inputObject, String configFilePath, String configFileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ClientConnectionConfigObject));
            if (!checkForConfigFile(configFilePath + configFileName)) Directory.CreateDirectory(configFilePath);
            TextWriter textWriter = new StreamWriter(configFilePath + configFileName);
            serializer.Serialize(textWriter, inputObject);
            textWriter.Close();
        }

        ClientConnectionConfigObject loadDefaultConfig()
        {
            ClientConnectionConfigObject _returnObject = new ClientConnectionConfigObject();
            _returnObject.kinectClientConfig.ID = "default";
            _returnObject.kinectClientConfig.ownIP = getOwnIP();
            _returnObject.targetIP = "localhost:8000";

            return _returnObject;
        }

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
    }

    public class ClientConnectionConfigObject
    {
        public String targetIP;
        public KinectClientObject kinectClientConfig;
    }

    
}
