using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Max
{
    public class MaxTimer : MaxService
    {
        public Timer MaxTimerTimer { get; set; }

        public bool IsPlayingSound = false;
        public MaxTimer(MaxEngine maxEngine, MaxUI maxUI, TimeSpan timeSpan) : base(maxEngine, maxUI)
        {
            this.OnStart(false);
            MaxTimerTimer = new Timer();
            MaxTimerTimer.Interval = timeSpan.TotalMilliseconds;
            MaxTimerTimer.Elapsed += AlarmTimer_Elapsed;
            this.Log($"Starting Service: {nameof(MaxTimer)}");
        }

        private void AlarmTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Log($"{nameof(MaxTimer)}: Playing timer sound");
            if (!IsPlayingSound)
            {
                MaxUtils.PlayAlarmSound();
                IsPlayingSound = true;
                MaxTimerTimer.Stop();
                this.OnFinished();
            }

        }

        public override void StartService()
        {
            MaxTimerTimer.Start();
        }

    }
}
