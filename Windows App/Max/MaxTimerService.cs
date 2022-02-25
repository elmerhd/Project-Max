using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Max
{
    public class MaxTimerService : MaxService
    {
        public Timer MaxTimerTimer { get; set; }

        public bool IsPlayingSound = false;
        public MaxTimerService(MaxEngine maxEngine, MaxUI maxUI, TimeSpan timeSpan) : base(maxEngine, maxUI)
        {
            this.OnStart(false);
            MaxTimerTimer = new Timer();
            MaxTimerTimer.Interval = timeSpan.TotalMilliseconds;
            MaxTimerTimer.Elapsed += AlarmTimer_Elapsed;
        }

        private void AlarmTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Log("Timer : Playing timer sound");
            if (!IsPlayingSound)
            {
                MaxUtils.PlayAlarmSound();
                IsPlayingSound = true;
                MaxTimerTimer.Stop();
            }

        }

        public override void StartService()
        {
            MaxTimerTimer.Start();
        }

    }
}
