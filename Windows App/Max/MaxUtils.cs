using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Drawing;
using NAudio.CoreAudioApi;
using System.Management;
using Microsoft.WindowsAPICodePack.Shell;
using System.Windows.Media;
using System.IO.Ports;
using System.Reflection;

namespace Max
{
    public class MaxUtils
    {
        public static void Run(Action action)
        {
            SafeRun(action);
        }

        public static void SafeRun(Action dAction)
        {
            Thread thr = new Thread(new ThreadStart(dAction));
            thr.Start();
        }

        //public static Bitmap GenerateQRCode(int width, int height, string text)
        //{
        //    var barcodeWriter = new BarcodeWriter();
        //    var encodingOptions = new EncodingOptions { Width = width, Height = height, Margin = 0, PureBarcode = false };
        //    encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
        //    barcodeWriter.Renderer = new BitmapRenderer {
        //        Background = Color.Black,
        //        Foreground = System.Drawing.ColorTranslator.FromHtml("#00FF33")
        //};
        //    barcodeWriter.Options = encodingOptions;
        //    barcodeWriter.Format = BarcodeFormat.QR_CODE;
        //    Bitmap bitmap = barcodeWriter.Write(text);
        //    return bitmap;
        //}

        public static void PlayWelcomeAudio()
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"max.wav");
            player.Play();
            Thread.Sleep(100);
        }
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static MMDevice GetSpeakerDevice()
        {
            MMDevice speaker = null;
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in devices)
            {
                Console.WriteLine(device.FriendlyName);
                if (device.FriendlyName == App.GetEngine().MaxConfig.Speaker)
                {
                    speaker = device;
                }
            }
            return speaker;
        }
        public static string [] GetDefinitionStrings()
        {
            return new string []
            {
                "*",
                "what is *",
                "what is a *",
                "what is the *",
                "what is an *"
            };
        }
        public static bool HasEquation(string query)
        {
            string[] equationPattern = new string[] { "plus", "minus", "times", "divide", "multiplied", "square root" };

            return equationPattern.Any(eq => query.Contains(eq));
        }

        /// <summary>
        /// Separate array to by comma and put `and` on last word 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ArrayToWords(string [] array)
        {
            string word = string.Empty;
            if (array.Length <= 2)
            {
                word = string.Join(" and ", array);
            }
            else if (array.Length > 2)
            {
                word = string.Join(", ", array);
                word = ReplaceLastOccurrence(word, ", ", " and ");
            }
            return word;
        }

        /// <summary>
        /// Replace last occurence of the word
        /// </summary>
        /// <param name="source"></param>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
            {
                return source;
            }

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        public static void ListAllCamerasAsync()
        {
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM win32_PNPEntity WHERE PnPClass='Camera'");
            ManagementObjectCollection objCollection = objSearcher.Get();
            var cameraIndex = 0;
            foreach (ManagementObject cameraDevice in objCollection)
            {
                Console.WriteLine($"Camera {cameraIndex}: {cameraDevice["Name"]}");
                cameraIndex++;
            }
        }

        public static void ListAllAudioAsync()
        {
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");

            ManagementObjectCollection objCollection = objSearcher.Get();
            var audioIndex = 0;
            foreach (ManagementObject audioDevice in objCollection)
            {
                Console.WriteLine($"Audo {audioIndex}: {audioDevice["Name"]}");
                audioIndex++;
            }
        }

        //public static SerialPort GetSerialPort()
        //{
        //    string portName = SerialPort.GetPortNames().FirstOrDefault(s => !String.IsNullOrEmpty(s)) ?? string.Empty;
        //    if ( !String.IsNullOrEmpty(portName))
        //    {
        //        return new SerialPort(portName, 9600);
        //    } 
        //    else
        //    {
        //        return null;
        //    }
        //}



    }

}
