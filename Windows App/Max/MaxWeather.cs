using OpenWeatherAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Max
{
    public class MaxWeather : MaxService
    {
        private OpenWeatherApiClient OpenWeatherApiClient;
        private string Location;
        private string WeatherIconFolder = "weather-icons";
        private const string apiKey = "d02f9f216395b2b8d82b34dd367f5a62";
        public MaxWeather(MaxEngine maxEngine, MaxUI maxUI, string location) : base(maxEngine, maxUI)
        {
            this.OnStart();
            this.OpenWeatherApiClient = new OpenWeatherApiClient(apiKey);
            this.Location = location;
            this.Log($"Starting Service: {nameof(MaxWeather)}");
        }

        public void DownloadIcon(string url)
        {
            this.Log($"{nameof(MaxWeather)}: Downloading Icon {url}");
            using (WebClient client = new WebClient())
            {
                Uri uri = new Uri(url);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                string path = Path.Combine(Environment.CurrentDirectory, $"{MaxEngine.ImagesFolder}\\{WeatherIconFolder}\\", filename);
                client.DownloadFileAsync(new Uri(url), path);
                this.Log($"{nameof(MaxWeather)}: \tIcon Saved: {path}");
            }
        }

        public string CheckIcon(string url)
        {
            Uri uri = new Uri(url);
            string filename = System.IO.Path.GetFileName(uri.LocalPath);
            string path = Path.Combine(Environment.CurrentDirectory, $"{MaxEngine.ImagesFolder}\\{WeatherIconFolder}\\", filename);
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return fileInfo.FullName;
            } 
            else
            {
                return string.Empty;
            }
        }

        public async void GetWeather()
        {
            string data = string.Empty;
            try
            {
                this.Log($"{nameof(MaxWeather)}: Getting Weather ...");
                this.Log($"{nameof(MaxWeather)}: Getting Weather in {Location}...");
                var query = await OpenWeatherApiClient.QueryAsync(Location);
                string weatherCondition = "";
                foreach(Weather weather in query.WeatherList)
                {
                    this.Log($"{nameof(MaxWeather)}: {weather.Main}");
                    this.Log($"{nameof(MaxWeather)}: {weather.Description}");
                    weatherCondition = weather.Description;
                    string iconUrl = $"https://openweathermap.org/img/wn/{weather.Icon}@4x.png";
                    string iconFileUrl = CheckIcon(iconUrl);
                    if (string.IsNullOrEmpty(iconFileUrl))
                    {
                        this.Log($"{nameof(MaxWeather)}: \tIcon Does not Exist, Using url {iconUrl}");
                        DownloadIcon(iconUrl);
                    }
                    else
                    {
                        this.Log($"{nameof(MaxWeather)}: Icon Exist, using local icon path {iconFileUrl}");
                        iconUrl = iconFileUrl;
                    }
                    this.MaxUI.ShowIllustration(iconUrl);
                    break;
                }
                
                if (!string.IsNullOrEmpty(weatherCondition))
                {
                    data = $"{{!salutation}}, the weather in {query.Name} is {weatherCondition} and the temperature is {query.Main.Temperature.CelsiusCurrent} °C.";
                } 
                else
                {
                    data = $"{{!salutation}}, the temperature in {query.Name} is {query.Main.Temperature.CelsiusCurrent} °C.";
                }
            } 
            catch(Exception e)
            {
                data = $"{{!salutation}}, I encountered an error while checking the weather. It say's {e.Message}";
            } 
            finally
            {
                this.Log($"{nameof(MaxWeather)} : {data}");
                this.OnFinished(data);
            }

        }

        public override void StartService()
        {
            GetWeather();
        }
    }
}
