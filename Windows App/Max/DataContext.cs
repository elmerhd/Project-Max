using System.ComponentModel;

namespace Max
{
    public class DataContext : INotifyPropertyChanged
    {
        private int _SpeakerMasterPeakValue;
        private int _MicMasterPeakValue;
        private int _CenterXY;
        private bool _MicrophoneOn;

        public int SpeakerMasterPeakValue
        {
            get { return _SpeakerMasterPeakValue; }
            set { _SpeakerMasterPeakValue = value; OnPropertyChanged("SpeakerMasterPeakValue"); }
        }

        public int MicMasterPeakValue
        {
            get { return _MicMasterPeakValue; }
            set { _MicMasterPeakValue = value; OnPropertyChanged("MicMasterPeakValue"); }
        }
        public int CenterXY
        {
            get { return _CenterXY; }
            set { _CenterXY = value; OnPropertyChanged("CenterXY"); }
        }

        public bool MicrophoneOn
        {
            get { return _MicrophoneOn; }
            set { _MicrophoneOn = value; OnPropertyChanged("MicrophoneOn"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}