using Post_KNV_Client.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Post_KNV_Client
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// the data manager; manages the logic
        /// </summary>
        DataManager.DataManager _DataManager;

        /// <summary>
        /// constructor; starting point for program
        /// </summary>
        public MainWindow()
        {
            //startup point
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    MessageBox.Show("Sorry, this application must be run as Administrator.");
                }

                // Shut down the current process
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();
            if (IsRunAsAdministrator()) writeConsole("[Application] Elevated to admin rights.");

            //subscribe to log events such as client status updates or message events
            LogManager.OnLogMessageEvent += LogManager_OnLogMessageEvent;
            LogManager.OnLogMessageDebugEvent += LogManager_OnLogMessageDebugEvent;
            LogManager.OnClientStatusChangeEvent += updateClientStatusDisplay;
            LogManager.OnKinectStatusChangeEvent += updateKinectStatusDisplay;
            LogManager.OnKinectFPSEvent += LogManager_OnKinectFPSEvent;

            //initialize data manager
            _DataManager = DataManager.DataManager.getInstance();
            _DataManager.OnColorPictureEvent += _DataManager_OnKinectColorPictureRecieved;
            _DataManager.OnDepthPictureEvent += _DataManager_OnKinectDepthPictureRecieved;

            //initialize ui status
            this._CheckboxCalculateColor.IsChecked = true;

            //initially update the client status
            this.updateClientStatusDisplay(ClientStatus.disconnected);
        }

        /// <summary>
        /// checks if the application is run as admin
        /// </summary>
        /// <returns>true if run as admin</returns>
        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #region event handlers

        void LogManager_OnKinectFPSEvent(int pFPS)
        {
            this.Dispatcher.Invoke(() => this._Txtblock_KinectFPS.Text = pFPS.ToString());            
        }

        /// <summary>
        /// gets fired everytime the data manager broadcasts a new color picture for the ui
        /// </summary>
        /// <param name="pColorPicture">the color picture</param>
        void _DataManager_OnKinectColorPictureRecieved(WriteableBitmap pColorPicture)
        {
            this.Dispatcher.Invoke(() =>
            {
                this._ColorPicture.Source = pColorPicture;
            });
        }

        /// <summary>
        /// gets fired everytime the data manager broadcasts a new depth picture for the ui
        /// </summary>
        /// <param name="pColorPicture">the depth picture</param>
        void _DataManager_OnKinectDepthPictureRecieved(WriteableBitmap pDepthPicture)
        {
            this.Dispatcher.Invoke(() =>
            {
                this._DepthPicture.Source = pDepthPicture;
            });
        }

        /// <summary>
        /// gets fired every time the log manager broadcasts a new debug message event
        /// </summary>
        /// <param name="message">the message</param>
        void LogManager_OnLogMessageDebugEvent(String message)
        {
            if (Config.ConfigManager._ClientConfigObject.debug)
                writeConsole(message);
        }

        /// <summary>
        /// gets fired every time the log manager broadcasts a new message event
        /// </summary>
        /// <param name="message"></param>
        void LogManager_OnLogMessageEvent(String message)
        {
            writeConsole(message);
        }

        /// <summary>
        /// writes in the console
        /// </summary>
        /// <param name="message">the output message</param>
        void writeConsole(String message)
        {
            this.Dispatcher.Invoke(() =>
                {
                    _Console.Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + " \n" + _Console.Text;
                });
        }

        /// <summary>
        /// helper variable to check if a settings window is already loaded
        /// </summary>
        SettingsWindow lastActiveSettingsWindow;
        /// <summary>
        /// function to call the Settings Window
        /// </summary>
        private void _Button_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (lastActiveSettingsWindow == null || !lastActiveSettingsWindow.IsLoaded)
            {
                lastActiveSettingsWindow = new SettingsWindow();
                lastActiveSettingsWindow.OnConfigChangeEvent += lastActiveSettingsWindow_OnConfigChangeEvent;
            }
            lastActiveSettingsWindow.Show();
        }

        /// <summary>
        /// gets fired when the settings window requests a config change; saves data, requests a config change to the server and updates the status
        /// </summary>
        void lastActiveSettingsWindow_OnConfigChangeEvent()
        {
            _DataManager.saveConfig();
            updateClientStatusDisplay(ClientStatus.unknown);
            updateKinectStatusDisplay(KinectStatus.unknown);
        }

        #endregion

        #region status update

        public enum ClientStatus { connected, waiting_for_ping, disconnected, unknown };
        public enum KinectStatus { connected, not_connected, unknown };
        /// <summary>
        /// updates the ui with the specified client status; fetches current data from the config
        /// </summary>
        /// <param name="pStatus">specified client status</param>
        private void updateClientStatusDisplay(ClientStatus pStatus)
        {
            this.Dispatcher.Invoke(() =>
            {

                //status block connection
                switch (pStatus)
                {
                    case ClientStatus.connected:
                        this._StatusBlockConnection.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(new Uri(@"pack://siteoforigin:,,,/Resources/checkmark_256_green.png"));
                        this._Txtblock_ConnectionStatus.Text = "connected";
                        break;
                    case ClientStatus.disconnected:
                        this._StatusBlockConnection.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(new Uri(@"pack://siteoforigin:,,,/Resources/cross_256_orange.png"));
                        this._Txtblock_ConnectionStatus.Text = "disconnected";
                        break;
                    case ClientStatus.waiting_for_ping:
                        this._StatusBlockConnection.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(new Uri(@"pack://siteoforigin:,,,/Resources/impressions_general_help_256.png"));
                        this._Txtblock_ConnectionStatus.Text = "waiting for ping";
                        break;
                    default:
                        break;
                }

                this._Txtblock_CurrentID.Text = Config.ConfigManager._ClientConfigObject.ID.ToString();
                this._Txtblock_CurrentIP.Text = Config.ConfigManager._ClientConfigObject.ownIP.ToString();
                this._Txtblock_ServerIP.Text = Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetIP.ToString();
                this._Txtblock_GatewayIP.Text = Config.ConfigManager._ClientConfigObject.clientConnectionConfig.targetGateway.ToString();
            });
        }

        /// <summary>
        /// updates the ui with the kinect status; fetches current data from the config
        /// </summary>
        /// <param name="pStatus">specified kinect status</param>
        private void updateKinectStatusDisplay(KinectStatus pStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (pStatus)
                {
                    case KinectStatus.connected:
                        this._StatusBlockKinect.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(new Uri(@"pack://siteoforigin:,,,/Resources/checkmark_256_green.png"));
                        this._Txtblock_KinectStatus.Text = "connected";
                        break;
                    case KinectStatus.not_connected:
                        this._StatusBlockKinect.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(new Uri(@"pack://siteoforigin:,,,/Resources/cross_256_orange.png"));
                        this._Txtblock_KinectStatus.Text = "not connected";
                        break;
                    default:
                        break;
                }
            });
        }

        #endregion

        /// <summary>
        /// checks or unchecks the color calculation
        /// </summary>
        private void _CheckboxCalculatePictures_Checked(object sender, RoutedEventArgs e)
        {
            if(sender == this._CheckboxCalculateColor)
                Config.ConfigManager._ClientConfigObject.clientKinectConfig.calculateColor = (bool)this._CheckboxCalculateColor.IsChecked;
        }

    }
}
