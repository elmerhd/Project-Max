using System.Speech.Synthesis;

namespace Max
{
    public class VoiceEngine
    {
        private MaxEngine MaxEngine;

        public SpeechSynthesizer speechSynthesizer { get; set; }


        public VoiceEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SelectVoice("IVONA 2 Kimberly");
            speechSynthesizer.Volume = 100;
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
            speechSynthesizer.Speak(textString);
        }
    }
}
