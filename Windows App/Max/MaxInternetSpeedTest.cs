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

        public MaxInternetSpeedTest(MaxEngine maxEngine, MaxUI maxUI) : base(maxEngine, maxUI)
        {
            this.OnStart();
            SpeedTestClientClient = new SpeedTestClient();
        }

        public void GetInternetSpeedTest()
        {
            string data = string.Empty;
            Server server = null;
            try
            {
                this.Log($"Testing Internet Speed ...");
                var settings = SpeedTestClientClient.GetSettings();
                this.Log($"Initializing Settings ...");
                this.Log($"Looking for Nearest Matching Server ...");
                foreach (Server foundServer in settings.Servers)
                {
                    this.Log($"Found Server : Name:{foundServer.Name} Host:{foundServer.Host} Country:{foundServer.Country} Distance:{foundServer.Distance} Sponsor:{foundServer.Sponsor}");
                    if ((foundServer.Sponsor.Contains("PLDT") || foundServer.Sponsor.Contains("Smart")))
                    {
                        server = foundServer;
                        break;
                    }
                }
                if (server != null)
                {
                    this.Log($"Selecting : Name:{server.Name} Host:{server.Host} Country:{server.Country} Distance:{server.Distance} Sponsor:{server.Sponsor}");
                }
                else
                {
                    this.Log($"No Matching Server from your provider. Selecting Nearest Server...");
                    server = settings.Servers.First();
                    this.Log($"Selecting : Name:{server.Name} Host:{server.Host} Country:{server.Country} Distance:{server.Distance} Sponsor:{server.Sponsor}");
                }
                this.Log($"Testing Latency ...");
                var latency = SpeedTestClientClient.TestServerLatency(server);
                this.Log($"Latency : {latency}");
                this.Log($"Testing Download Speed ...");
                var downloadSpeed = SpeedTestClientClient.TestDownloadSpeed(server, settings.Download.ThreadsPerUrl);
                this.Log($"Download Speed : {downloadSpeed}");
                this.Log($"Testing Upload Speed ...");
                var uploadSpeed = SpeedTestClientClient.TestUploadSpeed(server, settings.Upload.ThreadsPerUrl);
                this.Log($"Upload Speed : {uploadSpeed}");

                string delay = (latency > 100) ? "high" : "low";
                if (latency < 50 && latency > 1)
                {
                    delay = "very low";
                }
                else if (latency < 100 && latency > 50)
                {
                    delay = "low";
                }
                else if (latency < 150 && latency > 100)
                {
                    delay = "high";
                } else if (latency < 200 && latency > 150)
                {
                    delay = "very high";
                }

                data = $"{{!salutation}}, your network delay is {delay}, it is equivalent to {latency} milliseconds, your download speed is {downloadSpeed.ToString("N0")} Megabits per second and your upload speed is {uploadSpeed.ToString("N0")} Megabits per second.";
                
            } 
            catch (Exception e)
            {
                data = $"{{!salutation}}, I encountered an error while testing the internet speed. It say's {e.Message}";
            }
            finally
            {
                this.OnFinished(data);
            }
        }

        public override void StartService()
        {
            GetInternetSpeedTest();
        }
    }
}
