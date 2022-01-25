using System;
using System.Windows;
using System.ComponentModel;
using NAudio.CoreAudioApi;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SpotifyAPI.Web;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Max
{
    /// <summary>
    /// Interaction logic for MaxUI.xaml
    /// </summary>
    public partial class MaxUI : Window
    {
        public MMDevice SpeakerDevice { get; set; }
        public DataContext MaxDataContext { get; set; }
        public MaxEngine MaxEngine { get; set; }

        //public SerialPort SerialPort;

        public MaxUI()
        {
            InitializeComponent();
            MaxDataContext = new DataContext();
            this.DataContext = MaxDataContext;
        }

        public MaxUI(MaxEngine maxEngine)
        {
            InitializeComponent();
            this.MaxEngine = maxEngine;
            MaxDataContext = new DataContext();
            this.DataContext = MaxDataContext;
        }


        private void OnContentRendered(object sender, EventArgs e)
        {
            SpeakerDevice = MaxUtils.GetSpeakerDevice();
            DispatcherTimer masterPeakVolumeTimer = new DispatcherTimer();
            masterPeakVolumeTimer.Interval = TimeSpan.FromMilliseconds(1);
            masterPeakVolumeTimer.Tick += MasterPeakVolumeTimer_Tick;
            masterPeakVolumeTimer.Start();
            ShowName();
        }
        private void MasterPeakVolumeTimer_Tick(object sender, EventArgs e)
        {
            if (SpeakerDevice != null)
            {
                int value = (int)(Math.Round(SpeakerDevice.AudioMeterInformation.MasterPeakValue * 50));
                MaxDataContext.SpeakerMasterPeakValue = (value + 300);
                MaxDataContext.CenterXY = (value + 300) / 2;
                //byte pwm = Convert.ToByte(value);
                //byte[] data = new byte[1] { pwm };
                //if (SerialPort != null)
                //{
                //    SerialPort.Write(data, 0, 1);
                //}
            }
        }


       

        public void ShowName()
        {
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(4));
            MaxText.BeginAnimation(UIElement.OpacityProperty, animation);
        }

    }
    
}
