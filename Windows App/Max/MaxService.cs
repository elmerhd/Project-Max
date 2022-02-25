using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max
{
    public abstract class MaxService
    {
        public MaxEngine MaxEngine { get; set; }
        public MaxUI MaxUI { get; set; }
        public bool PlayLoadingMusic = true;
        public MaxService(MaxEngine maxEngine, MaxUI maxUI)
        {
            MaxEngine = maxEngine;
            MaxUI = maxUI;
        }

        public void OnStart()
        {
            Type type = this.GetType().UnderlyingSystemType;
            String className = type.Name;
            this.Log($"Starting Service : {className}");
            if (PlayLoadingMusic)
            {
                MaxUtils.PlayWaitingSound();
            }
        }
        public void OnStart(bool playWaitingSound)
        {
            PlayLoadingMusic = playWaitingSound;
            this.OnStart();
        }

        public void OnFinished(string data)
        {
            Type type = this.GetType().UnderlyingSystemType;
            String className = type.Name;
            Log($"Finishing Service : {className}");
            if (PlayLoadingMusic)
            {
                MaxUtils.StopWaitingSound();
            }
            if (!string.IsNullOrEmpty(data))
            {
                this.Speak(data);
            }
        }

        public void Log(string data)
        {
            MaxEngine.BrainEngine.Log(data);
        }

        public void Speak(string data)
        {
            MaxEngine.VoiceEngine.Speak(data);
        }
        public abstract void StartService();
    }
}
