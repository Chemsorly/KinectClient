using Post_KNV_MessageClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Post_KNV_Client.KinectManager
{
    /// <summary>
    /// manages the data integration of the depth data into the KinectDataPackage. notifies when the package reached the target size
    /// </summary>
    class KinectDataCollector
    {
        //the current KinectDataPackage which is filled with data
        KinectDataPackage currentPackage;

        //variable that checks if data has already been sent
        bool dataSent = false;

        //variable that checks if the data package is supposed to accept data
        bool acceptsData = false;

        /// <summary>
        /// gets fired when the data package is full
        /// </summary>
        /// <param name="pDataPackage">the data package</param>
        internal delegate void OnPackageFullEventReady(KinectDataPackage pDataPackage);
        internal event OnPackageFullEventReady OnPackageFullEvent;

        /// <summary>
        /// creates a new data package to be filled with data
        /// </summary>
        /// <param name="pCco">the ClientConfigObject with the configuration data</param>
        internal void initializeDataPackage(ClientConfigObject pCco)
        {
            currentPackage = new KinectDataPackage(pCco);
            dataSent = false;
            acceptsData = true;
        }

        /// <summary>
        /// adds depth data to the data package; notifies if package is full
        /// </summary>
        /// <param name="pDepthData">the depth data</param>
        internal void addPictureToDataPackage(ushort[] pDepthData)
        {
            if (acceptsData)
            {
                if ((currentPackage.rawDepthData.Count >= currentPackage.usedConfig.clientKinectConfig.numberOfPictures) && dataSent == false)
                {
                    OnPackageFullEvent.BeginInvoke(currentPackage, null, null);
                    dataSent = true;
                    acceptsData = false;
                    return;
                }
                currentPackage.addData(pDepthData);
            }
        }
    }
}
