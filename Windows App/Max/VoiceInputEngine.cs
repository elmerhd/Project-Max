using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media;
using Windows.Media.SpeechRecognition;

namespace Max
{
    public class VoiceInputEngine
    {
        private MaxEngine MaxEngine;

        public SpeechRecognizer SpeechRecognizer { get; set; }

        public VoiceInputEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            MaxEngine.BrainEngine.Log($"Loading Engine: {nameof(VoiceInputEngine)}");
            Task.Run(async () => { await InitializingVoiceRecognition(SpeechRecognizer.SystemSpeechLanguage); });
        }

        private async Task InitializingVoiceRecognition(Language recognizerLanguage)
        {
            MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)}: Starting Voice Recognition");
            if (SpeechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                SpeechRecognizer.StateChanged -= SpeechRecognizer_Call_StateChanged;
                SpeechRecognizer.ContinuousRecognitionSession.Completed -= Continuous_Call_RecognitionSession_Completed;
                SpeechRecognizer.ContinuousRecognitionSession.ResultGenerated -= Continuous_Call_RecognitionSession_ResultGenerated;
                this.SpeechRecognizer.Dispose();
                this.SpeechRecognizer = null;
            }
            try
            {
                SpeechRecognizer = new SpeechRecognizer(recognizerLanguage);
                SpeechRecognizer.StateChanged += SpeechRecognizer_Call_StateChanged;
                var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                SpeechRecognizer.Constraints.Add(dictationConstraint);
                SpeechRecognitionCompilationResult result = await SpeechRecognizer.CompileConstraintsAsync();
                if (result.Status != SpeechRecognitionResultStatus.Success)
                {
                    MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)} : Unable to compile grammar.");
                }
                else
                {
                    SpeechRecognizer.ContinuousRecognitionSession.Completed += Continuous_Call_RecognitionSession_Completed;
                    SpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += Continuous_Call_RecognitionSession_ResultGenerated;
                }
                await SpeechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
            catch (Exception ex)
            {
                MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)}: \tError: {ex.Message}");
            }
            
        }

        private void Continuous_Call_RecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // The garbage rule will not have a tag associated with it, the other rules will return a string matching the tag provided
            // when generating the grammar.
            string tag = "unknown";
            if (args.Result.Constraint != null)
            {
                tag = args.Result.Constraint.Tag;
            }
            MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)}: \tDetected: {args.Result.Text}\t Tag: {tag}\t Confidence: {args.Result.Confidence.ToString()}");
            if (args.Result.Confidence == SpeechRecognitionConfidence.Low ||  args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                if (Regex.Replace(args.Result.Text, @"[^0-9a-zA-Z\.,_]", string.Empty).ToLower() == "heymax") {
                    MaxEngine.VoiceOutputEngine.SpeakAsync(MaxEngine.MaxConfig.DefaultListeningMessages[new Random().Next(MaxEngine.MaxConfig.DefaultListeningMessages.Count)]);
                    App.IsReadyForListening = true;
                }
                if(App.IsReadyForListening)
                {
                    bool hasTime = MaxUtils.CheckHasTime(args.Result.Text);
                    if (hasTime)
                    {
                        MaxEngine.BrainEngine.Analyze(args.Result.Text, hasTime);
                    }
                    else
                    {
                        MaxEngine.BrainEngine.Analyze(args.Result.Text);
                    }
                }
            }
            else
            {
                MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)}: Sorry, I didn't catch that. (Heard: '{args.Result.Text}', Tag: {tag}, Confidence: {args.Result.Confidence.ToString()})");
            }
        }

        private async void Continuous_Call_RecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)}: Session Completed");
                await InitializingVoiceRecognition(SpeechRecognizer.SystemSpeechLanguage);
            }
        }

        private async void SpeechRecognizer_Call_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            App.GetUI().UpdateStatusText(args.State.ToString());
            MaxEngine.BrainEngine.Log($"{nameof(VoiceInputEngine)} : Call State:\t{args.State.ToString()}");
            if (args.State == SpeechRecognizerState.Idle)
            {
                await InitializingVoiceRecognition(SpeechRecognizer.SystemSpeechLanguage);
            }
        }
    }
}
