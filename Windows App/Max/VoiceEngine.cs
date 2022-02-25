using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace Max
{
    public class VoiceEngine
    {
        private MaxEngine MaxEngine;

        public SpeechSynthesizer SpeechSynthesizer { get; set; }

        public VoiceEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            SpeechSynthesizer = new SpeechSynthesizer();
            SpeechSynthesizer.SelectVoice(MaxEngine.MaxConfig.Voice);
            SpeechSynthesizer.Volume = 100;
            maxEngine.BrainEngine.Log($"Loading {nameof(VoiceEngine)}");
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
            SpeechSynthesizer.Speak(text);
        }
    }
}
