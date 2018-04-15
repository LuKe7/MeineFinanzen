// 03.04.2018   -Model-  CollWertpapSynchro.cs 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace MeineFinanzen.Model {
    // ObservableCollection: Stellt eine dynamische Datenauflistung dar,
    // die Benachrichtigungen bereitstellt,
    // wenn Elemente hinzugefügt oder entfernt werden
    // bzw. wenn die gesamte Liste aktualisiert wird.
    // public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    public class CollWertpapSynchro : ObservableCollection<WertpapSynchro>, IComparable {
        public CollWertpapSynchro() { }
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            WertpapSynchro wp = obj as WertpapSynchro;
            if (wp != null)
                return wp.WPSISIN.CompareTo(((WertpapSynchro)obj).WPSISIN);
            throw new NotImplementedException();
        }
    }
    public class WertpapSynchro : INotifyPropertyChanged, IEditableObject {
        public float WPSAnzahl { get; set; }
        public string WPSName { get; set; }
        public string WPSURL { get; set; }
        public DateTime WPSKursZeit { get; set; }
        public string WPSISIN { get; set; }
        private double _WPSKurs;
        public double WPSKurs {
            get { return _WPSKurs; }
            set {
                _WPSKurs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPSKurs"));
            }
        }
        public double WPSProzentAenderung { get; set; }
        public int WPSType { get; set; }
        public Single WPSSharpe { get; set; }
        public string WPXPathKurs { get; set; }
        public string WPXPathAend { get; set; }
        public string WPXPathZeit { get; set; }
        public string WPXPathSharp { get; set; }
        public string WPURLSharp { get; set; }
        public string WPSColor { get; set; }
        public string WPSVorgabeInt2 { get; set; }
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