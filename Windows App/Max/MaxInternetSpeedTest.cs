using SpeedTest;
using SpeedTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max
{
    public class MaxInternetSpeedTest : MaxService
    {
        public ISpeedTestClient SpeedTestClientClient = new SpeedTestClient();
        public MaxInternetSpeedTest()
        {
            this.OnStart();
            SpeedTestClientClient = new SpeedTestClient();
        }

        public void GetInternetSpeedTest()
        {
            try
            {
                App.GetEngine().BrainEngine.Log($"Testing Internet Speed ...");
                var settings = SpeedTestClientClient.GetSettings();
                App.GetEngine().BrainEngine.Log($"Initializing Settings ...");
                App.GetEngine().BrainEngine.Log($"Looking for Nearest Matching Server ...");

                foreach (Server s in settings.Servers)
                {
                    App.GetEngine().BrainEngine.Log($"Found Server : Name:{s.Name} Host:{s.Host} Country:{s.Country} Distance:{s.Distance} Sponsor:{s.Sponsor}");
                }
                var server = settings.Servers.Where(s1 => s1.Sponsor.Contains("PLDT")).First();
                if (server != null)
                {
                    App.GetEngine().BrainEngine.Log($"Selecting : Name:{server.Name} Host:{server.Host} Country:{server.Country} Distance:{server.Distance} Sponsor:{server.Sponsor}");
                }
                else
                {
                    App.GetEngine().BrainEngine.Log($"No Matching Server from your provider. Selecting Nearest Server...");
                    server = settings.Servers.First();
                    App.GetEngine().BrainEngine.Log($"Selecting : Name:{server.Name} Host:{server.Host} Country:{server.Country} Distance:{server.Distance} Sponsor:{server.Sponsor}");
                }
                App.GetEngine().BrainEngine.Log($"Testing Latency ...");
                var latency = SpeedTestClientClient.TestServerLatency(server);
                App.GetEngine().BrainEngine.Log($"Latency : {latency}");
                App.GetEngine().BrainEngine.Log($"Testing Download Speed ...");
                var downloadSpeed = SpeedTestClientClient.TestDownloadSpeed(server, settings.Download.ThreadsPerUrl);
                App.GetEngine().BrainEngine.Log($"Download Speed : {downloadSpeed}");
                App.GetEngine().BrainEngine.Log($"Testing Upload Speed ...");
                var uploadSpeed = SpeedTestClientClient.TestUploadSpeed(server, settings.Upload.ThreadsPerUrl);
                App.GetEngine().BrainEngine.Log($"Upload Speed : {uploadSpeed}");

                string delay = (latency > 100) ? "high" : "low";

                string data = $"{{!salutation}}, your network delay is {delay}, it is equivalent to {latency} milliseconds and your download speed is {downloadSpeed.ToString("#")} Megabits per seconds.";
                OnFinished();
                App.GetEngine().VoiceOutputEngine.Speak(data);
            } 
            catch (Exception e)
            {
                OnFinished();
                string data = $"{{!salutation}}, I encountered an error while testing. It say's {e.Message}";
                App.GetEngine().VoiceOutputEngine.Speak(data);
            }
        }

        public override void StartService()
        {
            GetInternetSpeedTest();
        }
    }
}
