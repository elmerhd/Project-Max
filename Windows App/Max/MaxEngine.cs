using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Max
{
    public class MaxEngine
    {
        public VoiceEngine VoiceOutputEngine;

        public BrainEngine BrainEngine;

        public ServerEngine ServerEngine;

        public MaxConfig MaxConfig;

        public SpotifyEngine SpotifyEngine;

        public FaceRecognitionEngine FaceRecognitionEngine;

        public NetflixEngine NetflixEngine;

        public AppEngine AppEngine;

        public string ConfigFolder = @"config";

        public string BrainFolder = @"brain";

        public string LogsFolder = @"logs";

        public string FacesFolder = @"brain/faces";

        public string FaceFile = "faces.json";

        private string ConfigFile = "config.json";

        private string HaarCascadeFile = "haarcascade_frontalface_default.xml";

        public MaxEngine()
        {
            FixFilesAndFolders();
            InitConfig();
        }

        public void FixFilesAndFolders()
        {
            
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }
            
            if (!Directory.Exists(BrainFolder))
            {
                Directory.CreateDirectory(BrainFolder);
            }
            
            if (!Directory.Exists(LogsFolder))
            {
                Directory.CreateDirectory(LogsFolder);
            }

            if (!Directory.Exists(FacesFolder))
            {
                Directory.CreateDirectory(FacesFolder);
            }
        }

        public void InitConfig()
        {
            FileInfo fileInfo = new FileInfo( $"{ConfigFolder}/{ConfigFile}");

            if (fileInfo.Exists)
            {
                MaxConfig = MaxConfig.ReadConfig(fileInfo.FullName);
            }
            else
            {
                MaxConfig = new MaxConfig();
                MaxConfig.Voice = "IVONA 2 Emma";
                MaxConfig.AppId = "93e667aa-3452-43a9-a2d7-12ee078fead4";
                MaxConfig.DefaultSalutation = "sir";
                MaxConfig.DefaultUserName = "elmer";
                MaxConfig.Speaker = "Speakers (High Definition Audio Device)";
                MaxConfig.SpotifyDeviceId = "a28a3bda23a1a3254ebeeec083eb1660cd6da771";
                MaxConfig.DefaultOnlineMessages.AddRange(new string[] { "I am online and ready, {!salutation}.", "I am now online {!salutation}"});
                MaxConfig.DefaultOKMessages.AddRange(new string[] { "Yes, {!salutation}.", "Okay, {!salutation}.", "For you {!salutation}, anything.", "Go ahead, {!salutation}.", "I'm Listening.", "I'm Listening, {!salutation}."});
                MaxConfig.DefaultIntroductionMessages.AddRange(new string[] { "Allow me to introduce myself. My name is max, an autonomous computer help program and personal assistant.", "Hello there, My name is max. an autonomous computer help program and personal assistant." });
                MaxConfig.DefaultCommandMessages.AddRange(new string[] { " Yes, {!salutation}.", "Okay, {!salutation}.", "Right away, {!salutation}.", "As you wish, {!salutation}.", "I'm on it.", "Just a second, {!salutation}.", "For you {!salutation}, anything." , "One moment {!salutation}." });
                MaxConfig.DefaultWaitingMessages.AddRange(new string[] { "One moment {!salutation}", "I'm on it." , "Just a second, {!salutation}", "Checking {!salutation}", "Yes, I can do it.", "For you {!salutation} anything"});
                MaxConfig.FacePhotosPath = FacesFolder;
                MaxConfig.FaceListTextFile = $"{FacesFolder}/{FaceFile}";
                MaxConfig.HaarCascadePath = $"{ConfigFolder}/{HaarCascadeFile}";
                MaxConfig.ImageFileExtension = ".bmp";
                MaxConfig.ActiveCameraIndex = 0;
                MaxConfig.WriteConfig(MaxConfig, fileInfo.FullName);
            }
        }

        public void Load()
        {
            new Thread(MaxUtils.WaitingSound.Play).Start();
            BrainEngine = new BrainEngine(this);
            VoiceOutputEngine = new VoiceEngine(this);
            VoiceOutputEngine.Speak(MaxConfig.DefaultIntroductionMessages[new Random().Next(MaxConfig.DefaultIntroductionMessages.Count)]);
            SpotifyEngine = new SpotifyEngine(this);
            AppEngine = new AppEngine(this);
            ServerEngine = new ServerEngine(this);
            //FaceRecognitionEngine = new FaceRecognitionEngine(this);
            NetflixEngine = new NetflixEngine(this);
            VoiceOutputEngine.Speak(MaxConfig.DefaultOnlineMessages[new Random().Next(MaxConfig.DefaultOnlineMessages.Count)]);
            MaxUtils.WaitingSound.Stop();
            
        }
    }

    public class MaxConfig
    {
        public string Voice { get; set; }
        public string AppId { get; set; }
        public string DefaultSalutation { get; set; }
        public string DefaultUserName { get; set; }
        public List<string> DefaultOnlineMessages { get; set; }
        public string Speaker { get; set; }
        public string SpotifyDeviceId { get; set; }
        public List<string> DefaultCommandMessages { get; set; }
        public List<string> DefaultIntroductionMessages { get; set; }
        public List<string> DefaultWaitingMessages { get; set; }
        public List<string> DefaultOKMessages { get; set; }
        public string FacePhotosPath { get; set; }
        public string FaceListTextFile { get; set; }
        public string HaarCascadePath { get; set; }
        public int TimerResponseValue { get; set; }
        public string ImageFileExtension { get; set; }
        public int ActiveCameraIndex { get; set; }

        public MaxConfig()
        {
            DefaultCommandMessages = new List<string>();
            DefaultIntroductionMessages = new List<string>();
            DefaultOnlineMessages = new List<string>();
            DefaultWaitingMessages = new List<string>();
            DefaultOKMessages = new List<string>();
        }

        public static void WriteConfig(MaxConfig conf, string file)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            using (StreamWriter sw = new StreamWriter(file))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, conf);
            }
        }
        public static MaxConfig ReadConfig(string file)
        {
            return JsonConvert.DeserializeObject<MaxConfig>(File.ReadAllText(file));
        }
    }

}
