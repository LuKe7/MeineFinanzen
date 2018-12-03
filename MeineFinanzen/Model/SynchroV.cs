using System;
using System.ComponentModel;
using System.Windows.Media;
using ProjSynIntNS.Model;
namespace MeineFinanzen.Model {
    public class SynchroV : INotifyPropertyChanged {
        public float WPVAnzahl { get; set; }
        public string WPVName { get; set; }
        public string WPVISIN { get; set; }
        public Wertpapierklasse WPVType { get; set; }
        public DateTime WPVKursZeit { get; set; }
        public double WPVAktWert { get; set; }
        private Single _WPVKurs;
        public Single WPVKurs {
            get { return _WPVKurs; }
            set {
                _WPVKurs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKurs"));
            }
        }
        public Single WPVProzentAenderung { get; set; }
        public Single WPVSharpe { get; set; }
        public string WPVURL { get; set; }
        public string WPVRowColor { get; set; }
        public string WPVBemerkung { get; set; }
        public string WPVSort { get; set; }
        public Brush WPVForegroundColor { get; set; }
        public bool WPVAnzeigen { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }     
    }
}