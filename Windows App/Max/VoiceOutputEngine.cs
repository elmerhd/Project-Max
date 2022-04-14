using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Sockets;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Max
{
    public class VoiceOutputEngine
    {
        private MaxEngine MaxEngine;

        public SpeechSynthesizer SpeechSynthesizer { get; set; }

        public VoiceOutputEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            SpeechSynthesizer = new SpeechSynthesizer();
            SpeechSynthesizer.SelectVoice(MaxEngine.MaxConfig.Voice);
            SpeechSynthesizer.Volume = 100;
            MaxEngine.BrainEngine.Log($"Loading Engine: {nameof(VoiceOutputEngine)}");

        }
        public void Speak(string text)
        {
            if (text.Contains("{!alarm_time}"))
            {
                text = text.Replace("{!alarm_time}", MaxUtils.DecodedDateTime.ToString("hh:mm"));
            }
            if(text.Contains("{!salutation}"))
            {
                text = text.Replace("{!salutation}", this.MaxEngine.MaxConfig.DefaultSalutation);
            }

            if (App.GetUI() != null)
            {
                App.GetUI().UpdateResponseText(text);
            }
            MaxEngine.BrainEngine.Log($"{nameof(VoiceOutputEngine)}: Speaking {text}");
            SpeechSynthesizer.Speak(text);
        }

        public void SpeakAsync(string text)
        {
            if (text.Contains("{!alarm_time}"))
            {
                text = text.Replace("{!alarm_time}", MaxUtils.DecodedDateTime.ToString("hh:mm"));
            }
            if (text.Contains("{!salutation}"))
            {
                text = text.Replace("{!salutation}", this.MaxEngine.MaxConfig.DefaultSalutation);
            }
            if (App.GetUI() != null)
            {
                App.GetUI().UpdateResponseText(text);
            }
            MaxEngine.BrainEngine.Log($"{nameof(VoiceOutputEngine)}: Speaking Async {text}");
            SpeechSynthesizer.SpeakAsync(text);
        }
    }
}
