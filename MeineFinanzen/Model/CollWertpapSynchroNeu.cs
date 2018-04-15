// 14.04.2018   -Model-  CollWertpapSynchroNeu.cs 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace MeineFinanzen.Model {
    // ObservableCollection: Stellt eine dynamische Datenauflistung dar,
    // die Benachrichtigungen bereitstellt,
    // wenn Elemente hinzugefügt oder entfernt werden
    // bzw. wenn die gesamte Liste aktualisiert wird.
    // public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    public class CollWertpapSynchroNeu : ObservableCollection<WertpapSynchroNeu>, IComparable {
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            WertpapSynchroNeu wp = obj as WertpapSynchroNeu;
            if (wp != null)
                return wp.WPVISIN.CompareTo(((WertpapSynchroNeu)obj).WPVISIN);
            throw new NotImplementedException();
        }
    }
    public class WertpapSynchroNeu : INotifyPropertyChanged, IEditableObject {
        // private
        private float _WPVAnzahl;
        //private string _WPVName;
        //private string _WPVURL;
        //private DateTime _WPVKursZeit;
        //private string _WPVISIN;
        private double _WPVKurs;
        private double _WPVKursNeu;
        //private double _WPVProzentAenderung;
        //private int _WPVType; 
        private Single _WPVSharpe { get; set; }
        private string _WPVURLSharp { get; set; }
        private string _WPVColor { get; set; }
        // public
        public float WPVAnzahl {
            get { return _WPVAnzahl; }
            set {
                _WPVAnzahl = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVAnzahl"));
            }    }        
        public string WPVName { get; set; }
        public string WPVURL { get; set; }
        public string WPVISIN { get; set; }                    
        public double WPVKurs {
            get { return _WPVKurs; }
            set {
                _WPVKurs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKurs"));
            }
        }
        public double WPVKursNeu {
            get { return _WPVKursNeu; }
            set {
                _WPVKursNeu = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPVKursNeu"));
            }
        }
        public double WPVProzentAenderung { get; set; }
        public double WPVProzentAenderungNeu { get; set; }
        public DateTime WPVKursZeit { get; set; }
        public DateTime WPVKursZeitNeu { get; set; }
        public Single WPVSharpe { get; set; }
        public Single WPVSharpeNeu { get; set; }
        public int WPVType { get; set; }              
        public string WPVURLSharp { get; set; }
        public string WPVColor { get; set; }       
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }
}