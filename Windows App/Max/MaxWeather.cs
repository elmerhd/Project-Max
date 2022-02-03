using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max
{
    public class MaxWeather : MaxService
    {
        private OpenWeatherAPI.OpenWeatherApiClient OpenWeatherApiClient;
        private string Location;
        private const string apiKey = "d02f9f216395b2b8d82b34dd367f5a62";
        public MaxWeather(string location)
        {
            this.OnStart();
            OpenWeatherApiClient = new OpenWeatherAPI.OpenWeatherApiClient(apiKey);
            Location = location;
        }

        public async void GetWeather()
        {
            try
            {
                App.GetEngine().BrainEngine.Log($"Getting Weather ...");
                App.GetEngine().BrainEngine.Log($"Getting Weather in {Location}...");
                var query = await OpenWeatherApiClient.QueryAsync(Location);
                App.GetEngine().BrainEngine.Log($"Results : Location:{query.Name} | Temperature_Celcius:{query.Main.Temperature.CelsiusCurrent} | Temperature_Fahrenheit:{query.Main.Temperature.FahrenheitCurrent} |  Humidity:{query.Main.Humidity} | Wind:{query.Wind.SpeedMetersPerSecond}");
                string data = $"{{!salutation}}, the temperature in {query.Name} is currently {query.Main.Temperature.CelsiusCurrent} °C";
                OnFinished();
                App.GetEngine().VoiceOutputEngine.Speak(data);
            } 
            catch(Exception e)
            {
                OnFinished();
                string data = $"{{!salutation}}, I encountered and error while testing. It say's {e.Message}";
                App.GetEngine().VoiceOutputEngine.Speak(data);
            }
            
        }

        public override void StartService()
        {
            GetWeather();
        }
    }
}
