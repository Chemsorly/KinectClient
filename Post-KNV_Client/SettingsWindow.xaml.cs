using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Post_KNV_Client.Config;

namespace Post_KNV_Client
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        /// <summary>
        /// constructor
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
            loadConfigData();
        }

        /// <summary>
        /// loads the config and writes it into the window
        /// </summary>
        private void loadConfigData()
        {
            //ClientConfig
            this._Txtbox_OwnIP.Text = Config.ConfigManager._ClientConfigObject.ownIP.ToString();
            this._Txtbox_ID.Text = Config.ConfigManager._ClientConfigObject.ID.ToString();
            this._Txtbox_name.Text = Config.ConfigManager._ClientConfigObject.name.ToString();
            this._Checkbox_DebugLog.IsChecked = Config.ConfigManager._ClientConfigObject.debug;
            
            //ClientConnectionConfig
            this._Txtbox_targetIP.Text = Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP.ToString();
            this._Txtbox_listeningPort.Text = Config.ConfigManager._ClientConfigObject.clientConnectionConfig.listeningPort.ToString();
            this._Txtbox_gatewayIP.Text = Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway.ToString();

            //ClientKinectConfig
            this._Txtbox_minDepth.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.minDepth.ToString();
            this._Txtbox_maxDepth.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.maxDepth.ToString();
            this._Txtbox_min_X_Depth.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.xMinDepth.ToString();
            this._Txtbox_max_X_Depth.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.xMaxDepth.ToString();
            this._Txtbox_min_Y_Depth.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.yMinDepth.ToString();
            this._Txtbox_max_Y_Depth.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.yMaxDepth.ToString();

            //transformation matrix
            if (Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix != null)
            {
                this._Txtbox_Trans_00.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 0].ToString();
                this._Txtbox_Trans_10.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 0].ToString();
                this._Txtbox_Trans_20.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 0].ToString();
                this._Txtbox_Trans_30.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 0].ToString();

                this._Txtbox_Trans_01.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 1].ToString();
                this._Txtbox_Trans_11.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 1].ToString();
                this._Txtbox_Trans_21.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 1].ToString();
                this._Txtbox_Trans_31.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 1].ToString();

                this._Txtbox_Trans_02.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 2].ToString();
                this._Txtbox_Trans_12.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 2].ToString();
                this._Txtbox_Trans_22.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 2].ToString();
                this._Txtbox_Trans_32.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 2].ToString();

                this._Txtbox_Trans_03.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 3].ToString();
                this._Txtbox_Trans_13.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 3].ToString();
                this._Txtbox_Trans_23.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 3].ToString();
                this._Txtbox_Trans_33.Text = Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 3].ToString();
            }
        }

        /// <summary>
        /// saves the configuration
        /// </summary>
        private void saveData()
        {
            //ClientConfig
            Config.ConfigManager._ClientConfigObject.ownIP = this._Txtbox_OwnIP.Text;
            Config.ConfigManager._ClientConfigObject.ID = int.Parse(this._Txtbox_ID.Text);
            Config.ConfigManager._ClientConfigObject.name = this._Txtbox_name.Text;
            Config.ConfigManager._ClientConfigObject.debug = (bool)this._Checkbox_DebugLog.IsChecked;

            //ClientConnectionConfig
            Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP = this._Txtbox_targetIP.Text;
            Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway = this._Txtbox_gatewayIP.Text;
            Config.ConfigManager._ClientConfigObject.clientConnectionConfig.listeningPort = int.Parse(this._Txtbox_listeningPort.Text);

            //ClientKinectConfig
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.minDepth = ushort.Parse(this._Txtbox_minDepth.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.maxDepth = ushort.Parse(this._Txtbox_maxDepth.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.xMinDepth = int.Parse(this._Txtbox_min_X_Depth.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.xMaxDepth = int.Parse(this._Txtbox_max_X_Depth.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.yMinDepth = int.Parse(this._Txtbox_min_Y_Depth.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.yMaxDepth = int.Parse(this._Txtbox_max_Y_Depth.Text);

            //transformation matrix
            if (Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix == null)
                Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix = new double[4, 4];

            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 0] = double.Parse(this._Txtbox_Trans_00.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 0] = double.Parse(this._Txtbox_Trans_10.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 0] = double.Parse(this._Txtbox_Trans_20.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 0] = double.Parse(this._Txtbox_Trans_30.Text);

            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 1] = double.Parse(this._Txtbox_Trans_01.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 1] = double.Parse(this._Txtbox_Trans_11.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 1] = double.Parse(this._Txtbox_Trans_21.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 1] = double.Parse(this._Txtbox_Trans_31.Text);

            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 2] = double.Parse(this._Txtbox_Trans_02.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 2] = double.Parse(this._Txtbox_Trans_12.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 2] = double.Parse(this._Txtbox_Trans_22.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 2] = double.Parse(this._Txtbox_Trans_32.Text);

            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[0, 3] = double.Parse(this._Txtbox_Trans_03.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[1, 3] = double.Parse(this._Txtbox_Trans_13.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[2, 3] = double.Parse(this._Txtbox_Trans_23.Text);
            Config.ConfigManager._ClientConfigObject.clientKinectConfig.transformationMatrix[3, 3] = double.Parse(this._Txtbox_Trans_33.Text);
        }

        /// <summary>
        /// saves the configuration and broadcasts it to the server so it gets updated there as well
        /// </summary>
        private void _Button_OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.saveData();
                OnConfigChangeEvent();
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        public delegate void OnConfigChangeRequest();
        public event OnConfigChangeRequest OnConfigChangeEvent;

        /// <summary>
        /// cancels the saving
        /// </summary>
        private void _Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
