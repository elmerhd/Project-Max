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
using System.Windows.Media.Imaging;

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

        public string LastSubText { get; set; }

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
        public void UpdateResponseText(string text) 
        {
            this.Dispatcher.Invoke(()=> {
                LastSubText = text;
                ResponseText.Text = text;
            });
        }

        public void UpdateRecognizedText(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                RecognizedText.Text = text;
            });
        }

        public void ShowUI() {
            this.Dispatcher.Invoke(() =>
            {
                Show();
            });
        }

        public void HideUI()
        {
            this.Dispatcher.Invoke(() =>
            {
                Hide();
            });
        }

        public void ShowIllustration(string imageURL)
        {
            this.Dispatcher.Invoke(() =>
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageURL, UriKind.Absolute);
                bitmap.EndInit();

                IllustrationImage.Source = bitmap;
                HideElement(MaxText);
                ShowElement(IllustrationImage);
                RemoveIllustrations();
            });
        }

        public void RemoveIllustrations()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            timer.Tick += (sender, args) =>
            {
                HideElement(IllustrationImage);
                ShowElement(MaxText);
                timer.Stop();
            };

            timer.Start();
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            SpeakerDevice = MaxUtils.GetSpeakerDevice();
            DispatcherTimer masterPeakVolumeTimer = new DispatcherTimer();
            masterPeakVolumeTimer.Interval = TimeSpan.FromMilliseconds(1);
            masterPeakVolumeTimer.Tick += MasterPeakVolumeTimer_Tick;
            masterPeakVolumeTimer.Start();
            ShowElement(MaxText);
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


       

        public void ShowElement(UIElement element)
        {
            element.Opacity = 0;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(4));
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void HideElement(UIElement element)
        {
            element.Opacity = 1;
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(4));
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

    }
    
}
