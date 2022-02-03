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
            SpeechSynthesizer.SelectVoice("IVONA 2 Kimberly");
            SpeechSynthesizer.Volume = 100;

            
            maxEngine.BrainEngine.Log($"Loading {nameof(VoiceEngine)}");
        }

        public void Speak(string text)
        {
            string textString;
            if(text.Contains("{!salutation}"))
            {
                textString = text.Replace("{!salutation}", this.MaxEngine.MaxConfig.DefaultSalutation);
            }
            else
            {
                textString = text;
            }

            if (App.GetUI() != null)
            {
                App.GetUI().UpdateResponseText(textString);
            }
            SpeechSynthesizer.Speak(textString);
        }
    }
}
