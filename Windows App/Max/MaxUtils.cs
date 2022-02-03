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
using System.Diagnostics;
using SpeedTest;
using System.Media;
using NCalc;

namespace Max
{
    public class MaxUtils
    {
        private static Dictionary<string, string> MathOperatorsMap = new Dictionary<string, string> { 
            { "x", "*" }, { "X", "*" }, { "times", "*" }, { "multiplied by", "*" }, { "plus", "+" }, { "added by", "+" }, { "minus", "-" }, { "subtracted by", "-" }, { "divided by", "/" },
            { "one", "1" }, { "two", "2" }, { "three", "3" }, { "four", "4" }, { "five", "5" }, { "six", "6" }, { "seven", "7" }, { "eight", "8" }, { "nine", "9" }
        };
        public static void Run(Action action)
        {
            SafeRun(action);
        }
        public static SoundPlayer WaitingSound = new SoundPlayer(@"max.wav");

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
            string[] equationPattern = new string[] { "plus", "minus", "times", "divide", "multiplied", "square root", "+", "-", "*", "/", "X" , "x" };

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
        public static void PlayWaitingSound()
        {
            new Thread(WaitingSound.Play).Start();
        }

        public static void StopWaitingSound()
        {
            new Thread(WaitingSound.Stop).Start();
        }


        public static void CheckInternetSpeed()
        {
            PlayWaitingSound();
            ISpeedTestClient _speedTestClientClient = new SpeedTestClient();
            var settings = _speedTestClientClient.GetSettings();
            var latency = _speedTestClientClient.TestServerLatency(settings.Servers.First());
            var downloadSpeed = _speedTestClientClient.TestDownloadSpeed(settings.Servers.First(), settings.Download.ThreadsPerUrl);
            var uploadSpeed = _speedTestClientClient.TestUploadSpeed(settings.Servers.First(), settings.Upload.ThreadsPerUrl);

            string delay = (latency > 100) ? "high" : "low";

            string data = $"{{!salutation}}, your network delay is {delay}, it is equivalent to {latency} milliseconds and your download speed is {downloadSpeed.ToString("#")} Megabits per seconds.";
            App.GetEngine().BrainEngine.Log($"latency : {latency}");
            App.GetEngine().BrainEngine.Log($"downloadSpeed : {downloadSpeed}");
            App.GetEngine().BrainEngine.Log($"uploadSpeed : {uploadSpeed}");
            WaitingSound.Stop();
            App.GetEngine().VoiceOutputEngine.Speak(data);
        }

        public static void Shutdown()
        {
            PlayWaitingSound();
            Process.Start("shutdown", "/s /t 5");
            WaitingSound.Stop();
        }

        public static string SolveEquation(string query)
        {
            //query = ResolveQuery(query);
            App.GetEngine().BrainEngine.Log($"SolveEquation :  {query}");
            var expression = new Expression(query);
            var result = expression.Evaluate();
            return NumberToWords((int)result);
        }

        public static string ResolveQuery(string query)
        {
            string resolvedQuery = string.Empty;

            if (query != null)
            {
                App.GetEngine().BrainEngine.Log($"ResolveQuery :  {query}");
                foreach (string word in query.Split(" "))
                {
                    App.GetEngine().BrainEngine.Log($"ResolveQuery :  {word}");
                    if (MathOperatorsMap.ContainsKey(word))
                    {
                        App.GetEngine().BrainEngine.Log($"MathOperatorsMap[word] :  {MathOperatorsMap[word]}");
                        resolvedQuery = query.Replace(word, MathOperatorsMap[word]);
                    }
                }
            }
            App.GetEngine().BrainEngine.Log($"ResolveQuery :  {resolvedQuery}");
            return resolvedQuery;
        }
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words;
        }
        public static int WordsToNumber(string number)
        {
            string[] words = number.ToLower().Split(new char[] { ' ', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ones = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            string[] teens = { "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            string[] tens = { "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            Dictionary<string, int> modifiers = new Dictionary<string, int>() { {"billion", 1000000000}, {"million", 1000000}, {"thousand", 1000}, {"hundred", 100} };

            if (number == "eleventy billion")
                return int.MaxValue; // 110,000,000,000 is out of range for an int!

            int result = 0;
            int currentResult = 0;
            int lastModifier = 1;

            foreach (string word in words)
            {
                if (modifiers.ContainsKey(word))
                {
                    lastModifier *= modifiers[word];
                }
                else
                {
                    int n;

                    if (lastModifier > 1)
                    {
                        result += currentResult * lastModifier;
                        lastModifier = 1;
                        currentResult = 0;
                    }

                    if ((n = Array.IndexOf(ones, word) + 1) > 0)
                    {
                        currentResult += n;
                    }
                    else if ((n = Array.IndexOf(teens, word) + 1) > 0)
                    {
                        currentResult += n + 10;
                    }
                    else if ((n = Array.IndexOf(tens, word) + 1) > 0)
                    {
                        currentResult += n * 10;
                    }
                    else if (word != "and")
                    {
                        throw new ApplicationException("Unrecognized word: " + word);
                    }
                }
            }

            return result + currentResult * lastModifier;
        }
    }
    
}
