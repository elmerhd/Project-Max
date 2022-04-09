using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace Max
{
    public class MaxAlarm : MaxService
    {
        public Timer MaxAlarmTimer { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime Now { get; set; }

        public bool IsPlayingSound = false;

        public static string fileName = "alarms.maxai";

        public FileInfo FileInfo { get; set; }

        public bool Save { get; set; }

        public MaxAlarm(MaxEngine maxEngine, MaxUI maxUI, DateTime dateTime, bool save) : base(maxEngine, maxUI)
        {
            this.OnStart(false);
            this.DateTime = dateTime;
            this.FileInfo = new FileInfo(fileName);
            this.Save = save;
            this.CheckAlarmFile();
            this.MaxAlarmTimer = new Timer();
            this.MaxAlarmTimer.Interval = 1000;
            this.MaxAlarmTimer.Elapsed += Timer_Elapsed;
        }

        public void CheckAlarmFile()
        {
            if (!this.FileInfo.Exists)
            {
                this.FileInfo.Create();
            }
        }

        public void SaveAlarm()
        {
            if (this.Save)
            {
                File.AppendAllText(fileName, DateTime.ToString("dd/MM/yyyy hh:mm tt") + Environment.NewLine);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string data = string.Empty;
            this.Now = DateTime.Now;
            if (Now.Month == DateTime.Month && Now.Day == DateTime.Day && Now.Year == DateTime.Year && Now.Hour == DateTime.Hour && Now.Minute == DateTime.Minute)
            {
                this.Log("Alarm : Playing alarm sound");
                if (!IsPlayingSound)
                {
                    MaxUtils.PlayAlarmSound();
                    IsPlayingSound = true;
                    MaxAlarmTimer.Stop();
                    this.OnFinished("");
                }
            }

        }

        public override void StartService()
        {
            MaxAlarmTimer.Start();
            SaveAlarm();
        }

        public static List<MaxAlarm> GetAlarms()
        {
            List<MaxAlarm> alarms = new List<MaxAlarm>();
            foreach (string line in File.ReadLines($"{fileName}"))
            {
                if (!string.IsNullOrEmpty(line.Trim()))
                {
                    DateTime dt = DateTime.ParseExact(line, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
                    if (dt > DateTime.Now)
                    {
                        alarms.Add(new MaxAlarm(App.GetEngine(), App.GetUI(), dt, false));
                    }
                }
            }
            return alarms;
        }
    }
}
